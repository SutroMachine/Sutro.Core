using System.Collections.Generic;

namespace Sutro.Core.Settings.Material
{
    public static class MaterialProfileFactoryFFF
    {
        public static IEnumerable<MaterialProfileFFF> EnumerateDefaults()
        {
            yield return new MaterialProfileFFF().ConfigureCommon().ConfigurePLA();
            yield return new MaterialProfileFFF().ConfigureCommon().ConfigureABS();
            yield return new MaterialProfileFFF().ConfigureCommon().ConfigurePETG();
            yield return new MaterialProfileFFF().ConfigureCommon().ConfigureNylon();
            yield return new MaterialProfileFFF().ConfigureCommon().ConfigureTPU();
        }

        private static MaterialProfileFFF ConfigureABS(this MaterialProfileFFF material)
        {
            material.Material = "ABS";
            material.ExtruderTempC = 230;
            material.HeatedBedTempC = 90;
            return material;
        }

        private static MaterialProfileFFF ConfigureCommon(this MaterialProfileFFF material)
        {
            material.Supplier = "Generic";
            material.FilamentDiamMM = 1.75;
            return material;
        }

        private static MaterialProfileFFF ConfigureNylon(this MaterialProfileFFF material)
        {
            material.Material = "Nylon";
            material.ExtruderTempC = 250;
            material.HeatedBedTempC = 85;
            return material;
        }

        private static MaterialProfileFFF ConfigurePETG(this MaterialProfileFFF material)
        {
            material.Material = "PETG";
            material.ExtruderTempC = 235;
            material.HeatedBedTempC = 70;
            return material;
        }

        private static MaterialProfileFFF ConfigurePLA(this MaterialProfileFFF material)
        {
            material.Material = "PLA";
            material.ExtruderTempC = 210;
            material.HeatedBedTempC = 50;
            return material;
        }

        private static MaterialProfileFFF ConfigureTPU(this MaterialProfileFFF material)
        {
            material.Material = "TPU";
            material.ExtruderTempC = 220;
            material.HeatedBedTempC = 50;
            return material;
        }
    }
}