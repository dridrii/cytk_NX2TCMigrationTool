using System;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Collections.Generic;
using Newtonsoft.Json;
using NXOpen;
using NXOpen.UF;

namespace cytk_NX2TC_NXWorker
{
    // class members
    public class Program
    {
        private static Session theSession;
        private static UI theUI;
        private static UFSession theUfSession;
        public static Program theProgram;
        public static bool isDisposeCalled;

        //------------------------------------------------------------------------------
        // Constructor
        //------------------------------------------------------------------------------
        public Program()
        {
            try
            {
                theSession = Session.GetSession();
                theUI = UI.GetUI();
                theUfSession = UFSession.GetUFSession();
                isDisposeCalled = false;
            }
            catch (NXOpen.NXException ex)
            {
                Console.Error.WriteLine($"NX Exception: {ex.Message}");
            }
        }

        //------------------------------------------------------------------------------
        //  Explicit Activation
        //      This entry point is used to activate the application explicitly
        //------------------------------------------------------------------------------
        public static int Main(string[] args)
        {
            int retValue = 0;
            try
            {
                theProgram = new Program();

                if (args.Length < 1)
                {
                    Console.WriteLine("Usage: run_dotnet_nxopen cytk_NX2TC_NXWorker.exe <pipeName>");
                    return 1;
                }

                string pipeName = args[0];
                Console.WriteLine($"Starting NX Worker with pipe: {pipeName}");
                Console.WriteLine("Worker is waiting for commands. Press Ctrl+C to exit.");

                // Run in a continuous loop
                while (true)
                {
                    // Connect to the named pipe
                    using (var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut))
                    {
                        Console.WriteLine($"Waiting for connection on pipe: {pipeName}");
                        pipeServer.WaitForConnection();
                        Console.WriteLine("Client connected");

                        using (var reader = new StreamReader(pipeServer, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true))
                        using (var writer = new StreamWriter(pipeServer, Encoding.UTF8, bufferSize: 1024, leaveOpen: true))
                        {
                            // Read the request
                            string requestJson = reader.ReadLine();
                            dynamic request = JsonConvert.DeserializeObject(requestJson);
                            string command = request.Command;

                            Console.WriteLine($"Received command: {command}");

                            // Process the command
                            object result = null;
                            bool success = true;
                            string errorMessage = null;

                            try
                            {
                                switch (command)
                                {
                                    case "AnalyzePartFamilyType":
                                        string filePath = request.Parameters.ToString();
                                        result = AnalyzePartFamilyType(filePath);
                                        break;
                                    case "Exit":
                                        Console.WriteLine("Exit command received. Shutting down worker.");
                                        // Make sure to clean up and close any open parts
                                        try
                                        {
                                            Part workPart = theSession.Parts.Work;
                                            if (workPart != null)
                                            {
                                                //theSession.Parts.CloseAll(BasePart.CloseModified, PartCloseResponses true);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.Error.WriteLine($"Error closing parts: {ex.Message}");
                                        }
                                        theProgram.Dispose();
                                        return 0; // Exit the application
                                    default:
                                        success = false;
                                        errorMessage = $"Unknown command: {command}";
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                success = false;
                                errorMessage = ex.Message;
                                Console.Error.WriteLine($"Error executing command: {ex.Message}");
                                Console.Error.WriteLine(ex.StackTrace);
                            }

                            // Create and send the response
                            var response = new
                            {
                                Success = success,
                                ErrorMessage = errorMessage,
                                Data = result
                            };

                            string responseJson = JsonConvert.SerializeObject(response);
                            writer.WriteLine(responseJson);
                            writer.Flush();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                retValue = 1;
            }
            finally
            {
                theProgram?.Dispose();
            }
            return retValue;
        }

        //------------------------------------------------------------------------------
        // Following method disposes all the class members
        //------------------------------------------------------------------------------
        public void Dispose()
        {
            try
            {
                if (isDisposeCalled == false)
                {
                    // Clean up any resources
                }
                isDisposeCalled = true;
            }
            catch (NXOpen.NXException ex)
            {
                Console.Error.WriteLine($"Error disposing: {ex.Message}");
            }
        }

        //------------------------------------------------------------------------------
        // Analyze the part type using NX Open API
        //------------------------------------------------------------------------------
        private static Dictionary<string, bool> AnalyzePartFamilyType(string filePath)
        {
            Console.WriteLine($"Analyzing part family type: {filePath}");

            Dictionary<string, bool> result = new Dictionary<string, bool>
    {
        { "IsPartFamilyMaster", false },
        { "IsPartFamilyMember", false }
    };

            try
            {
                // Load the part
                PartLoadStatus partLoadStatus;
                BasePart basePart = theSession.Parts.OpenBase(filePath, out partLoadStatus);

                if (basePart != null)
                {
                    Console.WriteLine($"Successfully loaded part: {basePart.FullPath}");

                    // Check if it's a part family master
                    try
                    {
                        Tag partTag = basePart.Tag;
                        bool isFamilyTemplate = false;
                        theUfSession.Part.IsFamilyTemplate(partTag, out isFamilyTemplate);
                        result["IsPartFamilyMaster"] = isFamilyTemplate;
                        Console.WriteLine($"Is family template: {isFamilyTemplate}");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error checking family template: {ex.Message}");
                    }

                    // Check if it's a part family member
                    try
                    {
                        // Implementation for checking if this is a part family member
                        // This will depend on the specific NX API available
                        bool isFamilyMember = false;
                        // Example (placeholder - replace with actual API call):
                        // theUfSession.Part.IsFamilyMember(partTag, out isFamilyMember);
                        result["IsPartFamilyMember"] = isFamilyMember;
                        Console.WriteLine($"Is family member: {isFamilyMember}");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error checking family member: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error analyzing part family: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
            }

            return result;
        }

        //------------------------------------------------------------------------------
        // Check if the part is an assembly by examining its structure
        //------------------------------------------------------------------------------
        private static bool IsAssemblyByStructure(string filePath)
        {
            Console.WriteLine($"Checking if part is an assembly: {filePath}");

            try
            {
                // Load the part
                PartLoadStatus partLoadStatus;
                BasePart basePart = theSession.Parts.OpenBase(filePath, out partLoadStatus);

                if (basePart != null)
                {
                    Console.WriteLine($"Successfully loaded part: {basePart.FullPath}");

                    if (basePart is Part part)
                    {
                        //// Check if it's defined as an assembly type
                        //if (part.Prototype == BasePart.PrototypeType.Assembly)
                        //{
                        //    Console.WriteLine("Part is an assembly (by prototype)");
                        //    return true;
                        //}

                        //// Check if it has components
                        //try
                        //{
                        //    if (part.ComponentAssembly != null &&
                        //        part.ComponentAssembly.RootComponent != null)
                        //    {
                        //        Component[] components = part.ComponentAssembly.RootComponent.GetChildren();
                        //        if (components != null && components.Length > 0)
                        //        {
                        //            Console.WriteLine($"Part is an assembly (has {components.Length} components)");
                        //            return true;
                        //        }
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    Console.Error.WriteLine($"Error checking components: {ex.Message}");
                        //}
                    }

                    // Close the part
                    try
                    {
                        //theSession.Parts.CloseBase(basePart, Part.CloseBaseOptions.DestroyWindow, null);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error closing part: {ex.Message}");
                    }
                }
                else
                {
                    Console.Error.WriteLine($"Failed to load part: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error checking assembly structure: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("Part is not an assembly");
            return false;
        }

        public static int GetUnloadOption(string arg)
        {
            //Unloads the image when the NX session terminates
            return System.Convert.ToInt32(Session.LibraryUnloadOption.AtTermination);
        }
    }
}