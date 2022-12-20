using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HazardDistributionCumulativeChartCreator : CumulativeLineChartCreatorBase {

        private HazardDistributionSection _section;
        private string _exposureUnit;

        public HazardDistributionCumulativeChartCreator(HazardDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _exposureUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "f72099da-c97e-498b-b482-dc304a2c9573";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }
        public override string Title => $"Cumulative individual critical effect dose distribution.";
        public override PlotModel Create() {
            return base.createPlotModel(
                _section.PercentilesGrid,
                _section.UncertaintyLowerLimit,
                _section.UncertaintyUpperLimit,
                $"Individual critical effect dose ({_exposureUnit})"
            );
        }
    }
}
