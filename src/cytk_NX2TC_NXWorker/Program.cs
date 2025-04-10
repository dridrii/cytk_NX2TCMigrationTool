using System;
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

                if (args.Length < 2)
                {
                    Console.WriteLine("Usage: run_dotnet_nxopen cytk_NX2TC_NXWorker.exe <pipeName> <command>");
                    return 1;
                }

                string pipeName = args[0];
                string command = args[1];

                Console.WriteLine($"Starting NX Worker with pipe: {pipeName}, command: {command}");

                // Connect to the named pipe
                using (var pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut))
                {
                    Console.WriteLine($"Waiting for connection on pipe: {pipeName}");
                    pipeServer.WaitForConnection();
                    Console.WriteLine("Client connected");

                    using (var reader = new StreamReader(pipeServer, leaveOpen: true))
                    using (var writer = new StreamWriter(pipeServer, leaveOpen: true))
                    {
                        // Read the request
                        string requestJson = reader.ReadLine();
                        dynamic request = JsonConvert.DeserializeObject(requestJson);

                        // Process the command
                        object result = null;
                        bool success = true;
                        string errorMessage = null;

                        try
                        {
                            switch (command)
                            {
                                case "AnalyzePartType":
                                    string filePath = request.Parameters.ToString();
                                    result = AnalyzePartType(filePath);
                                    break;
                                case "IsAssemblyByStructure":
                                    filePath = request.Parameters.ToString();
                                    result = IsAssemblyByStructure(filePath);
                                    break;
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

                theProgram.Dispose();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.Error.WriteLine(ex.StackTrace);
                retValue = 1;
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
        private static Dictionary<string, bool> AnalyzePartType(string filePath)
        {
            Console.WriteLine($"Analyzing part type: {filePath}");

            Dictionary<string, bool> result = new Dictionary<string, bool>
            {
                { "IsPart", false },
                { "IsAssembly", false },
                { "IsDrafting", false },
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
                        result["IsPartFamilyMaster"] = false;
                    }

                    // Check if it's a part family member
                    try
                    {
                        Tag partTag = basePart.Tag;
                        Tag parentTemplateTag = Tag.Null;
                        theUfSession.Part.AskPartFamilyTemplateTag(partTag, out parentTemplateTag);
                        bool isPartFamilyMember = parentTemplateTag != Tag.Null;
                        result["IsPartFamilyMember"] = isPartFamilyMember;
                        Console.WriteLine($"Is family member: {isPartFamilyMember}");
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine($"Error checking family member: {ex.Message}");
                        result["IsPartFamilyMember"] = false;
                    }

                    // Check if it's a drafting
                    if (basePart is Part part)
                    {
                        bool isDrafting = false;
                        try
                        {
                            isDrafting = part.Prototype == BasePart.PrototypeType.Drafting ||
                                       (part.DrawingSheets != null && part.DrawingSheets.Count > 0);
                            result["IsDrafting"] = isDrafting;
                            Console.WriteLine($"Is drafting: {isDrafting}");
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Error checking drafting: {ex.Message}");
                            result["IsDrafting"] = false;
                        }

                        // Check if it's an assembly
                        try
                        {
                            bool isAssembly = part.Prototype == BasePart.PrototypeType.Assembly;

                            // Also check for components
                            try
                            {
                                if (part.ComponentAssembly != null &&
                                    part.ComponentAssembly.RootComponent != null &&
                                    part.ComponentAssembly.RootComponent.GetChildren() != null &&
                                    part.ComponentAssembly.RootComponent.GetChildren().Length > 0)
                                {
                                    isAssembly = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.Error.WriteLine($"Error checking components: {ex.Message}");
                            }

                            result["IsAssembly"] = isAssembly;
                            Console.WriteLine($"Is assembly: {isAssembly}");

                            // If none of the special types, it's a simple part
                            if (!isAssembly && !isDrafting &&
                                !result["IsPartFamilyMaster"] && !result["IsPartFamilyMember"])
                            {
                                result["IsPart"] = true;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Error checking assembly: {ex.Message}");
                            // Default to part if we can't determine
                            result["IsPart"] = true;
                        }
                    }

                    // Close the part
                    try
                    {
                        theSession.Parts.CloseBase(basePart, Part.CloseBaseOptions.DestroyWindow, null);
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
                Console.Error.WriteLine($"Error analyzing part: {ex.Message}");
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
                        // Check if it's defined as an assembly type
                        if (part.Prototype == BasePart.PrototypeType.Assembly)
                        {
                            Console.WriteLine("Part is an assembly (by prototype)");
                            return true;
                        }

                        // Check if it has components
                        try
                        {
                            if (part.ComponentAssembly != null &&
                                part.ComponentAssembly.RootComponent != null)
                            {
                                Component[] components = part.ComponentAssembly.RootComponent.GetChildren();
                                if (components != null && components.Length > 0)
                                {
                                    Console.WriteLine($"Part is an assembly (has {components.Length} components)");
                                    return true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Error checking components: {ex.Message}");
                        }
                    }

                    // Close the part
                    try
                    {
                        theSession.Parts.CloseBase(basePart, Part.CloseBaseOptions.DestroyWindow, null);
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