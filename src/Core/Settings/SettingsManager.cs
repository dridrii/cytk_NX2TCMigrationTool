using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;

namespace cytk_NX2TCMigrationTool.src.Core.Settings
{
    public class SettingsManager
    {
        private readonly string _settingsFilePath;
        private readonly string _schemaFilePath;
        private XmlDocument _settings;

        public SettingsManager(string settingsFilePath, string schemaFilePath)
        {
            _settingsFilePath = settingsFilePath;
            _schemaFilePath = schemaFilePath;
            _settings = new XmlDocument();
        }

        public void Initialize()
        {
            if (!File.Exists(_settingsFilePath))
            {
                CreateDefaultSettings();
            }

            LoadSettings();
        }

        private void LoadSettings()
        {
            _settings.Load(_settingsFilePath);

            // Validate against schema
            _settings.Schemas.Add(null, _schemaFilePath);
            _settings.Validate(null);
        }

        private void CreateDefaultSettings()
        {
            File.Copy("DefaultSettings.xml", _settingsFilePath);
        }

        public string GetSetting(string xpath)
        {
            var node = _settings.SelectSingleNode(xpath);
            return node?.InnerText;
        }

        public void SaveSetting(string xpath, string value)
        {
            var node = _settings.SelectSingleNode(xpath);
            if (node != null)
            {
                node.InnerText = value;
                _settings.Save(_settingsFilePath);
            }
        }
    }
}