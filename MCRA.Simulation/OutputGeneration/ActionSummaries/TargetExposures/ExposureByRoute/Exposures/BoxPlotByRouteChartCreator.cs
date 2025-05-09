﻿using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class BoxPlotByRouteChartCreator : BoxPlotChartCreatorBase {

        private readonly List<ExposureByRoutePercentileRecord> _records;
        private readonly string _unit;
        private readonly bool _showOutliers;

        public BoxPlotByRouteChartCreator(
            List<ExposureByRoutePercentileRecord> records,
            string unit,
            bool showOutliers
        ) {
            _records = records;
            _unit = unit;
            _showOutliers = showOutliers;
            Width = 500;
            Height = 80 + Math.Max(_records.Count * _cellSize, 80);
        }

        public override string Title => $"Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95.";

        public override string ChartId {
            get {
                var pictureId = "1c7a3cb3-c55b-41f9-9ba9-151571d3ab84";
                return StringExtensions.CreateFingerprint(pictureId);
            }
        }

        public override PlotModel Create() {
            return create(
                records: _records.Cast<BoxPlotChartRecord>().ToList(),
                labelHorizontalAxis: $"Exposure ({_unit})",
                showOutliers: _showOutliers
            );
        }
    }
}
