using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using cytk_NX2TCMigrationTool.src.Core.Common.Utilities;

namespace cytk_NX2TCMigrationTool.src.UI.Windows
{
    public class LogViewerForm : Form
    {
        private TextBox _logTextBox;
        private ComboBox _logLevelComboBox;
        private Button _refreshButton;
        private Button _clearButton;
        private Button _closeButton;
        private CheckBox _autoRefreshCheckBox;
        private System.Windows.Forms.Timer _refreshTimer; // Fully qualified Timer type
        private ComboBox _logFileComboBox;

        private string _currentLogFile;
        private DateTime _lastModified = DateTime.MinValue;

        public LogViewerForm()
        {
            InitializeComponent();
            LoadLogFiles();
            LoadCurrentLog();

            // Set up auto refresh timer
            _refreshTimer = new System.Windows.Forms.Timer();
            _refreshTimer.Interval = 1000; // 1 second
            _refreshTimer.Tick += (s, e) => RefreshLog();
            _refreshTimer.Start();
        }

        private void InitializeComponent()
        {
            this.Text = "Log Viewer";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = true;

            // Main layout panel
            TableLayoutPanel mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };

            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

            this.Controls.Add(mainPanel);

            // Top panel for controls
            Panel topPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Log file selection
            Label logFileLabel = new Label
            {
                Text = "Log File:",
                AutoSize = true,
                Location = new Point(10, 12)
            };
            topPanel.Controls.Add(logFileLabel);

            _logFileComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 250,
                Location = new Point(80, 8)
            };
            _logFileComboBox.SelectedIndexChanged += (s, e) => LoadCurrentLog();
            topPanel.Controls.Add(_logFileComboBox);

            // Log level filter
            Label logLevelLabel = new Label
            {
                Text = "Log Level:",
                AutoSize = true,
                Location = new Point(350, 12)
            };
            topPanel.Controls.Add(logLevelLabel);

            _logLevelComboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
                Location = new Point(420, 8)
            };

            // Add log levels to the combo box
            foreach (LogLevel level in Enum.GetValues(typeof(LogLevel)))
            {
                _logLevelComboBox.Items.Add(level.ToString());
            }

            // Default to Info level
            _logLevelComboBox.SelectedIndex = (int)LogLevel.Info;
            _logLevelComboBox.SelectedIndexChanged += (s, e) => LoadCurrentLog();
            topPanel.Controls.Add(_logLevelComboBox);

            // Auto refresh checkbox
            _autoRefreshCheckBox = new CheckBox
            {
                Text = "Auto Refresh",
                Checked = true,
                Location = new Point(560, 10),
                AutoSize = true
            };
            _autoRefreshCheckBox.CheckedChanged += (s, e) => _refreshTimer.Enabled = _autoRefreshCheckBox.Checked;
            topPanel.Controls.Add(_autoRefreshCheckBox);

            // Refresh button
            _refreshButton = new Button
            {
                Text = "Refresh",
                Location = new Point(670, 8),
                Width = 80
            };
            _refreshButton.Click += (s, e) => LoadCurrentLog();
            topPanel.Controls.Add(_refreshButton);

            mainPanel.Controls.Add(topPanel, 0, 0);

            // Log text box
            _logTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Font = new Font("Consolas", 9F),
                BackColor = Color.White,
                WordWrap = false
            };

            mainPanel.Controls.Add(_logTextBox, 0, 1);

            // Bottom panel for buttons
            Panel bottomPanel = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Clear button
            _clearButton = new Button
            {
                Text = "Clear",
                Location = new Point(670, 8),
                Width = 80
            };
            _clearButton.Click += (s, e) => {
                if (MessageBox.Show("Clear the log viewer? This does not delete the log file.",
                                    "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _logTextBox.Clear();
                }
            };
            bottomPanel.Controls.Add(_clearButton);

            // Close button
            _closeButton = new Button
            {
                Text = "Close",
                Location = new Point(770, 8),
                Width = 80,
                DialogResult = DialogResult.Cancel
            };
            _closeButton.Click += (s, e) => this.Close();
            bottomPanel.Controls.Add(_closeButton);

            mainPanel.Controls.Add(bottomPanel, 0, 2);

            // Set as cancel button
            this.CancelButton = _closeButton;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_refreshTimer != null)
                {
                    _refreshTimer.Stop();
                    _refreshTimer.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}