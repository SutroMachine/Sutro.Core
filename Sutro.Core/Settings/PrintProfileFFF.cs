using gs;
using Sutro.Core.FunctionalTest;
using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using Sutro.Core.Settings.Material;
using Sutro.Core.Settings.Part;
using System;

namespace Sutro.Core.Settings
{
    public class PrintProfileFFF : PrintProfileBase<
        MachineProfileFFF, MaterialProfileFFF, PartProfileFFF>
    {
        public static int SchemaVersion => 1;

        public PrintProfileFFF()
        {
            FillTypeFactory = new FillTypeFactory(this);
            Machine = new MachineProfileFFF();
            Material = new MaterialProfileFFF();
            Part = new PartProfileFFF();
        }

        public virtual IPrintProfile Clone()
        {
            return CloneAs<PrintProfileFFF>();
        }

        public FillTypeFactory FillTypeFactory { get; }
    }
}