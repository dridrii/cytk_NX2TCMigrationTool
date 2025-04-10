using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;

namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    public partial class LogViewerForm : Form
    {
        private string _currentLogFile;
        private DateTime _lastModified = DateTime.MinValue;

        public LogViewerForm()
        {
            InitializeComponent();

            // Hook up event handlers manually after InitializeComponent
            SetupEventHandlers();

            // Load data
            LoadLogFiles();
            LoadCurrentLog();
        }

        private void SetupEventHandlers()
        {
            // Set up event handlers for controls
            _logFileComboBox.SelectedIndexChanged += OnLogFileComboBoxSelectedIndexChanged;
            _logLevelComboBox.SelectedIndexChanged += OnLogLevelComboBoxSelectedIndexChanged;
            _autoRefreshCheckBox.CheckedChanged += OnAutoRefreshCheckBoxCheckedChanged;
            _refreshButton.Click += OnRefreshButtonClick;
            _clearButton.Click += OnClearButtonClick;
            _closeButton.Click += OnCloseButtonClick;

            // Set up auto refresh timer
            _refreshTimer = new System.Windows.Forms.Timer(components);
            _refreshTimer.Interval = 1000; // 1 second
            _refreshTimer.Tick += OnRefreshTimerTick;
            _refreshTimer.Start();
        }

        private void OnLogFileComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCurrentLog();
        }

        private void OnLogLevelComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            LoadCurrentLog();
        }

        private void OnAutoRefreshCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            _refreshTimer.Enabled = _autoRefreshCheckBox.Checked;
        }

        private void OnRefreshButtonClick(object sender, EventArgs e)
        {
            LoadCurrentLog();
        }

        private void OnClearButtonClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("Clear the log viewer? This does not delete the log file.",
                "Confirm Clear", MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _logTextBox.Clear();
            }
        }

        private void OnCloseButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnRefreshTimerTick(object sender, EventArgs e)
        {
            RefreshLog();
        }

        private void LoadLogFiles()
        {
            _logFileComboBox.Items.Clear();

            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");

            if (Directory.Exists(logDir))
            {
                string[] logFiles = Directory.GetFiles(logDir, "nx2tc_log_*.log");
                Array.Sort(logFiles); // Sort alphabetically (will be chronological due to naming)

                foreach (string file in logFiles)
                {
                    _logFileComboBox.Items.Add(Path.GetFileName(file));
                }

                // Select the most recent log (last in the sorted list)
                if (_logFileComboBox.Items.Count > 0)
                {
                    _logFileComboBox.SelectedIndex = _logFileComboBox.Items.Count - 1;
                }

                // Also add the current log directly from the logger
                if (!string.IsNullOrEmpty(Logger.Instance.LogFilePath))
                {
                    string currentLogName = Path.GetFileName(Logger.Instance.LogFilePath);

                    if (!_logFileComboBox.Items.Contains(currentLogName))
                    {
                        _logFileComboBox.Items.Add(currentLogName);
                        _logFileComboBox.SelectedIndex = _logFileComboBox.Items.Count - 1;
                    }
                }
            }
        }

        private void LoadCurrentLog()
        {
            if (_logFileComboBox.SelectedItem == null)
            {
                return;
            }

            string selectedLogFile = _logFileComboBox.SelectedItem.ToString();
            string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs", selectedLogFile);

            _currentLogFile = logPath;

            if (File.Exists(logPath))
            {
                try
                {
                    // Get current cursor position before refreshing
                    int selectionStart = _logTextBox.SelectionStart;
                    int selectionLength = _logTextBox.SelectionLength;

                    // Get the selected log level
                    LogLevel selectedLevel = (LogLevel)_logLevelComboBox.SelectedIndex;

                    // Read log file
                    string[] lines = File.ReadAllLines(logPath);
                    List<string> filteredLines = new List<string>();

                    foreach (string line in lines)
                    {
                        // Check if the line matches the selected log level
                        if (selectedLevel != LogLevel.None && line.Contains("["))
                        {
                            // Extract log level from the line
                            int start = line.IndexOf('[') + 1;
                            int end = line.IndexOf(']', start);

                            if (start > 0 && end > start)
                            {
                                string levelString = line.Substring(start, end - start).Trim();

                                // Skip timestamp bracket
                                start = line.IndexOf('[', end) + 1;
                                end = line.IndexOf(']', start);

                                if (start > 0 && end > start)
                                {
                                    levelString = line.Substring(start, end - start).Trim();

                                    if (Enum.TryParse<LogLevel>(levelString, out LogLevel lineLevel))
                                    {
                                        // Skip if line level is less than selected level
                                        if (lineLevel < selectedLevel)
                                        {
                                            continue;
                                        }
                                    }
                                }
                            }
                        }

                        filteredLines.Add(line);
                    }

                    _logTextBox.Text = string.Join(Environment.NewLine, filteredLines);

                    // Record the last modified time
                    _lastModified = File.GetLastWriteTime(logPath);

                    // Restore cursor position or scroll to end if new content
                    if (selectionStart < _logTextBox.Text.Length)
                    {
                        _logTextBox.SelectionStart = selectionStart;
                        _logTextBox.SelectionLength = selectionLength;
                    }
                    else
                    {
                        _logTextBox.SelectionStart = _logTextBox.Text.Length;
                        _logTextBox.ScrollToCaret();
                    }
                }
                catch (Exception ex)
                {
                    _logTextBox.Text = $"Error loading log file: {ex.Message}";
                }
            }
            else
            {
                _logTextBox.Text = "Log file not found.";
            }
        }

        private void RefreshLog()
        {
            // Only refresh if auto-refresh is enabled and the current log file exists
            if (_autoRefreshCheckBox.Checked && !string.IsNullOrEmpty(_currentLogFile) && File.Exists(_currentLogFile))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(_currentLogFile);

                // Only reload if the file has been modified
                if (lastWriteTime > _lastModified)
                {
                    LoadCurrentLog();
                }
            }
        }
    }
}