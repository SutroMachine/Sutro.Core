using Sutro.Core.Settings;

namespace gs
{
    public static class FlashforgeAssembler
    {
        public static BaseDepositionAssembler Factory(
            GCodeBuilder builder, PrintProfileFFF settings)
        {
            var asm = new RepRapAssembler(builder, settings);
            asm.HomeSequenceF = (builder) => HomeSequence(builder, settings);
            asm.HeaderCustomizerF = HeaderCustomF;
            asm.TravelGCode = 1;
            return asm;
        }

        private static void HomeSequence(GCodeBuilder builder, PrintProfileFFF settings)
        {
            if (settings.Machine.HasAutoBedLeveling && settings.Machine.EnableAutoBedLeveling)
            {
                builder.BeginGLine(28).AppendL("W").AppendComment("home all without bed level");
                builder.BeginGLine(80, "auto-level bed");
            }
            else
            {
                // standard home sequenece
                builder.BeginGLine(28, "home x/y").AppendI("X", 0).AppendI("Y", 0);
                builder.BeginGLine(28, "home z").AppendI("Z", 0);
            }
        }

        private static void HeaderCustomF(RepRapAssembler.HeaderState state, GCodeBuilder Builder)
        {
            if (state == RepRapAssembler.HeaderState.AfterComments)
            {
                Builder.BeginMLine(201)
                    .AppendI("X", 1000).AppendI("Y", 1000).AppendI("Z", 200).AppendI("E", 5000)
                    .AppendComment("Set maximum accelleration in mm/sec^2");
                Builder.BeginMLine(203)
                    .AppendI("X", 200).AppendI("Y", 200).AppendI("Z", 12).AppendI("E", 120)
                    .AppendComment("Set maximum feedrates in mm/sec");
                Builder.BeginMLine(204)
                    .AppendI("S", 1250).AppendI("T", 1250)
                    .AppendComment("Set acceleration for moves (S) and retract (T)");
                Builder.BeginMLine(205)
                    .AppendF("X", 10).AppendF("Y", 10).AppendF("Z", 0.4).AppendF("E", 2.5)
                    .AppendComment("Set jerk limits in mm/sec");
                Builder.BeginMLine(205)
                    .AppendI("S", 0).AppendI("T", 0)
                    .AppendComment("Set minimum extrude and travel feed rate in mm/sec");
            }
        }
    }
}