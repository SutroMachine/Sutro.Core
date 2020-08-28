using Sutro.Core.Settings;

namespace gs
{
    public static class PrintrbotAssembler
    {
        public static BaseDepositionAssembler Factory(
            GCodeBuilder builder, PrintProfileFFF settings)
        {
            var asm = new RepRapAssembler(builder, settings);
            asm.HeaderCustomizerF = (state, builder) => HeaderCustomF(settings, state, builder);
            return asm;
        }

        private static void HeaderCustomF(
            PrintProfileFFF settings, RepRapAssembler.HeaderState state, GCodeBuilder builder)
        {
            if (state == RepRapAssembler.HeaderState.BeforePrime)
            {
                if (settings.MachineProfile.HasAutoBedLeveling && settings.MachineProfile.EnableAutoBedLeveling)
                    builder.BeginGLine(29, "auto-level bed");
            }
        }
    }
}