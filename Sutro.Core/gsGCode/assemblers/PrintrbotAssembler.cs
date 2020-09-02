using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings;

namespace gs
{
    public static class PrintrbotAssembler
    {
        public static BaseDepositionAssembler Factory(
            GCodeBuilder builder, IPrintProfileFFF settings)
        {
            var asm = new RepRapAssembler(builder, settings);
            asm.HeaderCustomizerF = (state, builder) => HeaderCustomF(settings as IPrintProfileFFF, state, builder);
            return asm;
        }

        private static void HeaderCustomF(
            IPrintProfileFFF settings, RepRapAssembler.HeaderState state, GCodeBuilder builder)
        {
            if (state == RepRapAssembler.HeaderState.BeforePrime)
            {
                if (settings.Machine.HasAutoBedLeveling && settings.Machine.EnableAutoBedLeveling)
                    builder.BeginGLine(29, "auto-level bed");
            }
        }
    }
}