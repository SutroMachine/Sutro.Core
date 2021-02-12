using Sutro.Core.Models;

namespace gs
{
    public static class PrintMeshOptionsFactory
    {
        public static PrintMeshOptions Default()
        {
            return new PrintMeshOptions()
            {
                IsSupport = false,
                IsCavity = false,
                IsCropRegion = false,
                IsOpen = false,
                OpenPathMode = OpenPathsModes.Default
            };
        }

        public static PrintMeshOptions Support()
        {
            return new PrintMeshOptions()
            {
                IsCavity = false,
                IsCropRegion = false,
                IsOpen = false,
                IsSupport = true,
                OpenPathMode = OpenPathsModes.Default
            };
        }

        public static PrintMeshOptions Cavity()
        {
            return new PrintMeshOptions()
            {
                IsSupport = false,
                IsCropRegion = false,
                IsOpen = false,
                IsCavity = true,
                OpenPathMode = OpenPathsModes.Default
            };
        }

        public static PrintMeshOptions CropRegion()
        {
            return new PrintMeshOptions()
            {
                IsSupport = false,
                IsCavity = false,
                IsOpen = false,
                IsCropRegion = true,
                OpenPathMode = OpenPathsModes.Default
            };
        }
    }
}