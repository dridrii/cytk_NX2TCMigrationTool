using System;
using System.Collections.Generic;
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

            // Validate against schema if available
            if (File.Exists(_schemaFilePath))
            {
                _settings.Schemas.Add(null, _schemaFilePath);
                _settings.Validate(null);
            }
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

        /// <summary>
        /// Gets a list of XML elements matching the given XPath
        /// </summary>
        /// <param name="xpath">XPath query to execute</param>
        /// <returns>A list of matching XML nodes</returns>
        public List<XmlNode> GetSettingElements(string xpath)
        {
            var result = new List<XmlNode>();
            var nodes = _settings.SelectNodes(xpath);

            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    result.Add(node);
                }
            }

            return result;
        }

        /// <summary>
        /// Adds a new element to the settings XML
        /// </summary>
        /// <param name="parentXPath">XPath to the parent element</param>
        /// <param name="elementName">Name of the new element</param>
        /// <param name="value">Value of the new element</param>
        /// <returns>True if the element was added, false otherwise</returns>
        public bool AddElement(string parentXPath, string elementName, string value)
        {
            var parentNode = _settings.SelectSingleNode(parentXPath);
            if (parentNode != null)
            {
                var newElement = _settings.CreateElement(elementName);
                newElement.InnerText = value;
                parentNode.AppendChild(newElement);
                _settings.Save(_settingsFilePath);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes an element from the settings XML
        /// </summary>
        /// <param name="xpath">XPath to the element to remove</param>
        /// <returns>True if the element was removed, false otherwise</returns>
        public bool RemoveElement(string xpath)
        {
            var node = _settings.SelectSingleNode(xpath);
            if (node != null && node.ParentNode != null)
            {
                node.ParentNode.RemoveChild(node);
                _settings.Save(_settingsFilePath);
                return true;
            }

            return false;
        }
    }
}