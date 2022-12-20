using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using System;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmVsModelledIndividualDayConcentrationBySubstanceBoxPlotChartCreator : HbmVsModelledBoxPlotChartCreatorBase {

        private HbmVsModelledIndividualDayConcentrationsBySubstanceSection _section;
        private readonly string _unit;

        public HbmVsModelledIndividualDayConcentrationBySubstanceBoxPlotChartCreator(HbmVsModelledIndividualDayConcentrationsBySubstanceSection section, string unit) {
            _section = section;
            _unit = unit;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmBoxPlotRecords.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "f9b735d9-35c5-4cda-b1b8-c009c0c0eed3";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => "Lower whiskers: p5, p10; box: p25, p50, p75; upper whiskers: p90 and p95; monitoring (blue) vs modelled (green)";

        public override PlotModel Create() {
            return create(_section.HbmBoxPlotRecords, $"Concentration ({_unit})", true);
        }
    }
}


