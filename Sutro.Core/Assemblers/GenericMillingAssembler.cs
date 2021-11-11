using g3;
using Sutro.Core.GCodeBuilders;
using Sutro.Core.Settings;
using System;

namespace Sutro.Core.Assemblers
{
    public class GenericMillingAssembler : BaseMillingAssembler
    {
        public static BaseMillingAssembler Factory(GCodeBuilder builder, IPrintProfileFFF settings)
        {
            return new GenericMillingAssembler(builder, settings);
        }

        public IPrintProfileFFF Settings { get; }

        public GenericMillingAssembler(GCodeBuilder useBuilder, IPrintProfileFFF settings) : base(useBuilder)
        {
            Settings = settings;

            OmitDuplicateZ = true;
            OmitDuplicateF = true;

            HomeSequenceF = StandardHomeSequence;
        }

        public override void UpdateProgress(int i)
        {
            // Do nothing by defualt
        }

        public override void ShowMessage(string s)
        {
            Builder.AddCommentLine(s);
        }

        /// <summary>
        /// Replace this to run your own home sequence
        /// </summary>
        public Action<GCodeBuilder> HomeSequenceF { get; set; }

        public enum HeaderState
        {
            AfterComments,
            BeforeHome
        };

        public Action<HeaderState, GCodeBuilder> HeaderCustomizerF { get; set; } = (state, builder) => { };

        public override void AppendHeader()
        {
            AppendHeader_StandardRepRap();
        }

        private void AppendHeader_StandardRepRap()
        {
            base.AddStandardHeader(Settings);

            HeaderCustomizerF(HeaderState.AfterComments, Builder);

            Builder.BeginGLine(21, "units=mm");
            Builder.BeginGLine(90, "absolute positions");

            HeaderCustomizerF(HeaderState.BeforeHome, Builder);

            HomeSequenceF(Builder);

            currentPos = Vector3d.Zero;

            ShowMessage("Cut Started");

            in_travel = false;

            UpdateProgress(0);
        }

        public override void AppendFooter()
        {
            AppendFooter_StandardRepRap();
        }

        private void AppendFooter_StandardRepRap()
        {
            UpdateProgress(100);

            Builder.AddCommentLine("End of print");
            ShowMessage("Done!");

            Builder.BeginMLine(30, "end program");

            Builder.EndLine();      // need to force this
        }

        public virtual void StandardHomeSequence(GCodeBuilder builder)
        {
            // Do nothing by default
        }
    }
}