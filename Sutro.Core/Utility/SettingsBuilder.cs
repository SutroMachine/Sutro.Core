using Newtonsoft.Json;
using Sutro.Core.Logging;
using Sutro.Core.Persistence;
using System.IO;

namespace Sutro.Core.Utility
{
    public class SettingsBuilder<TSettings> : ISettingsBuilder<TSettings> where TSettings : new()
    {
        protected readonly ILogger logger;

        protected readonly JsonSerializerSettings jsonSerializerSettings = GetSerializerSettings();

        protected static JsonSerializerSettings GetSerializerSettings()
        {
            var contractResolver = new IgnoreablePropertiesContractResolver();
            contractResolver.Ignore(typeof(string), new string[] { "$schema" });

            return new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                ContractResolver = contractResolver,
            };
        }

        public TSettings Settings { get; }

        protected static JsonSerializerSettings CreateDefaultSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Error,
            };
        }

        public SettingsBuilder(TSettings settings, ILogger logger, JsonSerializerSettings jsonSerializerSettings = null)
        {
            Settings = settings;
            this.logger = logger;
            this.jsonSerializerSettings = jsonSerializerSettings ?? CreateDefaultSerializerSettings();
        }

        public void ApplyJSONFile(string settingFile)
        {
            if (!File.Exists(settingFile))
            {
                throw new FileLoadException("Must provide valid settings file path.");
            }
            else
            {
                logger.LogMessage($"Loading file {Path.GetFullPath(settingFile)}");
                string json = File.ReadAllText(settingFile);
                JsonConvert.PopulateObject(json, Settings, jsonSerializerSettings);
            }
        }

        public void ApplyJSONSnippet(string snippet)
        {
            var json = StringUtil.FormatSettingOverride(snippet);
            JsonConvert.PopulateObject(json, Settings, jsonSerializerSettings);
        }
    }
}