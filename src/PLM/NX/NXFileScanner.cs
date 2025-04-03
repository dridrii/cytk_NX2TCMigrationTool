using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;
using cytk_NX2TCMigrationTool.src.Core.Database.Models;
using cytk_NX2TCMigrationTool.src.Core.Database.Repositories;
using cytk_NX2TCMigrationTool.src.Core.Settings;

namespace cytk_NX2TCMigrationTool.src.PLM.NX
{
    public class NXFileScanner
    {
        private readonly PartRepository _partRepository;
        private readonly SettingsManager _settingsManager;
        private readonly string _salt;
        private readonly Logger _logger;

        // Event for progress reporting
        public event EventHandler<FileScanProgressEventArgs> ScanProgress;

        public NXFileScanner(PartRepository partRepository, SettingsManager settingsManager)
        {
            _partRepository = partRepository ?? throw new ArgumentNullException(nameof(partRepository));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _logger = Logger.Instance;

            // Get salt from settings
            _salt = _settingsManager.GetSetting("/Settings/Application/Salt") ?? "CYTKdefault123";

            _logger.Debug("NXFileScanner", $"Scanner initialized with salt: {_salt}");
        }

        /// <summary>
        /// Scans all configured root directories for NX part files and adds them to the database
        /// </summary>
        /// <returns>Number of files scanned and added</returns>
        public async Task<ScanResults> ScanAllRootDirectoriesAsync()
        {
            var results = new ScanResults();

            _logger.Info("NXFileScanner", "Starting scan of all root directories");

            // Get all root directories from settings
            var rootDirElements = _settingsManager.GetSettingElements("/Settings/NX/RootDirectories/Directory");

            int totalDirectories = rootDirElements.Count;
            int currentDirectory = 0;

            _logger.Info("NXFileScanner", $"Found {totalDirectories} directories to scan");

            foreach (var dirElement in rootDirElements)
            {
                string rootDir = dirElement.InnerText;
                currentDirectory++;

                _logger.Info("NXFileScanner", $"Processing directory {currentDirectory}/{totalDirectories}: {rootDir}");

                if (string.IsNullOrEmpty(rootDir))
                {
                    _logger.Warning("NXFileScanner", "Skipping empty directory path");
                    continue;
                }

                if (!Directory.Exists(rootDir))
                {
                    _logger.Error("NXFileScanner", $"Directory does not exist: {rootDir}");
                    results.Errors.Add($"Directory does not exist: {rootDir}");
                    continue;
                }

                // Process this directory
                ScanProgress?.Invoke(this, new FileScanProgressEventArgs
                {
                    CurrentOperation = $"Scanning directory {currentDirectory} of {totalDirectories}: {rootDir}",
                    OverallProgress = (int)((double)currentDirectory / totalDirectories * 100),
                    CurrentFile = string.Empty,
                    TotalFiles = 0,
                    CurrentFileNumber = 0
                });

                var dirResults = await ScanDirectoryAsync(rootDir);

                // Aggregate results
                results.FilesScanned += dirResults.FilesScanned;
                results.FilesAdded += dirResults.FilesAdded;
                results.DuplicatesFound += dirResults.DuplicatesFound;
                results.Errors.AddRange(dirResults.Errors);
            }

            _logger.Info("NXFileScanner", $"Scan complete. Files scanned: {results.FilesScanned}, " +
                                          $"Files added: {results.FilesAdded}, " +
                                          $"Duplicates: {results.DuplicatesFound}, " +
                                          $"Errors: {results.Errors.Count}");

            return results;
        }

