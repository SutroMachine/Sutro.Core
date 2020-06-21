using CommandLine;
using System.Collections.Generic;

namespace gs
{
    public class CommandLineOptions
    {
        [Value(0, MetaName = "generator", Required = true, HelpText = "Generator to use.")]
        public string Generator { get; set; }

        [Value(1, MetaName = "gcode", Required = true, HelpText = "Path to output gcode file.")]
        public string GCodeFilePath { get; set; }

        [Value(2, MetaName = "mesh", Required = false, HelpText = "Path to input mesh file.")]
        public string MeshFilePath { get; set; }

        [Option('c', "center_xy", Required = false, Default = false, HelpText = "Center the part on the print bed in XY.")]
        public bool CenterXY { get; set; }

        [Option('z', "drop_z", Required = false, Default = false, HelpText = "Drop the part to the print bed in Z.")]
        public bool DropZ { get; set; }

        [Option('s', "settings_files", Required = false, HelpText = "Settings file(s).")]
        public IEnumerable<string> SettingsFiles { get; set; }

        [Option('o', "settings_override", Required = false, HelpText = "Override individual settings")]
        public IEnumerable<string> SettingsOverride { get; set; }

        [Option('v', "verbosity", Required = false, Default = 1, HelpText = "Detail of logging information to report (0 = none, 1 = errors, 2 = warnings, 3 = info)")]
        public int Verbosity { get; set; }
    }
}