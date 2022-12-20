using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using System;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CumulativeDayConcentrationsBoxPlotChartCreator : HbmVsModelledBoxPlotChartCreatorBase {

        private CumulativeDayConcentrationsSection _section;
        private readonly string _unit;

        public CumulativeDayConcentrationsBoxPlotChartCreator(CumulativeDayConcentrationsSection section, string unit) {
            _section = section;
            _unit = unit;
            Width = 500;
            Height = 80 + Math.Max(_section.Records.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "ac5f7844-88db-4acc-bfa3-877e834f20ba";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90, p95; monitoring vs modelled";

        public override PlotModel Create() {
            return create(_section.Records, $"Concentration ({_unit})", true);
        }
    }
}
