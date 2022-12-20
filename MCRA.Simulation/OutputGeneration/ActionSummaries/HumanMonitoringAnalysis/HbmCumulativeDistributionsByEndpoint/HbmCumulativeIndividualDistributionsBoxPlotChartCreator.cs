using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using System;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDistributionsBoxPlotChartCreator : HbmConcentrationsBoxPlotChartCreatorBase {

        private HbmCumulativeIndividualDistributionsSection _section;
        private string _unit;

        public HbmCumulativeIndividualDistributionsBoxPlotChartCreator(HbmCumulativeIndividualDistributionsSection section, string unit) {
            _section = section;
            _unit = unit;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmBoxPlotRecords.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "f1145c79-846a-4077-a282-7d4c552745f7";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section.HbmBoxPlotRecords, $"Cumulative concentration ({_unit})");
        }
    }
}


