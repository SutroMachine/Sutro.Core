using gs;
using gs.FillTypes;
using gs.utility;
using Sutro.Core.FunctionalTest.FeatureMismatchExceptions;
using Sutro.Core.Models.GCode;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;

namespace Sutro.Core.FunctionalTest
{

    public class ResultAnalyzer<TFeatureInfo> : IResultAnalyzer where TFeatureInfo : IFeatureInfo, new()
    {
        private readonly IFeatureInfoFactory<TFeatureInfo> featureInfoFactory;

        protected sealed class Result
        {
            public ReadOnlyCollection<LayerInfo<TFeatureInfo>> Layers { get; }
            public LayerInfo<TFeatureInfo> Total { get; }

            public Result(IList<LayerInfo<TFeatureInfo>> list)
            {
                Layers = new ReadOnlyCollection<LayerInfo<TFeatureInfo>>(list);
                
                Total = new LayerInfo<TFeatureInfo>();
                foreach (var layer in Layers)
                {
                    foreach (var feature in layer.GetFillTypes())
                    {
                        layer.GetFeatureInfo(feature, out var featureInfo);
                        Total.AddFeatureInfo(featureInfo);
                    }
                }
            }
        }

        public ResultAnalyzer(IFeatureInfoFactory<TFeatureInfo> featureInfoFactory)
        {
            this.featureInfoFactory = featureInfoFactory;
        }

        public ComparisonReport CompareResults(string pathExpected, string pathActual)
        {
            var expected = PerLayerInfoFromGCodeFile(pathExpected);
            var actual = PerLayerInfoFromGCodeFile(pathActual);

            var report = new ComparisonReport();

            if (actual.Layers.Count != expected.Layers.Count)
            {                
                report.AddSummary(new Comparison(false, $"Layers: expected {expected.Layers.Count} layers, actual {actual.Layers.Count}."));
            }
            else
            {
                report.AddSummary(new Comparison(true, $"Layers: {expected.Layers.Count}"));
            }

            foreach (var (featureType, comparison) in actual.Total.Compare(expected.Total))
            {
                report.AddTotal(featureType, comparison);
            }

            int nLayers = Math.Min(actual.Layers.Count, expected.Layers.Count);
            for (int layerIndex = 0; layerIndex < nLayers; layerIndex++)
            {
                report.AddLayer(layerIndex, actual.Layers[layerIndex].Compare(expected.Layers[layerIndex]));
            }
            return report;
        }

        protected virtual GCodeFile LoadGCodeFileFromDisk(string gcodeFilePath)
        {
            var parser = new GenericGCodeParser();
            using var fileReader = File.OpenText(gcodeFilePath);
            return parser.Parse(fileReader);
        }

        protected virtual Result PerLayerInfoFromGCodeFile(string gcodeFilePath)
        {
            return new Result(PerLayerInfoFromGCodeFile(LoadGCodeFileFromDisk(gcodeFilePath)));
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
                    currentLayer = new LayerInfo<TFeatureInfo>();
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