        /// <summary>
        /// Scans a single directory for NX part files and adds them to the database
        /// </summary>
        /// <param name="rootDir">Directory to scan</param>
        /// <returns>Results of the scan</returns>
        public async Task<ScanResults> ScanDirectoryAsync(string rootDir)
        {
            var results = new ScanResults();

            try
            {
                _logger.Debug("NXFileScanner", $"Starting scan of directory: {rootDir}");

                // Get all NX part files in the directory and subdirectories
                _logger.Trace("NXFileScanner", $"Searching for .prt files in: {rootDir}");
                var files = Directory.GetFiles(rootDir, "*.prt", SearchOption.AllDirectories);

                results.FilesScanned = files.Length;
                _logger.Debug("NXFileScanner", $"Found {files.Length} .prt files to process");

                int fileNumber = 0;

                foreach (var file in files)
                {
                    fileNumber++;

                    _logger.Trace("NXFileScanner", $"Processing file {fileNumber}/{files.Length}: {file}");

                    // Report progress
                    ScanProgress?.Invoke(this, new FileScanProgressEventArgs
                    {
                        CurrentOperation = "Processing files",
                        OverallProgress = 0, // Will be calculated by caller
                        CurrentFile = file,
                        TotalFiles = files.Length,
                        CurrentFileNumber = fileNumber
                    });

                    try
                    {
                        // Process the file
                        bool wasAdded = await ProcessFileAsync(file);

                        if (wasAdded)
                        {
                            results.FilesAdded++;
                            _logger.Trace("NXFileScanner", $"Successfully added file: {file}");
                        }
                        else
                        {
                            results.DuplicatesFound++;
                            _logger.Trace("NXFileScanner", $"File is a duplicate: {file}");
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMessage = $"Error processing file {file}: {ex.Message}";
                        _logger.Error("NXFileScanner", errorMessage);
                        _logger.Debug("NXFileScanner", $"Exception details: {ex}");
                        results.Errors.Add(errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error scanning directory {rootDir}: {ex.Message}";
                _logger.Error("NXFileScanner", errorMessage);
                _logger.Debug("NXFileScanner", $"Exception details: {ex}");
                results.Errors.Add(errorMessage);
            }

            _logger.Debug("NXFileScanner", $"Directory scan complete for: {rootDir}. " +
                                           $"Scanned: {results.FilesScanned}, " +
                                           $"Added: {results.FilesAdded}, " +
                                           $"Duplicates: {results.DuplicatesFound}, " +
                                           $"Errors: {results.Errors.Count}");

            return results;
        }

        /// <summary>
        /// Process a single NX part file
        /// </summary>
        /// <param name="filePath">Path to the NX part file</param>
        /// <returns>True if file was added, false if it was a duplicate</returns>
        private async Task<bool> ProcessFileAsync(string filePath)
        {
            _logger.Trace("NXFileScanner", $"Calculating checksum for: {filePath}");

            // Generate checksum for the file
            string checksum = await Task.Run(() => FileUtils.CalculateChecksum(filePath));

            _logger.Trace("NXFileScanner", $"Checksum: {checksum} for file: {filePath}");

            // Check if this exact file has already been processed (same path and checksum)
            var parts = _partRepository.GetByChecksum(checksum);
            bool isDuplicate = false;
            string duplicateOfId = null;

            // If there are existing parts with this checksum, check if any have the same path
            foreach (var existingPart in parts)
            {
                if (existingPart.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase))
                {
                    // This is the same physical file we've already processed, skip it
                    _logger.Debug("NXFileScanner", $"File already exists in database with same path: {filePath}");
                    return false;
                }

                // If we found another file with the same checksum but different path, it's a duplicate
                isDuplicate = true;
                duplicateOfId = existingPart.Id;
                _logger.Debug("NXFileScanner", $"Found existing part with same checksum: {existingPart.Id} - {existingPart.FilePath}");
                break;
            }

            // Create a new part
            string id = FileUtils.GenerateUniqueId(_salt);
            _logger.Trace("NXFileScanner", $"Generated ID: {id} for file: {filePath}");

            var part = new Part
            {
                Id = id,
                Name = Path.GetFileNameWithoutExtension(filePath),
                Type = "NXPart",
                Source = "NX",
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Checksum = checksum,
                IsDuplicate = isDuplicate,
                DuplicateOf = duplicateOfId,
                Metadata = null
            };

            try
            {
                // Add the part to the database
                _logger.Trace("NXFileScanner", $"Adding part to database: {part.Id} - {part.FilePath}");
                _partRepository.Add(part);

                return !isDuplicate; // Return true if this was not a duplicate
            }
            catch (Exception ex)
            {
                _logger.Error("NXFileScanner", $"Database error while adding part {part.Id}: {ex.Message}");
                throw; // Re-throw to be handled by caller
            }
        }


        /// <summary>
        /// Results of a file scan operation
        /// </summary>
        public class ScanResults
        {
            /// <summary>
            /// Number of files scanned
            /// </summary>
            public int FilesScanned { get; set; }

            /// <summary>
            /// Number of files added to the database
            /// </summary>
            public int FilesAdded { get; set; }

            /// <summary>
            /// Number of duplicate files found
            /// </summary>
            public int DuplicatesFound { get; set; }

            /// <summary>
            /// List of errors that occurred during the scan
            /// </summary>
            public List<string> Errors { get; set; } = new List<string>();
        }
    }

    /// <summary>
    /// Event arguments for file scan progress reporting
    /// </summary>
    public class FileScanProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Description of the current operation
        /// </summary>
        public string CurrentOperation { get; set; }

        /// <summary>
        /// Overall progress percentage (0-100)
        /// </summary>
        public int OverallProgress { get; set; }

        /// <summary>
        /// Path to the current file being processed
        /// </summary>
        public string CurrentFile { get; set; }

        /// <summary>
        /// Total number of files to process
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// Current file number being processed
        /// </summary>
        public int CurrentFileNumber { get; set; }
    }
}