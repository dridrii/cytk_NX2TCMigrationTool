using System;
using System.IO;
using System.Text;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Diagnostics;
using Newtonsoft.Json;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;

namespace cytk_NX2TCMigrationTool.src.Core.Common.NXCommunication
{
    public class NXWorkerClient
    {
        private readonly string _nxPath;
        private readonly string _nxWorkerPath;
        private readonly Logger _logger;
        private readonly string _pipeName = "cytk_NX2TC_Pipeline"; // Use a consistent pipe name

        private Process _nxWorkerProcess;
        private bool _workerStarted = false;

        public NXWorkerClient(string nxPath, string nxWorkerPath)
        {
            _nxPath = nxPath ?? throw new ArgumentNullException(nameof(nxPath));
            _nxWorkerPath = nxWorkerPath ?? throw new ArgumentNullException(nameof(nxWorkerPath));
            _logger = Logger.Instance;
        }

        public async Task StartWorkerAsync()
        {
            if (_workerStarted)
            {
                _logger.Debug("NXWorkerClient", "Worker process already started");
                return;
            }

            _logger.Debug("NXWorkerClient", "Starting worker process");

            // Run the NX Worker process
            string runDotnetNxOpen = Path.Combine(_nxPath, "NXBIN", "run_dotnet_nxopen.exe");
            if (!File.Exists(runDotnetNxOpen))
            {
                throw new FileNotFoundException($"run_dotnet_nxopen.exe not found at {runDotnetNxOpen}");
            }

            if (!File.Exists(_nxWorkerPath))
            {
                throw new FileNotFoundException($"NX Worker executable not found at {_nxWorkerPath}");
            }

            // Create the process
            var processInfo = new ProcessStartInfo
            {
                FileName = runDotnetNxOpen,
                Arguments = $"\"{_nxWorkerPath}\" {_pipeName}",
                UseShellExecute = false,
                CreateNoWindow = false, // Setting to false to allow the console to be visible during testing
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            _nxWorkerProcess = new Process { StartInfo = processInfo };

            // Set up event handlers to log worker output
            _nxWorkerProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger.Debug("NXWorker", e.Data);
            };
            _nxWorkerProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    _logger.Error("NXWorker", e.Data);
            };

            // Start the worker process
            _nxWorkerProcess.Start();
            _nxWorkerProcess.BeginOutputReadLine();
            _nxWorkerProcess.BeginErrorReadLine();

            _workerStarted = true;
            _logger.Info("NXWorkerClient", "Worker process started");

            // Wait a moment to let the worker initialize
            await Task.Delay(2000);
        }

        public async Task StopWorkerAsync()
        {
            if (!_workerStarted || _nxWorkerProcess == null)
            {
                _logger.Debug("NXWorkerClient", "No worker process to stop");
                return;
            }

            try
            {
                // Send exit command to worker
                await SendCommandAsync<object>("Exit", null);
                _logger.Debug("NXWorkerClient", "Exit command sent to worker process");

                // Wait for process to exit gracefully with a longer timeout
                if (!_nxWorkerProcess.WaitForExit(10000))  // 10 seconds timeout
                {
                    _logger.Warning("NXWorkerClient", "Worker process did not exit gracefully, forcing termination");
                    _nxWorkerProcess.Kill();
                }

                _nxWorkerProcess.Dispose();
                _nxWorkerProcess = null;
                _workerStarted = false;
                _logger.Info("NXWorkerClient", "Worker process stopped");
            }
            catch (Exception ex)
            {
                _logger.Error("NXWorkerClient", $"Error stopping worker process: {ex.Message}");

                // Ensure the process is killed in case of errors
                try
                {
                    if (_nxWorkerProcess != null && !_nxWorkerProcess.HasExited)
                        _nxWorkerProcess.Kill();

                    if (_nxWorkerProcess != null)
                        _nxWorkerProcess.Dispose();
                }
                catch (Exception killEx)
                {
                    _logger.Error("NXWorkerClient", $"Error forcing worker termination: {killEx.Message}");
                }

                _nxWorkerProcess = null;
                _workerStarted = false;

                throw; // Rethrow to be handled by caller
            }
        }

        public async Task<T> SendCommandAsync<T>(string command, object parameters)
        {
            _logger.Debug("NXWorkerClient", $"Sending command: {command}");

            // Start the worker if it's not already running
            if (!_workerStarted)
            {
                await StartWorkerAsync();
            }

            // Create a request object
            var request = new NXWorkerRequest
            {
                Command = command,
                Parameters = parameters
            };

            // Serialize the request
            string requestJson = JsonConvert.SerializeObject(request);

            try
            {
                // Create a named pipe to communicate with the worker
                using (var pipeClient = new NamedPipeClientStream(".", _pipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
                {
                    // Connect to the pipe (timeout after 10 seconds)
                    await pipeClient.ConnectAsync(10000);

                    // Create StreamWriter for writing to the pipe
                    using (var writer = new StreamWriter(pipeClient, Encoding.UTF8, bufferSize: 1024, leaveOpen: true))

                    {
                        // Write the request to the pipe
                        await writer.WriteLineAsync(requestJson);
                        await writer.FlushAsync();

                        // Create StreamReader for reading the response
                        using (var reader = new StreamReader(pipeClient, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
                        {
                            // Read the response
                            string responseJson = await reader.ReadLineAsync();

                            // Parse the response
                            var response = JsonConvert.DeserializeObject<NXWorkerResponse<T>>(responseJson);

                            // Check if the command was successful
                            if (!response.Success)
                            {
                                throw new Exception($"NX Worker command failed: {response.ErrorMessage}");
                            }

                            return response.Data;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("NXWorkerClient", $"Error communicating with NX Worker: {ex.Message}");
                throw;
            }
        }


    }

    public class NXWorkerRequest
    {
        public string Command { get; set; }
        public object Parameters { get; set; }
    }

    public class NXWorkerResponse<T>
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public T Data { get; set; }
    }
}