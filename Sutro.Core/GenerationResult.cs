﻿using gs;
using Sutro.Core.Logging;
using Sutro.Core.Models.GCode;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sutro.Core
{
    public class GenerationResult
    {
        public GCodeFile GCode { get; set; }

        private List<LogEntry> logEntries = new List<LogEntry>();
        public ReadOnlyCollection<LogEntry> LogEntries => logEntries.AsReadOnly();

        public GCodeGenerationDetails Details { get; set; }

        public GenerationResultStatus Status { get; set; } = GenerationResultStatus.Failure;

        public void AddLog(LoggingLevel warning, string message)
        {
            logEntries.Add(new LogEntry(warning, message));
        }
    }
}