using g3;
using Newtonsoft.Json;
using Sutro.Core.Models;
using Sutro.Core.Models.Profiles;
using System;
using System.Collections.Generic;

namespace gs
{
    public interface IPlanarAdditiveSettings : IProfile
    {
        double LayerHeightMM { get; }

        AssemblerFactoryF AssemblerType();

        MachineInfo BaseMachine { get; set; }
    }
    public abstract class PlanarAdditiveSettings : SettingsPrototype, IPlanarAdditiveSettings
    {
        /// <summary>
        /// This is the "name" of this settings (eg user identifier)
        /// </summary>
        public string Identifier = "Defaults";

        public double LayerHeightMM { get; set; } = 0.2;

        public abstract MachineInfo BaseMachine { get; set; }

        public string ManufacturerName { get => BaseMachine.ManufacturerName; set => BaseMachine.ManufacturerName = value; }
        public string ModelIdentifier { get => BaseMachine.ModelIdentifier; set => BaseMachine.ModelIdentifier = value; }
        public double MachineBedSizeXMM { get => BaseMachine.BedSizeXMM; set => BaseMachine.BedSizeXMM = value; }
        public double MachineBedSizeYMM { get => BaseMachine.BedSizeYMM; set => BaseMachine.BedSizeYMM = value; }
        public double MachineBedSizeZMM { get => BaseMachine.MaxHeightMM; set => BaseMachine.MaxHeightMM = value; }

        public MachineBedOriginLocationX OriginX 
        {
            get => MachineBedOriginLocationUtility.LocationXFromScalar(BaseMachine.BedOriginFactorX);
            set => BaseMachine.BedOriginFactorX = MachineBedOriginLocationUtility.LocationXFromEnum(value);
        }

        public MachineBedOriginLocationY OriginY
        {
            get => MachineBedOriginLocationUtility.LocationYFromScalar(BaseMachine.BedOriginFactorY);
            set => BaseMachine.BedOriginFactorY = MachineBedOriginLocationUtility.LocationYFromEnum(value);
        }

        public abstract string MaterialName { get; set; }
        public abstract string ProfileName { get; set; }

        public abstract AssemblerFactoryF AssemblerType();

        public abstract IProfile Clone();
    }

    public class SingleMaterialFFFSettings : PlanarAdditiveSettings
    {
        public SingleMaterialFFFSettings()
        {
            FillTypeFactory = new FillTypeFactory(this);
        }


        // This is a bit of an odd place for this, but settings are where we actually
        // know what assembler we should be using...
        public override AssemblerFactoryF AssemblerType()
        {
            throw new NotImplementedException($"{GetType()}.AssemblerType() not provided");
        }

        public override IProfile Clone()
        {
            return CloneAs<SingleMaterialFFFSettings>();
        }

        protected FFFMachineInfo machineInfo;

        public FFFMachineInfo Machine
        {
            get { if (machineInfo == null) machineInfo = new FFFMachineInfo(); return machineInfo; }
            set { machineInfo = value; }
        }

        public override MachineInfo BaseMachine
        {
            get { return Machine; }
            set
            {
                if (value is FFFMachineInfo)
                    machineInfo = value as FFFMachineInfo;
                else
                    throw new Exception("SingleMaterialFFFSettings.Machine.set: type is not FFFMachineInfo!");
            }
        }

        public FillTypeFactory FillTypeFactory { get; }

        #region Material

        public string MaterialSource { get; set; } = "Generic";
        public string MaterialType { get; set; } = "PLA";
        public string MaterialColor { get; set; } = "Blue";

        public override string MaterialName
        {
            get => $"{MaterialSource} {MaterialType} - {MaterialColor}";
            set { }
        }

        #endregion Material

        /*
         * Temperatures
         */

        public int ExtruderTempC = 210;
        public int HeatedBedTempC = 0;

        // Material Info
        public double FilamentCostPerKG { get; set; } = 19.19;

        public double FilamentGramsPerCubicMM { get; set; } = 0.00125;

        /*
		 * Distances.
		 * All units are mm
		 */

        public bool EnableRetraction = true;
        public bool UseFirmwareRetraction = false;
        public double RetractDistanceMM = 1.3;
        public double MinRetractTravelLength = 2.5;     // don't retract if we are travelling less than this distance

        public bool ZipperAlignedToPoint = false; // overrides ShellRandomizeStart if set
        public double ZipperLocationX = 0.0;
        public double ZipperLocationY = 0.0;
        public bool ShellRandomizeStart = false;  // not compatible with ZipperAlignedToPoint

        /*
		 * Speeds.
		 * All units are mm/min = (mm/s * 60)
		 */

        // these are all in units of millimeters/minute
        public double RetractSpeed = 25 * 60;   // 1500

        public double ZTravelSpeed = 23 * 60;   // 1380

        public double RapidTravelSpeed = 150 * 60;  // 9000

        public double CarefulExtrudeSpeed = 30 * 60;    // 1800
        public double RapidExtrudeSpeed = 90 * 60;      // 5400
        public double MinExtrudeSpeed = 20 * 60;        // 600

        public double OuterPerimeterSpeedX { get; set; } = 0.5;
        public double InnerPerimeterSpeedX { get; set; } = 1;
        public double SolidFillSpeedX { get; set; } = 1;

        public double FanSpeedX = 1.0;                  // default fan speed, fraction of max speed (generally unknown)

        // Settings for z-lift on rapid travel moves
        public bool TravelLiftEnabled { get; set; } = true;

