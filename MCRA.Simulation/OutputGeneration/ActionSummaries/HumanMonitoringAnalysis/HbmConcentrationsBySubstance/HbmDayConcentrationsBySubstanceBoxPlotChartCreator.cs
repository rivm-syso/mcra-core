using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmDayConcentrationsBySubstanceBoxPlotChartCreator : HbmConcentrationsBoxPlotChartCreatorBase { 

        private readonly HbmIndividualDayDistributionBySubstanceSection _section;
        private readonly ExposureTarget _target;
        private readonly string _unit;

        public HbmDayConcentrationsBySubstanceBoxPlotChartCreator(
            HbmIndividualDayDistributionBySubstanceSection section,
            ExposureTarget target,
            string unit
        ) {
            _section = section;
            _unit = unit;
            _target = target;
            Width = 500;
            Height = 80 + Math.Max(_section.HbmBoxPlotRecords[_target].Count * _cellSize, 80);
        }

        public override string ChartId {
            get {
                var pictureId = "68f6d4e5-6076-4d3d-9b55-8d5b67d38fd0";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId + _target.Code);
            }
        }

        public override PlotModel Create() {
            return create(_section.HbmBoxPlotRecords[_target], $"Concentration ({_unit})");
        }
    }
}
