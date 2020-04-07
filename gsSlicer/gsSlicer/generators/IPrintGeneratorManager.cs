﻿using g3;
using System;
using System.Collections.Generic;
using System.IO;

namespace gs
{
    public interface IPrintGeneratorManager
    {
        Version PrintGeneratorAssemblyVersion { get; }
        string PrintGeneratorAssemblyName { get; }
        string PrintGeneratorName { get; }

        bool AcceptsParts { get; }
        ISettingsBuilder SettingsBuilder { get; }

        GCodeFile GCodeFromMesh(DMesh3 mesh, out IEnumerable<string> generationReport);

        void SaveGCodeToFile(TextWriter output, GCodeFile file);
    }
}