        public double TravelLiftHeight { get; set; } = 0.2;
        public double TravelLiftDistanceThreshold { get; set; } = 5d;

        /*
         * Shells
         */
        public int Shells { get; set; } = 2;
        public int InteriorSolidRegionShells = 0;       // how many shells to add around interior solid regions (eg roof/floor)
        public bool OuterShellLast = false;             // do outer shell last (better quality but worse precision)

        /*
		 * Roof/Floors
		 */
        public virtual int RoofLayers { get; set; } = 2;
        public virtual int FloorLayers { get; set; } = 2;

        /*
         *  Solid fill settings
         */
        public double ShellsFillNozzleDiamStepX = 1.0;      // multipler on Machine.NozzleDiamMM, defines spacing between adjacent

        // nested shells/perimeters. If < 1, they overlap.
        public double SolidFillNozzleDiamStepX = 1.0;       // multipler on Machine.NozzleDiamMM, defines spacing between adjacent

        // solid fill parallel lines. If < 1, they overlap.
        public double SolidFillBorderOverlapX = 0.25f;      // this is a multiplier on Machine.NozzleDiamMM, defines how far we

        // overlap solid fill onto border shells (if 0, no overlap)

        /*
		 * Sparse infill settings
		 */
        public double SparseLinearInfillStepX = 5.0;      // this is a multiplier on FillPathSpacingMM

        public double SparseFillBorderOverlapX = 0.25f;     // this is a multiplier on Machine.NozzleDiamMM, defines how far we
                                                            // overlap solid fill onto border shells (if 0, no overlap)

        public List<double> InfillAngles = new List<double> { -45, 45 };

        /*
         * Start layer controls
         */
        public int StartLayers = 0;                      // number of start layers, special handling
        public double StartLayerHeightMM = 0;            // height of start layers. If 0, same as regular layers

        /*
         * Skirt controls
         */
        public int SkirtCount = 0;
        public int SkirtLayers = 0;
        public double SkirtGap = 0;
        public double SkirtSpacingStepX = 1.0;

        /*
         * Support settings
         */
        public bool GenerateSupport = true;              // should we auto-generate support
        public double SupportOverhangAngleDeg = 35;      // standard "support angle"
        public double SupportSpacingStepX = 5.0;         // usage depends on support technique?
        public double SupportVolumeScale = 1.0;          // multiplier on extrusion volume
        public bool EnableSupportShell = true;           // should we print a shell around support areas
        public double SupportAreaOffsetX = -0.5;         // 2D inset/outset added to support regions. Multiplier on Machine.NozzleDiamMM.
        public double SupportSolidSpace = 0.35f;         // how much space to leave between model and support
        public double SupportRegionJoinTolX = 2.0;		 // support regions within this distance will be merged via topological dilation. Multiplier on NozzleDiamMM.
        public bool EnableSupportReleaseOpt = true;      // should we use support release optimization
        public double SupportReleaseGap = 0.2f;          // how much space do we leave
        public double SupportMinDimension = 1.5;         // minimal size of support polygons
        public bool SupportMinZTips = true;              // turn on/off detection of support 'tip' regions, ie tiny islands.
        public double SupportPointDiam = 2.5f;           // width of per-layer support "points" (keep larger than SupportMinDimension!)
        public int SupportPointSides = 4;                // number of vertices for support-point polygons (circles)

        /*
		 * Bridging settings
		 */
        public bool EnableBridging = true;
        public double MaxBridgeWidthMM = 10.0;
        public double BridgeFillNozzleDiamStepX = 0.85;  // multiplier on FillPathSpacingMM
        public double BridgeVolumeScale = 1.0;           // multiplier on extrusion volume
        public double BridgeExtrudeSpeedX = 0.5;		 // multiplier on CarefulExtrudeSpeed

        /*
         * Toolpath filtering options
         */
        public double MinLayerTime = 5.0;                // minimum layer time in seconds
        public bool ClipSelfOverlaps = false;            // if true, try to remove portions of toolpaths that will self-overlap
        public double SelfOverlapToleranceX = 0.75;      // what counts as 'self-overlap'. this is a multiplier on NozzleDiamMM
        public double MinInfillLengthMM = 2.0;

        /*
         * Debug/Utility options
         */
        [JsonConverter(typeof(Interval1iConverter))]
        public Interval1i LayerRangeFilter = new Interval1i(0, 999999999);   // only compute slices in this range

        public bool RepairMesh = true;                  // run a mesh auto-repair after it's been loaded

        public bool GCodeAppendBeadDimensions { get; set; } = true;

        public override string ProfileName { get; set; } = "Default";

        /*
         * functions that calculate derived values
         * NOTE: these cannot be properties because then they will be json-serialized!
         */

        public virtual double ShellsFillPathSpacingMM()
        {
            return Machine.NozzleDiamMM * ShellsFillNozzleDiamStepX;
        }

        public virtual double SolidFillPathSpacingMM()
        {
            return Machine.NozzleDiamMM * SolidFillNozzleDiamStepX;
        }

        public virtual double BridgeFillPathSpacingMM()
        {
            return Machine.NozzleDiamMM * BridgeFillNozzleDiamStepX;
        }
    }

    // just for naming...
    public class GenericRepRapSettings : SingleMaterialFFFSettings
    {
        public override AssemblerFactoryF AssemblerType()
        {
            return RepRapAssembler.Factory;
        }

        public override IProfile Clone()
        {
            return CloneAs<GenericRepRapSettings>();
        }
    }
}