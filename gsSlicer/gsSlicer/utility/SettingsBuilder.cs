﻿using Newtonsoft.Json;
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

        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            MissingMemberHandling = MissingMemberHandling.Error,
        };

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
                try
                {
                    logger.WriteLine($"Loading file {Path.GetFullPath(settingFile)}");
                    string json = File.ReadAllText(settingFile);
                    JsonConvert.PopulateObject(json, Settings, jsonSerializerSettings);
                }
                catch (Exception e)
                {
                    logger.WriteLine("Error processing settings file: ");
                    logger.WriteLine(Path.GetFullPath(settingFile));
                    logger.WriteLine(e.Message);
                }
            }
        }

        public void ApplyJSONSnippet(string snippet)
        {
            var json = StringUtil.FormatSettingOverride(snippet);
            JsonConvert.PopulateObject(json, Settings, jsonSerializerSettings);
        }
    }
}