using Newtonsoft.Json;
using Sutro.Core.Logging;
using Sutro.Core.Persistence;
using Sutro.Core.Settings;
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

    public class SettingsBuilder<TSettings> : ISettingsBuilder<TSettings> where TSettings : new()
    {
        private readonly ILogger logger;

        private readonly static JsonSerializerSettings jsonSerializerSettings = GetSerializerSettings();

        private static JsonSerializerSettings GetSerializerSettings()
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

        public SettingsBuilder(TSettings settings, ILogger logger)
        {
            Settings = settings;
            this.logger = logger;
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