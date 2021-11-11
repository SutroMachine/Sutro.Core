using g3;
using Sutro.Core.Settings;
using System.Collections.Generic;

namespace Sutro.Core.Assemblers
{
    public interface IDepositionAssembler : IGCodeAssembler
    {
        double ExtruderA { get; }
        bool InRetract { get; }
        bool InTravel { get; }
        Vector3d NozzlePosition { get; }

        void AppendComment(string comment);

        void AppendExtrudeTo(Vector3d position, double feedRate, double extrusion, string comment = null);

        void AppendFooter();

        void AppendHeader();

        void AppendMoveTo(Vector3d position, double feedRate, string comment);

        void BeginRetract(Vector3d position, double retractSpeed, double extrusion, string comment = null);

        void BeginTravel();

        void DisableFan();

        void EnableFan();

        void EndRetract(Vector3d position, double retractSpeed, double extrusion, string comment = null);

        void EndTravel();

        void FlushQueues();

        IEnumerable<string> GenerateTotalExtrusionReport(IPrintProfileFFF settings);

        void UpdateProgress(int v);
    }
}