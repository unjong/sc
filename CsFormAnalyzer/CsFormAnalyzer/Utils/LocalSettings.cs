using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsFormAnalyzer.Utils
{
	public class LocalSettings
	{
		private KeyValueConfigurationCollection settings;
		private Configuration config;

		/// <summary>
		/// None = 0,
		/// PerUserRoaming = 10,
		/// PerUserRoamingAndLocal = 20,
		/// </summary>
		public LocalSettings(string filePath, int level = 0)
		{
			var map = new ExeConfigurationFileMap();
			map.ExeConfigFilename = filePath;

			if (string.IsNullOrEmpty(filePath))
				config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			else
				config = ConfigurationManager.OpenMappedExeConfiguration(map, (ConfigurationUserLevel)level);

			settings = config.AppSettings.Settings;
		}

		public void Set(string key, string value)
		{
			if (ContainsKey(key))
				settings[key].Value = value;

			else
				settings.Add(key, value);

			config.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
		}

		public string Get(string key, string defaultReturn = null)
		{
			if (ContainsKey(key))
				return settings[key].Value;
			else
				return defaultReturn;
		}

        public void Remove(string key)
        {
            if (ContainsKey(key))
            {
                settings.Remove(key);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }
        }

		private bool ContainsKey(string key)
		{
			return settings.AllKeys.Contains<string>(key);
		}

		public void Save()
		{
			config.Save(ConfigurationSaveMode.Full);
			ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
		}
    }
}
