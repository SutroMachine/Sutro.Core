using gs.FillTypes;
using Sutro.Core.Settings;

namespace gs
{
    public class FillTypeFactory
    {
        private PrintProfileFFF settings;

        public FillTypeFactory(PrintProfileFFF settings)
        {
            this.settings = settings;
        }

        public IFillType Bridge()
        {
            return new BridgeFillType(settings.Part.BridgeVolumeScale, settings.Part.CarefulExtrudeSpeed * settings.Part.BridgeExtrudeSpeedX);
        }

        public IFillType Default()
        {
            return new DefaultFillType();
        }

        public IFillType InnerPerimeter()
        {
            return new InnerPerimeterFillType(1, settings.Part.InnerPerimeterSpeedX);
        }

        public IFillType InteriorShell()
        {
            return new InteriorShellFillType();
        }

        public IFillType OpenShellCurve()
        {
            return new OpenShellCurveFillType();
        }

        public IFillType OuterPerimeter()
        {
            return new OuterPerimeterFillType(1, settings.Part.OuterPerimeterSpeedX);
        }

        public IFillType SkirtBrim(PrintProfileFFF settings)
        {
            return new SkirtBrimFillType();
        }

        public IFillType Solid()
        {
            return new SolidFillType(1, settings.Part.SolidFillSpeedX);
        }

        public IFillType Sparse()
        {
            return new SparseFillType();
        }

        public IFillType Support()
        {
            return new SupportFillType(settings.Part.SupportVolumeScale, settings.Part.OuterPerimeterSpeedX);
        }
    }
}