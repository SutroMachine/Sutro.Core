using g3;
using Newtonsoft.Json;
using Sutro.Core.Models.Profiles;
using System.Collections.Generic;

namespace Sutro.Core.Settings.Part
{
    public class PartProfileFFF : PartProfileBase
    {
        public override int SchemaVersion => PrintProfileFFF.SchemaVersion;

        public override IProfile Clone()
        {
            return CloneAs<PartProfileFFF>();
        }


        #region Basic

       public virtual int RoofLayers { get; set; } = 2;
        public virtual int FloorLayers { get; set; } = 2;

        #endregion Basic

        #region Retraction

        public bool EnableRetraction { get; set; } = true;
        public bool UseFirmwareRetraction { get; set; } = false;
        public double RetractDistanceMM { get; set; } = 1.3;

        // Don't retract if we are travelling less than this distance
        public double MinRetractTravelLength { get; set; } = 2.5;

        #endregion Retraction

        #region Seam Location

        // Overrides ShellRandomizeStart if set
        public bool ZipperAlignedToPoint { get; set; } = false;

        public double ZipperLocationX { get; set; } = 0.0;
        public double ZipperLocationY { get; set; } = 0.0;

        // Not compatible with ZipperAlignedToPoint
        public bool ShellRandomizeStart { get; set; } = false;

        #endregion Seam Location

        #region Speeds

        // All units are mm/min = (mm/s * 60)

        public double RetractSpeed { get; set; } = 25 * 60;   // 1500
        public double ZTravelSpeed { get; set; } = 23 * 60;   // 1380
        public double RapidTravelSpeed { get; set; } = 150 * 60;  // 9000
        public double CarefulExtrudeSpeed { get; set; } = 30 * 60;    // 1800
        public double RapidExtrudeSpeed { get; set; } = 90 * 60;      // 5400
        public double MinExtrudeSpeed { get; set; } = 20 * 60;        // 600

        public double OuterPerimeterSpeedX { get; set; } = 0.5;
        public double InnerPerimeterSpeedX { get; set; } = 1;
        public double SolidFillSpeedX { get; set; } = 1;

        #endregion Speeds

        #region Misc

        // Default fan speed, fraction of max speed (generally unknown)
        public double FanSpeedX { get; set; } = 1.0;

        #endregion Misc

        #region Travel Lift

        public bool TravelLiftEnabled { get; set; } = true;
        public double TravelLiftHeight { get; set; } = 0.2;
        public double TravelLiftDistanceThreshold { get; set; } = 5d;

        #endregion Travel Lift

        #region Shells

        public int Shells { get; set; } = 2;

        // How many shells to add around interior solid regions (eg roof/floor)
        public int InteriorSolidRegionShells { get; set; } = 0;

        // Do outer shell last (better quality but worse precision)
        public bool OuterShellLast { get; set; } = false;

        #endregion Shells

        #region Solid Fill

        // Multipler on Machine.NozzleDiamMM, defines spacing between adjacent
        public double ShellsFillNozzleDiamStepX { get; set; } = 1.0;

        // Multipler on Machine.NozzleDiamMM, defines spacing between adjacent nested shells/perimeters. If < 1, they overlap.
        public double SolidFillNozzleDiamStepX { get; set; } = 1.0;

        // This is a multiplier on Machine.NozzleDiamMM, defines how far we overlap solid fill onto border shells (if 0, no overlap) solid fill parallel lines. If < 1, they overlap.
        public double SolidFillBorderOverlapX { get; set; } = 0.25f;

        #endregion Solid Fill

        #region Sparse Fill

        // Multiplier on FillPathSpacingMM
        public double SparseLinearInfillStepX { get; set; } = 5.0;

        // This is a multiplier on Machine.NozzleDiamMM, defines how far we
        // overlap solid fill onto border shells (if 0, no overlap)
        public double SparseFillBorderOverlapX { get; set; } = 0.25f;

        public List<double> InfillAngles { get; set; } = new List<double> { -45, 45 };

        #endregion Sparse Fill

        #region Start Layer

        // Number of layers to treat as start layers with special handling
        public int StartLayers { get; set; } = 0;

        // Height of start layers. If 0, same as regular layers.
        public double StartLayerHeightMM { get; set; } = 0;

        #endregion Start Layer

        #region Skirt

        public int SkirtCount { get; set; } = 0;
        public int SkirtLayers { get; set; } = 0;
        public double SkirtGap { get; set; } = 0;
        public double SkirtSpacingStepX { get; set; } = 1.0;

        #endregion Skirt

        #region Support

        // Should we auto-generate support
        public bool GenerateSupport { get; set; } = true;

        // Standard "support angle"
        public double SupportOverhangAngleDeg { get; set; } = 35;

        // Usage depends on support technique?
        public double SupportSpacingStepX { get; set; } = 5.0;

        // Multiplier on extrusion volume
        public double SupportVolumeScale { get; set; } = 1.0;

        // Should we print a shell around support areas
        public bool EnableSupportShell { get; set; } = true;

        // 2D inset/outset added to support regions; multiplier on Machine.NozzleDiamMM.
        public double SupportAreaOffsetX { get; set; } = -0.5;

        // How much space to leave between model and support
        public double SupportSolidSpace { get; set; } = 0.35f;

        // Support regions within this distance will be merged via topological dilation; multiplier on NozzleDiamMM.
        public double SupportRegionJoinTolX { get; set; } = 2.0;

        // Should we use support release optimization
        public bool EnableSupportReleaseOpt { get; set; } = true;

        // How much space do we leave
        public double SupportReleaseGap { get; set; } = 0.2f;

        // Minimal size of support polygons
        public double SupportMinDimension { get; set; } = 1.5;

        // Turn on/off detection of support 'tip' regions, ie tiny islands.
        public bool SupportMinZTips { get; set; } = true;

        // Width of per-layer support "points" (keep larger than SupportMinDimension!)
        public double SupportPointDiam { get; set; } = 2.5f;

        // Number of vertices for support-point polygons (circles)
        public int SupportPointSides { get; set; } = 4;

        #endregion Support

        #region Bridging

        public bool EnableBridging { get; set; } = true;
        public double MaxBridgeWidthMM { get; set; } = 10.0;

        // Multiplier on FillPathSpacingMM
        public double BridgeFillNozzleDiamStepX { get; set; } = 0.85;

        // Multiplier on extrusion volume
        public double BridgeVolumeScale { get; set; } = 1.0;

        // Multiplier on CarefulExtrudeSpeed
        public double BridgeExtrudeSpeedX { get; set; } = 0.5;

        #endregion Bridging

        #region Toolpath Filtering

        // Minimum layer time in seconds
        public double MinLayerTime { get; set; } = 5.0;

        // If true, try to remove portions of toolpaths that will self-overlap
        public bool ClipSelfOverlaps { get; set; } = false;

        // What counts as 'self-overlap'; this is a multiplier on NozzleDiamMM
        public double SelfOverlapToleranceX { get; set; } = 0.75;

        public double MinInfillLengthMM { get; set; } = 2.0;

        #endregion Toolpath Filtering

        #region Debug/Utility

        [JsonConverter(typeof(Interval1iConverter))]
        // Only compute slices in this range
        public Interval1i LayerRangeFilter { get; set; } = new Interval1i(0, 999999999);

        // Run a mesh auto-repair after it's been loaded
        public bool RepairMesh { get; set; } = true;

        public bool GCodeAppendBeadDimensions { get; set; } = true;

        #endregion Debug/Utility
    }
}