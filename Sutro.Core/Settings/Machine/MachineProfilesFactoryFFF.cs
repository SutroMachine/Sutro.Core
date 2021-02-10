using System.Collections.Generic;

namespace Sutro.Core.Settings.Machine
{
    public static partial class MachineProfilesFactoryFFF
    {
        public static IEnumerable<MachineProfileFFF> EnumerateDefaults()
        {
            foreach (var p in Flashforge.EnumerateDefaults())
                yield return p;

            foreach (var p in Makerbot.EnumerateDefaults())
                yield return p;

            foreach (var p in Monoprice.EnumerateDefaults())
                yield return p;

            foreach (var p in Printrbot.EnumerateDefaults())
                yield return p;

            foreach (var p in Prusa.EnumerateDefaults())
                yield return p;

            foreach (var p in RepRap.EnumerateDefaults())
                yield return p;
        }
    }
}