using g3;
using gs;
using Sutro.Core;
using Sutro.Core.Assemblers;
using Sutro.Core.Compilers;
using Sutro.Core.Fill;
using Sutro.Core.FillTypes;
using Sutro.Core.Models;
using Sutro.Core.Models.GCode;
using Sutro.Core.PartExteriors;
using Sutro.Core.Settings;
using Sutro.Core.Slicing;
using Sutro.Core.Toolpathing;
using Sutro.Core.Toolpaths;
using Sutro.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sutro.Core.Generators
{
    using ShellFillRegionDict = Dictionary<IShellsFillPolygon, FillRegions>;

    /// <summary>
    /// This is the top-level class that generates a GCodeFile for a stack of slices.
    /// Currently must subclass to provide resulting GCodeFile.
    /// </summary>
    public abstract class ThreeAxisPrintGenerator<T> : IPrintGenerator<T> where T : IPrintProfileFFF
    {
        // Data structures that must be provided by client
        public PrintMeshAssembly PrintMeshes { get; protected set; }

        public PlanarSliceStack Slices { get; protected set; }
        public IThreeAxisPrinterCompiler Compiler { get; protected set; }
        public IPrintProfileFFF Settings;      // public because you could modify
                                               // this during process, ie in BeginLayerF
                                               // to implement per-layer settings

        // available after calling Generate()
        public GCodeFile Result { get; protected set; }

        protected List<string> warnings = new List<string>();
        public IReadOnlyList<string> Warnings => warnings;

        // Generally we discard the paths at each layer as we generate them. If you
        // would like to analyze, set this to true, and then AccumulatedPaths will
        // be available after calling Generate(). The list AccumulatedPaths.Paths will
        // be a list with a separate PathSet for each layer, in bottom-up order.
        public bool AccumulatePathSet = false;

        public ToolpathSet AccumulatedPaths = null;

        public GenerationResult generationResult = null;

        public virtual IReadOnlyList<string> PrintTimeEstimate => TotalPrintTimeStatistics.ToStringList();

        public virtual IReadOnlyList<string> MaterialUsageEstimate => TotalExtrusionReport.ToList();

        public PrintTimeStatistics TotalPrintTimeStatistics { get; private set; } = new PrintTimeStatistics();

        /*
		 * Customizable functions you can use to configure/modify slicer behavior
		 */

        // replace this with your own error message handler
        public Action<string, string> ErrorF = (message, stack_trace) =>
        {
            Console.WriteLine("[EXCEPTION] ThreeAxisPrintGenerator: " + message + "\nSTACK TRACE: " + stack_trace);
        };

        // Replace this if you want to customize PrintLayerData type
        public Func<int, PlanarSlice, IPrintProfileFFF, PrintLayerData> PrintLayerDataFactoryF;

        // Replace this to use a different path builder
        public Func<PrintLayerData, ToolpathSetBuilder> PathBuilderFactoryF;

        // Replace this to use a different group scheduler
        public Func<PrintLayerData, IFillPathScheduler2d, Vector2d, GroupScheduler2d> GroupSchedulerFactoryF;

        // Replace this to use a different scheduler
        public Func<PrintLayerData, IFillPathScheduler2d> SchedulerFactoryF;

        // Replace this to use a different shell selector
        public Func<PrintLayerData, ILayerShellsSelector> ShellSelectorFactoryF;

        // This is called at the beginning of each layer, you can replace to
        // implement progress bar, etc
        public Action<PrintLayerData> BeginLayerF;

        // This is called before we process each shell. The Tag is transferred
        // from the associated region in the PlanarSlice, if it had one, otherwise it is int.MaxValue
        public Action<IFillPolygon, int> BeginShellF;

        // called at the end of each layer, before we compile the paths
        public ILayerPathsPostProcessor LayerPostProcessor;

        // this is called on polyline paths, return *true* to filter out a path. Useful for things like very short segments, etc
        // In default Initialize(), is set to a constant multiple of tool size
        public Func<FillCurve, bool> PathFilterF = null;

        // this is called on polyline paths, return *true* to filter out a path. Useful for things like very short segments, etc
        // In default Initialize(), is set to a constant multiple of tool size
        public Func<PlanarSliceStack, IPrintProfileFFF, IPartExterior> PartExteriorF = PrintGeneratorDefaults.PartExteriorFactory;

        // Called after we have finished print generation, use this to post-process the paths, etc.
        // By default appends a comment block with print time & material usage statistics
        public Action<IThreeAxisPrinterCompiler, ThreeAxisPrintGenerator<T>> PostProcessCompilerF
            = PrintGeneratorDefaults.AppendPrintStatistics;

        /// <summary>
        /// If this is set, we clip **generated** regions against it (ie generated support)
        /// Currently assuming that input PlanarSliceStack has already been clipped at slicer level!
        /// </summary>
        public List<GeneralPolygon2d> PathClipRegions = null;

        public IPartExterior PartExterior { get; protected set; }

        protected ThreeAxisPrintGenerator()
        {
        }

        public ThreeAxisPrintGenerator(PrintMeshAssembly meshes,
                                       PlanarSliceStack slices,
                                       T settings,
                                       IThreeAxisPrinterCompiler compiler)
        {
            Initialize(meshes, slices, settings, compiler);
        }

        public abstract void Initialize(PrintMeshAssembly meshes,
                                        PlanarSliceStack slices,
                                        T settings,
                                        AssemblerFactoryF overrideAssemblerF);

        public void Initialize(PrintMeshAssembly meshes,
                               PlanarSliceStack slices,
                               T settings,
                               IThreeAxisPrinterCompiler compiler)
        {
            PrintMeshes = meshes;
            Slices = slices;
            Settings = settings;
            Compiler = compiler;

            // set defaults for configurable functions

            PrintLayerDataFactoryF = (layer_i, slice, settingsArg) =>
            {
                return new PrintLayerData(slice.LayerIndex, slice, settingsArg);
            };

            PathBuilderFactoryF = (layer_data) =>
            {
                return new ToolpathSetBuilder();
            };

            SchedulerFactoryF = get_layer_scheduler;

            GroupSchedulerFactoryF = (layer_data, target, start) => new GroupScheduler2d(target, start);

            ShellSelectorFactoryF = (layer_data) =>
                new NextNearestLayerShellsSelector(layer_data.ShellFills);

            BeginLayerF = (layer_data) => { };

            BeginShellF = (shell_fill, tag) => { };

            LayerPostProcessor = null;

            if (PathFilterF == null)
                PathFilterF = (pline) => { return pline.TotalLength() < 3 * Settings.Machine.NozzleDiamMM; };

            PartExterior = PartExteriorF(Slices, Settings);
        }

        public virtual GenerationResult Generate(CancellationToken? cancellationToken)
        {
            var result = new GenerationResult();
            try
            {
                generate_result(cancellationToken);
                result.Status = GenerationResultStatus.Success;
                result.GCode = extract_result();
                result.Details = ExtractDetails();
            }
            catch (Exception e) when (!Config.Debug)
            {
                result.AddLog(Logging.LoggingLevel.Error, e.GetType().ToString() + ": " + e.Message);
                result.Status = GenerationResultStatus.Failure;
                if (Config.Debug)
                    throw;
            }
            return result;
        }

        protected virtual GCodeGenerationDetails ExtractDetails()
        {
            return new GCodeGenerationDetails(
                PrintTimeEstimate,
                TotalExtrusionReport,
                Warnings);
        }

        public virtual void GetProgress(out int curProgress, out int maxProgress)
        {
            curProgress = CurProgress;
            maxProgress = TotalProgress;
        }

        // subclasses must implement this to return GCodeFile result
        protected abstract GCodeFile extract_result();

        /*
         *  Internals
         */

        protected CancellationToken cancellationToken;

        // tags on slice polygons get transferred to shells
        protected IntTagSet<IFillPolygon> ShellTags = new IntTagSet<IFillPolygon>();

        // basic progress monitoring
        protected int TotalProgress = 1;

        protected int CurProgress = 0;

        protected virtual double LayerFillAngleF(int layer_i)
        {
            int count = Settings.Part.InfillAngles.Count;
            if (count > 0)
            {
                return Settings.Part.InfillAngles[layer_i % count];
            }
            else
            {
                return layer_i % 2 == 0 ? -45 : 45;
            }
        }

        // start and end layers we will solve for (intersection of layercount and LayerRangeFilter)
        protected int CurStartLayer;

        protected int CurEndLayer;

        /// <summary>
        /// This is the main driver of the slicing process
        /// </summary>
        protected virtual void generate_result(CancellationToken? cancellationToken)
        {
            this.cancellationToken = cancellationToken ?? CancellationToken.None;
            SetupGeneration();
            if (Cancelled()) return;

            PrecomputeGeneration();
            if (Cancelled()) return;

            PrintLayerData prevLayerData = null;

            CurStartLayer = Math.Max(0, Settings.Part.LayerRangeFilter.a);
            CurEndLayer = Math.Min(Slices.Count - 1, Settings.Part.LayerRangeFilter.b);

            // Now generate paths for each layer.
            // This could be parallelized to some extent, but we have to pass per-layer paths
            // to Scheduler in layer-order. Probably better to parallelize within-layer computes.
            for (int layer_i = CurStartLayer; layer_i <= CurEndLayer; ++layer_i)
            {
                if (Cancelled()) return;

                // allocate new layer data structure
                IPrintProfileFFF layerSettings = MakeLayerSettings(layer_i);
                PrintLayerData layerdata = PrintLayerDataFactoryF(layer_i, Slices[layer_i], layerSettings);
                layerdata.PreviousLayer = prevLayerData;

                if (LayerIsEmpty(layerdata))
                    continue;

                // create path accumulator
                ToolpathSetBuilder pathAccum = PathBuilderFactoryF(layerdata);
                layerdata.PathAccum = pathAccum;

                // rest of code does not directly access path builder, instead it
                // sends paths to scheduler.
                IFillPathScheduler2d layerScheduler = SchedulerFactoryF(layerdata);
                var groupScheduler = GroupSchedulerFactoryF(layerdata, layerScheduler, Compiler.NozzlePosition.xy);
                layerdata.Scheduler = groupScheduler;

                BeginLayerF(layerdata);
                CompileNewLayerHeader(layerdata);

                layerdata.ShellFills = get_layer_shells(layer_i);

                // make path-accumulator for this layer
                pathAccum.Initialize(Compiler.NozzlePosition);

                MoveToLayerPlane(pathAccum, layerdata);

                ScheduleSkirtBrim(layer_i, groupScheduler);
                if (Cancelled()) return;
                count_progress_step();

                ScheduleSupport(layer_i, layerdata, groupScheduler);
                if (Cancelled()) return;
                count_progress_step();

                ScheduleClosedPolygons(groupScheduler, layerdata, layer_i);
                if (Cancelled()) return;

                ScheduleOpenCurves(layerdata, groupScheduler);
                if (Cancelled()) return;

                // discard the group scheduler
                layerdata.Scheduler = groupScheduler.TargetScheduler;

                // last chance to post-process paths for this layer before they are baked in
                if (Cancelled()) return;
                if (LayerPostProcessor != null)
                    LayerPostProcessor.Process(layerdata, pathAccum.Paths);

                // change speeds if layer is going to finish too quickly
                EnforceMinimumLayerTime(layerSettings, pathAccum);
                if (Cancelled()) return;

                // compile this layer
                Compiler.AppendPaths(pathAccum.Paths, layerSettings);

                // add this layer to running pathset
                if (AccumulatedPaths != null)
                    AccumulatedPaths.Append(pathAccum.Paths);

                // we might want to consider this layer while we process next one
                prevLayerData = layerdata;

                if (Cancelled()) return;
                count_progress_step();
            }

            FinishGeneration();
        }

        private bool LayerIsEmpty(PrintLayerData layerdata)
        {
            int i = layerdata.layer_i;

            bool noShells =
                LayerShells == null ||
                LayerShells.Length <= i ||
                LayerShells[i] == null ||
                LayerShells[i].Count == 0;

            bool noSupport =
                LayerSupportAreas == null ||
                LayerSupportAreas.Length <= i ||
                LayerSupportAreas[i] == null ||
                LayerSupportAreas[i].Count == 0;

            return noShells && noSupport;
        }

        protected virtual void MoveToLayerPlane(ToolpathSetBuilder pathAccum, PrintLayerData layer)
        {
            pathAccum.AppendMoveToZ(layer.Slice.LayerZSpan.b, Settings.Part.ZTravelSpeed);
        }

        private void EnforceMinimumLayerTime(IPrintProfileFFF profile, ToolpathSetBuilder pathAccum)
        {
            if (profile.Part.MinLayerTime > 0)
            {
                CalculatePrintTime layer_time_calc = new CalculatePrintTime(pathAccum.Paths);

                bool layerModified = layer_time_calc.EnforceMinLayerTime(
                    profile.Part.MinLayerTime, profile.Part.MinExtrudeSpeed);

                if (layerModified)
                {
                    layer_time_calc.Calculate();
                }

                TotalPrintTimeStatistics.Add(layer_time_calc.TimeStatistics);
            }
        }

        private void ScheduleOpenCurves(PrintLayerData layerdata, GroupScheduler2d groupScheduler)
        {
            // append open paths
            groupScheduler.BeginGroup();
            add_open_paths(layerdata, groupScheduler);
            groupScheduler.EndGroup();
        }

        private void ScheduleSupport(int layer_i, PrintLayerData layerdata, GroupScheduler2d groupScheduler)
        {
            // do support
            // this could be done in parallel w/ roof/floor...
            var support_areas = get_layer_support_area(layer_i);
            if (support_areas != null)
            {
                groupScheduler.BeginGroup();
                fill_support_regions(support_areas, groupScheduler, layerdata);
                groupScheduler.EndGroup();
                layerdata.SupportAreas = support_areas;
            }
        }

        private void ScheduleSkirtBrim(int layer_i, GroupScheduler2d groupScheduler)
        {
            // do skirt first
            List<IShellsFillPolygon> skirt_layer = get_layer_skirts(layer_i);
            if (skirt_layer != null)
            {
                foreach (var skirt in skirt_layer)
                {
                    List<FillCurveSet2d> curves = skirt.GetFillCurves();
                    groupScheduler.BeginGroup();
                    groupScheduler.AppendCurveSets(curves);
                    groupScheduler.EndGroup();
                }
            }
        }

        protected virtual void CompileNewLayerHeader(PrintLayerData layerdata)
        {
            Compiler.AppendComment(" ");
            Compiler.AppendComment("========================");
            Compiler.AppendComment($"layer {layerdata.layer_i}: {layerdata.Slice.LayerZSpan.b:F3}mm");
        }

        /// <summary>
        /// Processing that happens before layer-by-layer generation. Includes
        /// parallel processing steps that can be done independently for multiple
        /// layers independently. Override to add additional precompute steps.
        /// </summary>
        protected virtual void PrecomputeGeneration()
        {
            PartExterior.Initialize(cancellationToken);

            // We need N above/below shell paths to do roof/floors, and *all* shells to do support.
            // Also we can compute shells in parallel. So we just precompute them all here.
            precompute_shells();
            if (Cancelled()) return;

            // compute solid/sparse in parallel based on shell interios, roofs & floors
            precompute_infill_regions();
            if (Cancelled()) return;

            // [TODO] use floor areas to determine support now?

            precompute_skirt();
            if (Cancelled()) return;

            precompute_support_areas();
            if (Cancelled()) return;
        }

        /// <summary>
        /// Final steps of print generation. Called after all layers are
        /// processed; override to add additional tear down.
        /// </summary>
        protected virtual void FinishGeneration()
        {
            Compiler.End();
            PostProcessCompilerF(Compiler, this);
        }

        /// <summary>
        /// Initial setup of print generation. Called before beginning the precompute steps;
        /// you can override it to add additional setup or processing.
        /// </summary>
        protected virtual void SetupGeneration()
        {
            int NProgressStepsPerLayer = 10;
            TotalProgress = NProgressStepsPerLayer * (Slices.Count - 1);
            CurProgress = 0;

            if (AccumulatePathSet == true)
                AccumulatedPaths = new ToolpathSet();

            // build spatial caches for slice polygons
            //bool need_slice_spatial = (Settings.GenerateSupport);
            bool need_slice_spatial = true;  // need this for bridges...
            if (need_slice_spatial)
            {
                Slices.BuildSliceSpatialCaches(true);
            }

            if (Cancelled()) return;

            // initialize compiler and get start nozzle position
            Compiler.Begin();
        }

        protected bool InputsAreInvalid()
        {
            if (Settings.Part.LayerHeightMM > Settings.Machine.NozzleDiamMM)
                warnings.Add("Layer height should not be greater than nozzle diameter.");

            return false;
        }

        /// <summary>
        /// assemble Settings for a given layer.
        /// </summary>
        protected virtual IPrintProfileFFF MakeLayerSettings(int layer_i)
        {
            var layerSettings = (IPrintProfileFFF)Settings.Clone();
            PlanarSlice slice = Slices[layer_i];
            // override standard layer height with slice ZSpan
            layerSettings.Part.LayerHeightMM = slice.LayerZSpan.Length;
            return layerSettings;
        }

        protected virtual void ScheduleClosedPolygons(GroupScheduler2d groupScheduler, PrintLayerData layerdata, int layer_i)
        {
            // selector determines what order we process shells in
            ILayerShellsSelector shellSelector = ShellSelectorFactoryF(layerdata);

            // a layer can contain multiple disjoint regions. Process each separately.
            IShellsFillPolygon shells_gen = shellSelector.Next(groupScheduler.CurrentPosition);
            while (shells_gen != null)
            {
                // schedule shell paths that we pre-computed
                List<FillCurveSet2d> shells_gen_paths = shells_gen.GetFillCurves();
                FillCurveSet2d outer_shell = shells_gen_paths.Count > 0 ? shells_gen_paths[shells_gen_paths.Count - 1] : null;
                bool do_outer_last = Settings.Part.OuterShellLast && shells_gen_paths.Count > 1;
                groupScheduler.BeginGroup();
                if (do_outer_last == false)
                {
                    groupScheduler.AppendCurveSets(shells_gen_paths);
                }
                else
                {
                    groupScheduler.AppendCurveSets(shells_gen_paths.GetRange(0, shells_gen_paths.Count - 1));
                }
                groupScheduler.EndGroup();
                if (Cancelled()) return;
                count_progress_step();

                // allow client to do configuration (eg change settings for example)
                BeginShellF(shells_gen, ShellTags.Get(shells_gen));

                // retrieve precomputed solid/sparse infill regions
                var fill_regions = LayerShellFillRegions[layer_i][shells_gen];

                if (fill_regions != null)
                {
                    // fill solid regions
                    groupScheduler.BeginGroup();
                    // [RMS] always call this for now because we may have bridge regions
                    // [TODO] we can precompute the bridge region calc we are doing here that is quite expensive...
                    fill_solid_regions(fill_regions.Solid, groupScheduler, layerdata, fill_regions.Sparse.Count > 0);
                    groupScheduler.EndGroup();

                    // fill infill regions
                    groupScheduler.BeginGroup();
                    fill_infill_regions(fill_regions.Sparse, groupScheduler, layerdata);
                    groupScheduler.EndGroup();
                    if (Cancelled()) return;
                    count_progress_step();
                }

                groupScheduler.BeginGroup();
                if (do_outer_last && outer_shell != null)
                {
                    groupScheduler.AppendCurveSets(new List<FillCurveSet2d>() { outer_shell });
                }
                groupScheduler.EndGroup();

                shells_gen = shellSelector.Next(groupScheduler.CurrentPosition);
            }
        }

        /// <summary>
        /// fill all infill regions
        /// </summary>
        protected virtual void fill_infill_regions(List<GeneralPolygon2d> infill_regions,
            IFillPathScheduler2d scheduler, PrintLayerData layer_data)
        {
            if (Settings.Part.SparseLinearInfillStepX < 0.1 || Settings.Part.SparseLinearInfillStepX > 100)
                return;
            double sparse_gap_width = Settings.Part.SparseLinearInfillStepX * Settings.Machine.NozzleDiamMM;

            foreach (GeneralPolygon2d infill_poly in infill_regions)
            {
                if (sparse_gap_width > infill_poly.Bounds.MaxDim * 2)
                    continue;

                List<GeneralPolygon2d> polys = new List<GeneralPolygon2d>() { infill_poly };

                if (Settings.Part.SparseFillBorderOverlapX > 0)
                {
                    double offset = Settings.Machine.NozzleDiamMM * Settings.Part.SparseFillBorderOverlapX;
                    polys = ClipperUtil.MiterOffset(polys, offset);
                }

                foreach (var poly in polys)
                {
                    fill_infill_region(poly, scheduler, layer_data);
                }
            }
        }

        /// <summary>
        /// fill polygon with sparse infill strategy
        /// </summary>
		protected virtual void fill_infill_region(GeneralPolygon2d infill_poly, IFillPathScheduler2d scheduler, PrintLayerData layer_data)
        {
            ICurvesFillPolygon infill_gen = new SparseLinesFillPolygon(infill_poly, new SparseFillType())
            {
                InsetFromInputPolygon = false,
                PathSpacing = Settings.Part.SparseLinearInfillStepX * Settings.SolidFillPathSpacingMM(),
                ToolWidth = Settings.Machine.NozzleDiamMM,
                AngleDeg = LayerFillAngleF(layer_data.layer_i),
                MinPathLengthMM = Settings.Part.MinInfillLengthMM
            };
            infill_gen.Compute();

            scheduler.AppendCurveSets(infill_gen.GetFillCurves());
        }

        protected virtual void fill_support_regions(List<GeneralPolygon2d> support_regions,
            IFillPathScheduler2d scheduler, PrintLayerData layer_data)
        {
            foreach (GeneralPolygon2d support_poly in support_regions)
                fill_support_region(support_poly, scheduler, layer_data);
        }

        /// <summary>
        /// fill polygon with support strategy
        ///     - single outer shell if Settings.PartProfile.EnableSupportShells = true
        ///     - then infill w/ spacing Settings.PartProfile.SupportSpacingStepX
        /// </summary>
		protected virtual void fill_support_region(GeneralPolygon2d support_poly, IFillPathScheduler2d scheduler, PrintLayerData layer_data)
        {
            AxisAlignedBox2d bounds = support_poly.Bounds;

            // settings may require a shell. However if support region
            // is very small, we will also use nested shells because infill
            // poly will likely be empty. In this case we nudge up the spacing
            // so that they are more loosely bonded
            // [TODO] we should only do this if we are directly below model. Otherwise this
            // branch is hit on any thin tube supports, that we could be printing empty
            int nShells = Settings.Part.EnableSupportShell ? 1 : 0;
            double support_spacing = Settings.Part.SupportSpacingStepX * Settings.Machine.NozzleDiamMM;
            double shell_spacing = Settings.Machine.NozzleDiamMM;
            if (bounds.MaxDim < support_spacing)
            {
                nShells = 3;
                shell_spacing = Settings.Machine.NozzleDiamMM * 1.25f;
            }

            List<GeneralPolygon2d> infill_polys = new List<GeneralPolygon2d>() { support_poly };

            if (nShells > 0)
            {
                ShellsFillPolygon shells_gen = new ShellsFillPolygon(support_poly, Settings.FillTypeFactory.Support());
                shells_gen.PathSpacing = shell_spacing;
                shells_gen.ToolWidth = Settings.Machine.NozzleDiamMM;
                shells_gen.Layers = nShells;
                shells_gen.FilterSelfOverlaps = false;
                shells_gen.PreserveInputInsetTopology = true;
                //shells_gen.FilterSelfOverlaps = true;
                shells_gen.PreserveOuterShells = false;
                //shells_gen.SelfOverlapTolerance = Settings.PartProfile.SelfOverlapToleranceX * Settings.PartProfile.Machine.NozzleDiamMM;
                shells_gen.DiscardTinyPolygonAreaMM2 = 0.1;
                shells_gen.DiscardTinyPerimeterLengthMM = 0.0;
                shells_gen.Compute();
                List<FillCurveSet2d> shell_fill_curves = shells_gen.GetFillCurves();

                // Note: this will wipe out fill types for inner and outer perimeters;
                // may need to create sub-fill types for support shells
                foreach (var fillpath in shell_fill_curves)
                {
                    fillpath.SetFillType(Settings.FillTypeFactory.Support());
                }
                scheduler.AppendCurveSets(shell_fill_curves);

                // expand inner polygon so that infill overlaps shell
                List<GeneralPolygon2d> inner_shells = shells_gen.GetInnerPolygons();
                if (Settings.Part.SparseFillBorderOverlapX > 0)
                {
                    double offset = Settings.Machine.NozzleDiamMM * Settings.Part.SparseFillBorderOverlapX;
                    infill_polys = ClipperUtil.MiterOffset(inner_shells, offset);
                }
            }

            foreach (var poly in infill_polys)
            {
                SupportLinesFillPolygon infill_gen = new SupportLinesFillPolygon(poly, Settings)
                {
                    InsetFromInputPolygon = Settings.Part.EnableSupportShell == false,
                    PathSpacing = support_spacing,
                    ToolWidth = Settings.Machine.NozzleDiamMM,
                    AngleDeg = 0,
                };
                infill_gen.Compute();
                scheduler.AppendCurveSets(infill_gen.GetFillCurves());
            }
        }

        /// <summary>
        /// fill set of solid regions
        /// </summary>
        protected virtual void fill_solid_regions(List<GeneralPolygon2d> solid_regions,
            IFillPathScheduler2d scheduler, PrintLayerData layer_data, bool bIsInfillAdjacent)
        {
            double filter_area = Settings.Machine.NozzleDiamMM * Settings.Machine.NozzleDiamMM;

            // if we have bridge regions on this layer, we subtract them from solid regions
            // and fill them using bridge strategy
            if (layer_data.layer_i > 0 && Settings.Part.EnableBridging)
            {
                // bridge regions for layer i were computed at layer i-1...
                List<GeneralPolygon2d> bridge_regions = get_layer_bridge_area(layer_data.layer_i - 1);

                if (bridge_regions.Count > 0)
                {
                    // bridge_regions are the physical bridge polygon.
                    // solid_regions are the regions we have not yet filled this layer.
                    // bridge works better if there is a 'landing pad' on either side, so we
                    // expand and then clip with the solid regions, to get actual bridge fill region.

                    double path_width = Settings.Machine.NozzleDiamMM;
                    double shells_width = Settings.Part.Shells * path_width;
                    //bridge_regions = ClipperUtil.MiterOffset(bridge_regions, shells_width, filter_area);
                    bridge_regions = ClipperUtil.SeparateMiterOffsets(bridge_regions, shells_width, filter_area);
                    bridge_regions = ClipperUtil.Intersection(bridge_regions, solid_regions, filter_area);
                    bridge_regions = CurveUtils2.FilterDegenerate(bridge_regions, filter_area);     // [RMS] do we need to do this?

                    if (bridge_regions.Count > 0)
                    {
                        // now have to subtract bridge region from solid region, in case there is leftover.
                        // We are not going to inset bridge region or solid fill,
                        // so we need to add *two* half-width tolerances
                        //var offset_regions = ClipperUtil.MiterOffset(bridge_regions, Settings.Machine.NozzleDiamMM, filter_area);
                        var offset_regions = ClipperUtil.SeparateMiterOffsets(bridge_regions, Settings.Machine.NozzleDiamMM, filter_area);
                        solid_regions = ClipperUtil.Difference(solid_regions, offset_regions, filter_area);
                        solid_regions = CurveUtils2.FilterDegenerate(solid_regions, filter_area);     // [RMS] do we need to do this?

                        foreach (var bridge_poly in bridge_regions)
                            fill_bridge_region(bridge_poly, scheduler, layer_data);
                    }
                }
            }

            foreach (GeneralPolygon2d solid_poly in solid_regions)
                fill_solid_region(layer_data, solid_poly, scheduler, bIsInfillAdjacent);
        }

        /// <summary>
        /// Fill polygon with solid fill strategy.
        /// If bIsInfillAdjacent, then we optionally add one or more shells around the solid
        /// fill, to give the solid fill something to stick to (imagine dense linear fill adjacent
        /// to sparse infill area - when the extruder zigs, most of the time there is nothing
        /// for the filament to attach to, so it pulls back. ugly!)
        /// </summary>
        protected virtual void fill_solid_region(PrintLayerData layer_data,
                                                 GeneralPolygon2d solid_poly,
                                                 IFillPathScheduler2d scheduler,
                                                 bool bIsInfillAdjacent = false)
        {
            if (Settings.SolidFillPathSpacingMM() == 0)
                return;

            List<GeneralPolygon2d> fillPolys = new List<GeneralPolygon2d>() { solid_poly };

            // if we are on an infill layer, and this shell has some infill region,
            // then we are going to draw contours around solid fill so it has
            // something to stick to
            // [TODO] should only be doing this if solid-fill is adjecent to infill region.
            //   But how to determine this? not easly because we don't know which polys
            //   came from where. Would need to do loop above per-polygon
            if (bIsInfillAdjacent && Settings.Part.InteriorSolidRegionShells > 0)
            {
                ShellsFillPolygon interior_shells = new ShellsFillPolygon(solid_poly, new InteriorShellFillType());
                interior_shells.PathSpacing = Settings.ShellsFillPathSpacingMM();
                interior_shells.ToolWidth = Settings.Machine.NozzleDiamMM;
                interior_shells.Layers = Settings.Part.InteriorSolidRegionShells;
                interior_shells.InsetFromInputPolygonX = 0;
                interior_shells.FilterSelfOverlaps = Settings.Part.ClipSelfOverlaps;
                interior_shells.PreserveOuterShells = false;
                interior_shells.SelfOverlapTolerance = Settings.Part.SelfOverlapToleranceX * Settings.Machine.NozzleDiamMM;
                interior_shells.Compute();
                scheduler.AppendCurveSets(interior_shells.GetFillCurves());
                fillPolys = interior_shells.InnerPolygons;
            }

            if (Settings.Part.SolidFillBorderOverlapX > 0)
            {
                double offset = Settings.Machine.NozzleDiamMM * Settings.Part.SolidFillBorderOverlapX;
                fillPolys = ClipperUtil.MiterOffset(fillPolys, offset);
            }

            // now actually fill solid regions
            foreach (GeneralPolygon2d fillPoly in fillPolys)
            {
                fill_solid_region(layer_data, fillPoly, scheduler);
            }
        }

        protected virtual void fill_solid_region(PrintLayerData layer_data, GeneralPolygon2d fillPoly, IFillPathScheduler2d scheduler)
        {
            ICurvesFillPolygon solid_gen = new ParallelLinesFillPolygon(fillPoly, Settings.FillTypeFactory.Solid())
            {
                InsetFromInputPolygon = false,
                PathSpacing = Settings.SolidFillPathSpacingMM(),
                ToolWidth = Settings.Machine.NozzleDiamMM,
                AngleDeg = LayerFillAngleF(layer_data.layer_i),
                FilterSelfOverlaps = Settings.Part.ClipSelfOverlaps,
                MinPathLengthMM = Settings.Part.MinInfillLengthMM
            };

            solid_gen.Compute();

            scheduler.AppendCurveSets(solid_gen.GetFillCurves());
        }

        /// <summary>
        /// Fill a bridge region. Goal is to use shortest paths possible.
        /// So, instead of just using fixed angle, we fit bounding box and
        /// use the shorter axis.
        /// </summary>
        protected virtual void fill_bridge_region(GeneralPolygon2d poly, IFillPathScheduler2d scheduler, PrintLayerData layer_data)
        {
            double spacing = Settings.BridgeFillPathSpacingMM();

            // fit bbox to try to find fill angle that has shortest spans
            Box2d box = poly.Outer.MinimalBoundingBox(0.00001);
            Vector2d axis = box.Extent.x > box.Extent.y ? box.AxisY : box.AxisX;
            double angle = Math.Atan2(axis.y, axis.x) * MathUtil.Rad2Deg;

            // [RMS] should we do something like this?
            //if (Settings.PartProfile.SolidFillBorderOverlapX > 0) {
            //	double offset = Settings.Machine.NozzleDiamMM * Settings.PartProfile.SolidFillBorderOverlapX;
            //	fillPolys = ClipperUtil.MiterOffset(fillPolys, offset);
            //}

            BridgeLinesFillPolygon fill_gen = new BridgeLinesFillPolygon(poly, Settings)
            {
                InsetFromInputPolygon = false,
                PathSpacing = spacing,
                ToolWidth = Settings.Machine.NozzleDiamMM,
                AngleDeg = angle,
            };
            fill_gen.Compute();
            scheduler.AppendCurveSets(fill_gen.GetFillCurves());
        }

        protected virtual void fill_bridge_region_decompose(GeneralPolygon2d poly, IFillPathScheduler2d scheduler, PrintLayerData layer_data)
        {
            poly.Simplify(0.1, 0.01, true);

            double minLength = Settings.Part.MaxBridgeWidthMM * 0.75;
            double minArea = minLength * minLength;

            var polys = PolygonDecomposer.Compute(poly, minArea);

            double spacing = Settings.BridgeFillPathSpacingMM();

            foreach (Polygon2d polypart in polys)
            {
                Box2d box = polypart.MinimalBoundingBox(0.00001);
                Vector2d axis = box.Extent.x > box.Extent.y ? box.AxisY : box.AxisX;
                double angle = Math.Atan2(axis.y, axis.x) * MathUtil.Rad2Deg;

                GeneralPolygon2d gp = new GeneralPolygon2d(polypart);

                ShellsFillPolygon shells_fill = new ShellsFillPolygon(gp, Settings.FillTypeFactory.Bridge());
                shells_fill.PathSpacing = Settings.SolidFillPathSpacingMM();
                shells_fill.ToolWidth = Settings.Machine.NozzleDiamMM;
                shells_fill.Layers = 1;
                shells_fill.InsetFromInputPolygonX = 0.25;
                shells_fill.FilterSelfOverlaps = false;
                shells_fill.PreserveOuterShells = false;
                shells_fill.Compute();
                scheduler.AppendCurveSets(shells_fill.GetFillCurves());
                var fillPolys = shells_fill.InnerPolygons;

                double offset = Settings.Machine.NozzleDiamMM * Settings.Part.SolidFillBorderOverlapX;
                fillPolys = ClipperUtil.MiterOffset(fillPolys, offset);

                foreach (var fp in fillPolys)
                {
                    BridgeLinesFillPolygon fill_gen = new BridgeLinesFillPolygon(fp, Settings)
                    {
                        InsetFromInputPolygon = false,
                        PathSpacing = spacing,
                        ToolWidth = Settings.Machine.NozzleDiamMM,
                        AngleDeg = angle,
                    };
                    fill_gen.Compute();
                    scheduler.AppendCurveSets(fill_gen.GetFillCurves());
                }
            }
        }

        /// <summary>
        /// Determine the sparse infill and solid fill regions for a layer, given the input regions that
        /// need to be filled, and the roof/floor areas above/below this layer.
        /// </summary>
        protected virtual FillRegions MakeFillRegions(int layerIndex,
            IReadOnlyCollection<GeneralPolygon2d> subject)
        {
            double minArea = Settings.Machine.NozzleDiamMM * Settings.Machine.NozzleDiamMM;

            var solidFillRegions = PartExterior.GetExteriorRegions(layerIndex, subject);

            // [TODO] I think maybe we should actually do another set of contours for the
            // solid region. At least one. This gives the solid & infill something to
            // connect to, and gives the contours above a continuous bonding thread

            // subtract solid fill from infill regions. However because we *don't*
            // inset fill regions, we need to subtract (solid+offset), so that
            // infill won't overlap solid region

            var sparseFillRegions = new List<GeneralPolygon2d>();

            if (solidFillRegions.Count > 0)
            {
                var solidWithBorder = ClipperUtil.MiterOffset(solidFillRegions, Settings.Machine.NozzleDiamMM, minArea);
                sparseFillRegions.AddRange(ClipperUtil.Difference(subject, solidWithBorder, minArea));
            }
            else
            {
                sparseFillRegions.AddRange(subject);
            }

            return new FillRegions(solidFillRegions, sparseFillRegions);
        }

        /// <summary>
        /// schedule any non-polygonal paths for the given layer (eg paths
        /// that resulted from open meshes, for example)
        /// </summary>
        protected virtual void add_open_paths(PrintLayerData layerdata, IFillPathScheduler2d scheduler)
        {
            PlanarSlice slice = layerdata.Slice;
            if (slice.Paths.Count == 0)
                return;

            FillCurveSet2d paths = new FillCurveSet2d();
            for (int pi = 0; pi < slice.Paths.Count; ++pi)
            {
                var pline = new FillCurve<FillSegment>(slice.Paths[pi])
                {
                    FillType = new OpenShellCurveFillType()
                };

                // leave space for end-blobs (input paths are extent we want to hit)
                var trimmed = pline.TrimFrontAndBack(Settings.Machine.NozzleDiamMM / 2);

                // ignore tiny paths
                if (PathFilterF != null && PathFilterF(trimmed) == true)
                    continue;

                paths.Append(trimmed);
            }

            scheduler.AppendCurveSets(new List<FillCurveSet2d>() { paths });
        }

        // The set of perimeter fills for each layer.
        // If we have sparse infill, we need to have multiple shells available to do roof/floors.
        // To do support, we ideally would have them all.
        // Currently we precompute all shell-fills up-front, in precompute_shells().
        // However you could override this behavior, eg do on-demand compute, in GetLayerShells()
        protected List<IShellsFillPolygon>[] LayerShells;

        /// <summary>
        /// return the set of shell-fills for a layer. This includes both the shell-fill paths
        /// and the remaining regions that need to be filled.
        /// </summary>
        protected virtual List<IShellsFillPolygon> get_layer_shells(int layer_i)
        {
            // evaluate shell on-demand
            //if ( LayerShells[layeri] == null ) {
            //    PlanarSlice slice = Slices[layeri];
            //    LayerShells[layeri] = compute_shells_for_slice(slice);
            //}
            return LayerShells[layer_i];
        }

        /// <summary>
        /// compute all the shells for the entire slice-stack
        /// </summary>
        protected virtual void precompute_shells()
        {
            int nLayers = Slices.Count;
            LayerShells = new List<IShellsFillPolygon>[nLayers];

            int max_roof_floor = Math.Max(Settings.Part.RoofLayers, Settings.Part.FloorLayers);
            int start_layer = Math.Max(0, Settings.Part.LayerRangeFilter.a - max_roof_floor);
            int end_layer = Math.Min(nLayers - 1, Settings.Part.LayerRangeFilter.b + max_roof_floor);

            Interval1i interval = new Interval1i(start_layer, end_layer);

#if DEBUG
            for (int layeri = interval.a; layeri <= interval.b; ++layeri)
#else
            gParallel.ForEach(interval, (layeri) =>
#endif
            {
                if (Cancelled()) return;
                PlanarSlice slice = Slices[layeri];
                LayerShells[layeri] = compute_shells_for_slice(slice);
                count_progress_step();
#if DEBUG
            }
#else
            });
#endif
        }

        /// <summary>
        /// compute all the shell-fills for a given slice
        /// </summary>
        protected virtual List<IShellsFillPolygon> compute_shells_for_slice(PlanarSlice slice)
        {
            List<IShellsFillPolygon> layer_shells = new List<IShellsFillPolygon>();
            foreach (GeneralPolygon2d shape in slice.Solids)
            {
                IShellsFillPolygon shells_gen = compute_shells_for_shape(shape, slice.LayerIndex);
                layer_shells.Add(shells_gen);

                if (slice.Tags.Has(shape))
                {
                    lock (ShellTags)
                    {
                        ShellTags.Add(shells_gen, slice.Tags.Get(shape));
                    }
                }
            }
            return layer_shells;
        }

        /// <summary>
        /// compute a shell-fill for the given shape (assumption is that shape.Outer
        /// is anoutermost perimeter)
        /// </summary>
        protected virtual IShellsFillPolygon compute_shells_for_shape(GeneralPolygon2d shape, int layer_i)
        {
            ShellsFillPolygon shells_gen = new ShellsFillPolygon(shape,
                Settings.FillTypeFactory.InnerPerimeter(),
                Settings.FillTypeFactory.OuterPerimeter());

            shells_gen.PathSpacing = Settings.ShellsFillPathSpacingMM();
            shells_gen.ToolWidth = Settings.Machine.NozzleDiamMM;
            shells_gen.Layers = Settings.Part.Shells;
            shells_gen.FilterSelfOverlaps = Settings.Part.ClipSelfOverlaps;
            shells_gen.PreserveOuterShells = true;
            shells_gen.SelfOverlapTolerance = Settings.Part.SelfOverlapToleranceX * Settings.Machine.NozzleDiamMM;
            shells_gen.DiscardTinyPerimeterLengthMM = Settings.Machine.NozzleDiamMM * 2.5;
            shells_gen.DiscardTinyPolygonAreaMM2 = Settings.Machine.NozzleDiamMM * Settings.Machine.NozzleDiamMM * 6.25;
            shells_gen.OuterShellLast = Settings.Part.OuterShellLast;

            shells_gen.Compute();
            return shells_gen;
        }

        /// <summary>
        /// return the set of shell-fills for a layer. This includes both the shell-fill paths
        /// and the remaining regions that need to be filled.
        /// </summary>
        protected virtual List<IShellsFillPolygon> get_layer_skirts(int layer_i)
        {
            if (LayerSkirts != null && layer_i < LayerSkirts.Length)
            {
                return LayerSkirts[layer_i];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// compute all the skirts for a given slice
        /// </summary>
        protected virtual List<IShellsFillPolygon> compute_skirts_for_slice(PlanarSlice slice)
        {
            List<IShellsFillPolygon> layer_skirts = new List<IShellsFillPolygon>();

            // we can compute the resolution as a function of skirt diameter, but probably not worth optimizing
            // const double angularResolution = 64; // number of segments around circle

            // the skirt is computed at each level independently instead of computing the outer path and using offsets
            // this improves the quality of the brim, as it will work in areas that have a narrow gap, and avoids overlaps
            // is some situations (e.g. when the width of the skirt is larger than an inside radius)
            for (int i = Settings.Part.SkirtCount - 1; i >= 0; i--)
            {
                double skirtDistance = Settings.Part.SkirtGap +
                    Settings.Machine.NozzleDiamMM + i * Settings.Part.SkirtSpacingStepX * Settings.Machine.NozzleDiamMM;
                // TODO: Add back angular resolution
                //List<GeneralPolygon2d> dilated = ClipperUtil.RoundOffset(slice.Solids, skirtDistance, -1, angularResolution);
                List<GeneralPolygon2d> dilated = ClipperUtil.RoundOffset(slice.Solids, skirtDistance, -1);

                foreach (GeneralPolygon2d shape in dilated)
                {
                    IShellsFillPolygon skirt_gen = compute_skirts_for_shape(shape, slice.LayerIndex);
                    layer_skirts.Add(skirt_gen);
                }
            }

            return layer_skirts;
        }

        /// <summary>
        /// compute skirts for the given shape, each path is computed independently
        /// </summary>
        protected virtual IShellsFillPolygon compute_skirts_for_shape(GeneralPolygon2d shape, int layer_i)
        {
            ShellsFillPolygon skirt_gen = new ShellsFillPolygon(shape, new SkirtBrimFillType());
            skirt_gen.ToolWidth = Settings.Machine.NozzleDiamMM;
            skirt_gen.PathSpacing = Settings.Machine.NozzleDiamMM * Settings.Part.SkirtSpacingStepX;
            skirt_gen.Layers = 1; // the path is computed indepedently for each distance.
            skirt_gen.FilterSelfOverlaps = Settings.Part.ClipSelfOverlaps;
            skirt_gen.PreserveOuterShells = false;
            skirt_gen.SelfOverlapTolerance = Settings.Part.SelfOverlapToleranceX * Settings.Machine.NozzleDiamMM;
            skirt_gen.OuterShellLast = false;

            skirt_gen.Compute();

            return skirt_gen;
        }

        // Each entry in the list has a collection of FillRegion objects for the layer.
        // The FillRegions are stored in a dictionary with a ShellsFillPolygon as the key
        // so the correct ones for each individual shell can be retrieved, rather than getting
        // all of them for a layer.
        protected List<ShellFillRegionDict> LayerShellFillRegions;

        /// <summary>
        /// compute all the solid/sparse areas for the entire stack, in parallel
        /// </summary>
        protected virtual void precompute_infill_regions()
        {
            int start_layer = Math.Max(0, Settings.Part.LayerRangeFilter.a);
            int end_layer = Math.Min(Slices.Count - 1, Settings.Part.LayerRangeFilter.b);

            LayerShellFillRegions = new List<ShellFillRegionDict>(
                new ShellFillRegionDict[Slices.Count]);

            Interval1i solve_infill_regions = new Interval1i(start_layer, end_layer);

#if DEBUG
            for (int layer_i = solve_infill_regions.a; layer_i <= solve_infill_regions.b; ++layer_i)
#else
            gParallel.ForEach(solve_infill_regions, (layer_i) =>
#endif
            {
                if (Cancelled()) return;
                compute_infill_regions(layer_i);
                count_progress_step();
#if DEBUG
            }
#else
            });
#endif
        }

        protected virtual void compute_infill_regions(int layer_i)
        {
            var regions = new ShellFillRegionDict();

            foreach (var shell in LayerShells[layer_i])
            {
                regions[shell] = MakeFillRegions(layer_i, shell.GetInnerPolygons());
            }

            LayerShellFillRegions[layer_i] = regions;
        }

        // The set of support areas for each layer
        protected List<GeneralPolygon2d>[] LayerSupportAreas;

        /// <summary>
        /// return the set of support-region polygons for a layer.
        /// </summary>
        protected virtual List<GeneralPolygon2d> get_layer_support_area(int layer_i)
        {
            return LayerSupportAreas[layer_i];
        }

        // The set of bridge areas for each layer. These are basically the support
        // areas that we can bridge. So, they are one layer below the model area.
        protected List<GeneralPolygon2d>[] LayerBridgeAreas;

        /// <summary>
        /// return the set of bridgeable support-region polygons for a layer.
        /// Note that the bridge regions for layer i are at layer i-1 (because
        /// these are the support areas)
        /// </summary>
        protected virtual List<GeneralPolygon2d> get_layer_bridge_area(int layer_i)
        {
            return LayerBridgeAreas[layer_i];
        }

        protected List<IShellsFillPolygon>[] LayerSkirts;

        protected virtual void precompute_skirt()
        {
            if (Settings.Part.SkirtLayers == 0 || Settings.Part.SkirtCount == 0)
            {
                return;
            }

            int skirtLayers = Settings.Part.SkirtLayers;
            LayerSkirts = new List<IShellsFillPolygon>[skirtLayers];

            for (int layeri = 0; layeri < skirtLayers; ++layeri)
            {
                PlanarSlice slice = Slices[layeri];
                LayerSkirts[layeri] = compute_skirts_for_slice(slice);

                foreach (var skirt in LayerSkirts[layeri])
                {
                    LayerShellFillRegions[layeri][skirt] = null;
                }
            }
        }

        /// <summary>
        /// compute support volumes for entire slice-stack
        /// </summary>
        protected virtual void precompute_support_areas()
        {
            generate_bridge_areas();

            if (Settings.Part.GenerateSupport)
                generate_support_areas();
            else
                add_existing_support_areas();
        }

        /// <summary>
        /// Find the unsupported regions in each layer that can be bridged
        /// </summary>
        protected virtual void generate_bridge_areas()
        {
            int nLayers = Slices.Count;

            LayerBridgeAreas = new List<GeneralPolygon2d>[nLayers];
            if (nLayers <= 1)
                return;

            // [RMS] does this make sense? maybe should be using 0 here?
            double bridge_tol = Settings.Machine.NozzleDiamMM * 0.5;
            double expand_delta = bridge_tol * 0.1;     // see usage below
            bridge_tol += expand_delta;
            double min_area = Settings.Machine.NozzleDiamMM;
            min_area *= min_area;

            gParallel.ForEach(Interval1i.Range(nLayers - 1), (layeri) =>
            {
                if (Cancelled()) return;
                PlanarSlice slice = Slices[layeri];
                PlanarSlice next_slice = Slices[layeri + 1];

                // To find bridgeable regions, we compute all floating regions in next layer.
                // Then we look for polys that are bridgeable, ie thing enough and fully anchored.
                List<GeneralPolygon2d> bridgePolys = null;
                if (Settings.Part.EnableBridging)
                {
                    // [RMS] bridge area is (next_solids - solids). However, for meshes with slight variations
                    // in identical stacked polygons (eg like created from mesh extrusions), there will be thousands
                    // of tiny polygons. We can filter them, but just computing them can take an enormous amount of time.
                    // So, we slightly offset the slice here. This means the bridge poly will be slightly under-sized,
                    // the assumption is we will be adding extra overlap anyway
                    List<GeneralPolygon2d> expandPolys = ClipperUtil.MiterOffset(slice.Solids, expand_delta, min_area);
                    bridgePolys = ClipperUtil.Difference(next_slice.Solids, expandPolys, min_area);
                    bridgePolys = CurveUtils2.FilterDegenerate(bridgePolys, min_area);
                    bridgePolys = CurveUtils2.Filter(bridgePolys, (p) =>
                    {
                        return layeri > 0 && is_bridgeable(p, layeri, bridge_tol);
                    });
                }

                LayerBridgeAreas[layeri] = bridgePolys != null
                    ? bridgePolys : new List<GeneralPolygon2d>();
            });
            LayerBridgeAreas[nLayers - 1] = new List<GeneralPolygon2d>();
        }

        /// <summary>
        /// Auto-generate the planar solids required to support each area,
        /// and then sweep them downwards.
        /// </summary>
        protected virtual void generate_support_areas()
        {
            var serviceProvider = new ServiceProvider(Settings);
            var generator = serviceProvider.GetSupportAreaGenerator();

            LayerSupportAreas = generator.Compute(
                Slices, LayerBridgeAreas, PathClipRegions, 
                count_progress_step, cancellationToken);
        }

        /// <summary>
        /// Add explicit support solids defined in PlanarSlices. This is called when
        /// Settings.PartProfile.GenerateSupport = false, otherwise these solids are included in
        /// precompute_support_areas().  (todo: have that call this?)
        /// </summary>
        protected virtual void add_existing_support_areas()
        {
            // space we leave between support polygons and solids
            double fSupportGapInLayer = Settings.Part.SupportSolidSpace;

            int nLayers = Slices.Count;
            LayerSupportAreas = new List<GeneralPolygon2d>[nLayers];
            if (nLayers <= 1)
                return;

            gParallel.ForEach(Interval1i.Range(Slices.Count), (i) =>
            {
                if (Cancelled()) return;
                PlanarSlice slice = Slices[i];
                if (slice.SupportSolids.Count == 0)
                    return;

                // if we have explicit support, we can union it in now
                List<GeneralPolygon2d> combineSupport = slice.SupportSolids;

                // simplify each poly
                // [RMS] for existing support we do only a very tiny amount of simplification...
                foreach (GeneralPolygon2d poly in combineSupport)
                {
                    PolySimplification2.Simplify(poly, 0.05 * Settings.Machine.NozzleDiamMM);
                }

                // make sure there is space between solid and support
                List<GeneralPolygon2d> dilatedSolid = ClipperUtil.MiterOffset(slice.Solids, fSupportGapInLayer);
                combineSupport = ClipperUtil.Difference(combineSupport, dilatedSolid);

                LayerSupportAreas[i] = combineSupport;

                count_progress_step();
            });
        }

        /*
         * Bridging and Support utility functions
         */

        /// <summary>
        /// Check if polygon can be bridged. Currently we allow this if all hold:
        /// 1) contracting by max bridge width produces empty polygon
        /// 2) all "turning" vertices of polygon are connected to previous layer
        /// [TODO] not sure this actually guarantees that unsupported distances
        /// *between* turns are within bridge threshold...
        /// </summary>
        protected virtual bool is_bridgeable(GeneralPolygon2d support_poly, int iLayer, double fTolDelta)
        {
            double max_bridge_dist = Settings.Part.MaxBridgeWidthMM;

            // if we inset by half bridge dist, and this doesn't completely wipe out
            // polygon, then it is too wide to bridge, somewhere
            // [TODO] this is a reasonable way to decompose into bridgeable chunks...
            double inset_delta = max_bridge_dist * 0.55;
            List<GeneralPolygon2d> offset = ClipperUtil.MiterOffset(support_poly, -inset_delta);
            if (offset != null && offset.Count > 0)
                return false;

            if (is_fully_connected(support_poly.Outer, iLayer, fTolDelta) == false)
                return false;
            foreach (var h in support_poly.Holes)
            {
                if (is_fully_connected(h, iLayer, fTolDelta) == false)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// check if all turn vertices of poly are connected ( see is_connected(vector2d) )
        /// </summary>
        protected virtual bool is_fully_connected(Polygon2d poly, int iLayer, double fTolDelta)
        {
            int NV = poly.VertexCount;
            for (int k = 0; k < NV; ++k)
            {
                Vector2d v = poly[k];
                if (k > 0 && poly.OpeningAngleDeg(k) > 179)
                    continue;
                if (is_connected(poly[k], iLayer, fTolDelta) == false)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if position is "connected" to a solid in the slice
        /// at layer i, where connected means distance is within tolerance
        /// [TODO] I don't think this will return true if pos is inside one of the solids...
        /// </summary>
        protected virtual bool is_connected(Vector2d pos, int iLayer, double fTolDelta)
        {
            double maxdist = fTolDelta;
            double maxdist_sqr = maxdist * maxdist;

            PlanarSlice slice = Slices[iLayer];
            double dist_sqr = slice.DistanceSquared(pos, maxdist_sqr, true, true);
            if (dist_sqr < maxdist_sqr)
                return true;

            return false;
        }

        /// <summary>
        /// Factory function to return a new PathScheduler to use for this layer.
        /// </summary>
        protected virtual IFillPathScheduler2d get_layer_scheduler(PrintLayerData layer_data)
        {
            SequentialScheduler2d scheduler = new SequentialScheduler2d(
                layer_data.PathAccum, layer_data.Settings, layer_data.Slice.LayerZSpan.b);

            scheduler.SpeedHint = GetSchedulerSpeedHint(layer_data);
            return scheduler;
        }

        protected virtual SpeedHint GetSchedulerSpeedHint(PrintLayerData layer_data)
        {
            return layer_data.layer_i == CurStartLayer ?
                            SpeedHint.Careful : SpeedHint.Rapid;
        }

        protected virtual void count_progress_step()
        {
            Interlocked.Increment(ref CurProgress);
        }

        protected virtual bool Cancelled()
        {
            return cancellationToken.IsCancellationRequested;
        }

        public IEnumerable<string> TotalExtrusionReport
        {
            get
            {
                return Compiler.GenerateTotalExtrusionReport(Settings);
            }
        }

        public PrintTimeStatistics PrintTimeStatistics
        {
            get
            {
                return TotalPrintTimeStatistics;
            }
        }
    }
}