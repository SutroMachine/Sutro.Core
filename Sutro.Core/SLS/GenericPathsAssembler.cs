﻿using Sutro.Core.Toolpaths;

namespace Sutro.Core.SLS
{
    public class GenericPathsAssembler : IPathsAssembler
    {
        public ToolpathSet AccumulatedPaths;

        public GenericPathsAssembler()
        {
            AccumulatedPaths = new ToolpathSet();
        }

        public void AppendPaths(IToolpathSet paths)
        {
            AccumulatedPaths.Append(paths);
        }

        public ToolpathSet TempGetAssembledPaths()
        {
            return AccumulatedPaths;
        }
    }
}