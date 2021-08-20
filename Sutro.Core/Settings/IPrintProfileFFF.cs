using g3;
using gs;
using Sutro.Core.FillTypes;
using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using Sutro.Core.Settings.Material;
using Sutro.Core.Settings.Part;
using System;

namespace Sutro.Core.Settings
{
    public interface IPrintProfileFFF : IPrintProfile, IPlanarAdditiveProfile
    {
        FillTypeFactory FillTypeFactory { get; }

        MachineProfileFFF Machine { get; set; }
        MaterialProfileFFF Material { get; set; }
        PartProfileFFF Part { get; set; }

        double SupportOverhangDistance()
        {
            return Part.LayerHeightMM / Math.Tan(Part.SupportOverhangAngleDeg * MathUtil.Deg2Rad);
        }
    }
}