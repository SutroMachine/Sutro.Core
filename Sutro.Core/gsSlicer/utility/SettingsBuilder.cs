using Newtonsoft.Json;
using Sutro.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace gs
{
    public interface ISettingsBuilder
    {
        void ApplyJSONFile(string settingFile);

        void ApplyJSONSnippet(string json);
    }

    public interface ISettingsBuilder<TSettings> : ISettingsBuilder
    {
        TSettings Settings { get; }
    }

    public class SettingsBuilder<TSettings> : ISettingsBuilder<TSettings> where TSettings : SettingsPrototype, new()
    {
        private readonly ILogger logger;

        protected readonly JsonSerializerSettings jsonSerializerSettings;

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
                logger.WriteLine("Must provide valid settings file path.");
            }
            else
            {
                logger.WriteLine($"Loading file {Path.GetFullPath(settingFile)}");
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