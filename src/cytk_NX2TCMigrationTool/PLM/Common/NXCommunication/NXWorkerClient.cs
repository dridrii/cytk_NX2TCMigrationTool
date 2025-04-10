using System;
using System.IO;
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
        private const string PipeName = "cytk_NX2TC_Pipeline";

        public NXWorkerClient(string nxPath, string nxWorkerPath)
        {
            _nxPath = nxPath ?? throw new ArgumentNullException(nameof(nxPath));
            _nxWorkerPath = nxWorkerPath ?? throw new ArgumentNullException(nameof(nxWorkerPath));
            _logger = Logger.Instance;
        }

        public async Task<T> SendCommandAsync<T>(string command, object parameters)
        {
            _logger.Debug("NXWorkerClient", $"Sending command: {command}");

            // Create a request object
            var request = new NXWorkerRequest
            {
                Command = command,
                Parameters = parameters
            };

            // Serialize the request
            string requestJson = JsonConvert.SerializeObject(request);

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
                Arguments = $"\"{_nxWorkerPath}\" {PipeName} \"{command}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = new Process { StartInfo = processInfo })
            {
                // Start the worker process
                process.Start();

                // Create a named pipe to communicate with the worker
                using (var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
                {
                    try
                    {
                        // Connect to the pipe (timeout after 10 seconds)
                        await pipeClient.ConnectAsync(10000);

                        // Create StreamWriter for writing to the pipe
                        using (var writer = new StreamWriter(pipeClient, System.Text.Encoding.UTF8, 1024, leaveOpen: true))
                        {
                            // Write the request to the pipe
                            await writer.WriteLineAsync(requestJson);
                            await writer.FlushAsync();

                            // Create StreamReader for reading the response
                            using (var reader = new StreamReader(pipeClient, System.Text.Encoding.UTF8, true, 1024, leaveOpen: true))
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
                    catch (TimeoutException)
                    {
                        _logger.Error("NXWorkerClient", "Connection to NX Worker timed out");
                        throw new TimeoutException("Connection to NX Worker timed out");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("NXWorkerClient", $"Error communicating with NX Worker: {ex.Message}");
                        throw;
                    }
                }
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