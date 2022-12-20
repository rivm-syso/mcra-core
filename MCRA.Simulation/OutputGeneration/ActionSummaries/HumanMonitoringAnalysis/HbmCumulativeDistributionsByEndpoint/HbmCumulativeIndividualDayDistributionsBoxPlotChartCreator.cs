using MCRA.Utils.ExtensionMethods;
using OxyPlot;
using System;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmCumulativeIndividualDayDistributionsBoxPlotChartCreator : HbmConcentrationsBoxPlotChartCreatorBase { 

        private HbmCumulativeIndividualDayDistributionsSection _section;
        private string _unit;

        public HbmCumulativeIndividualDayDistributionsBoxPlotChartCreator(HbmCumulativeIndividualDayDistributionsSection section, string unit) {
            _section = section;
            _unit = unit;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmBoxPlotRecords.Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "b387b501-8483-4e5f-b9ec-61543576b0a5";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(_section.HbmBoxPlotRecords, $"Cumulative concentration ({_unit})");
        }
    }
}


