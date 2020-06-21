using gs;
using gs.FillTypes;
using gs.utility;
using Sutro.Core.Logging;
using Sutro.Core.Models.GCode;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Sutro.Core.FunctionalTest
{
    public class ResultAnalyzer<TFeatureInfo> : IResultAnalyzer where TFeatureInfo : IFeatureInfo, new()
    {
        private readonly IFeatureInfoFactory<TFeatureInfo> featureInfoFactory;
        private readonly ILogger logger;

        public ResultAnalyzer(IFeatureInfoFactory<TFeatureInfo> featureInfoFactory, ILogger logger)
        {
            this.featureInfoFactory = featureInfoFactory;
            this.logger = logger;
        }

        public void CompareResults(string pathExpected, string pathActual)
        {
            var expected = PerLayerInfoFromGCodeFile(pathExpected);
            var actual = PerLayerInfoFromGCodeFile(pathActual);

            if (actual.Count != expected.Count)
            {
                throw new LayerCountMismatch($"Expected {expected.Count} layers but the result has {actual.Count}.");
            }

            for (int layerIndex = 0; layerIndex < actual.Count; layerIndex++)
            {
                logger.WriteLine($"Checking layer {layerIndex}");
                actual[layerIndex].AssertEqualsExpected(expected[layerIndex]);
            }
        }

        protected virtual GCodeFile LoadGCodeFileFromDisk(string gcodeFilePath)
        {
            var parser = new GenericGCodeParser();
            using var fileReader = File.OpenText(gcodeFilePath);
            return parser.Parse(fileReader);
        }

        protected virtual List<LayerInfo<TFeatureInfo>> PerLayerInfoFromGCodeFile(string gcodeFilePath)
        {
            return PerLayerInfoFromGCodeFile(LoadGCodeFileFromDisk(gcodeFilePath));
        }

        public virtual bool LineIsNewLayer(GCodeLine line)
        {
            return NewLayerPattern.Match(line?.Comment ?? "").Success;
        }

        protected virtual Regex NewLayerPattern => new Regex(@"layer [0-9]+");

        protected virtual List<LayerInfo<TFeatureInfo>> PerLayerInfoFromGCodeFile(GCodeFile gcode)
        {
            featureInfoFactory.Initialize();
            var layers = new List<LayerInfo<TFeatureInfo>>();

            LayerInfo<TFeatureInfo> currentLayer = null;
            string fillType = DefaultFillType.Label;

            foreach (var line in gcode.AllLines())
            {
                if (LineIsNewLayer(line))
                {
                    if (currentLayer != null)
                    {
                        currentLayer.AddFeatureInfo(featureInfoFactory.SwitchFeature(fillType));
                        layers.Add(currentLayer);
                    }
                    currentLayer = new LayerInfo<TFeatureInfo>(logger);
                    continue;
                }

                if (LineIsNewFeatureType(line, fillType, out var newFillType))
                {
                    currentLayer?.AddFeatureInfo(featureInfoFactory.SwitchFeature(newFillType));
                }

                featureInfoFactory.ObserveGcodeLine(line);
            }

            currentLayer?.AddFeatureInfo(featureInfoFactory.SwitchFeature(fillType));
            layers.Add(currentLayer);
            return layers;
        }

        private bool LineIsNewFeatureType(GCodeLine line, string fillType, out string newFillType)
        {
            newFillType = fillType;
            if (GCodeLineUtil.ExtractFillType(line, ref newFillType))
            {
                if (newFillType != fillType)
                {
                    return true;
                }
            }
            return false;
        }
    }
}