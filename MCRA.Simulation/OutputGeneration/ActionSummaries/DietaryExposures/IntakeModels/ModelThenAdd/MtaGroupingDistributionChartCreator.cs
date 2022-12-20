using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class MtaGroupingDistributionChartCreator : HistogramChartCreatorBase {

        private readonly UsualIntakeDistributionPerCategoryModelSection _section;
        private readonly string _intakeUnit;

        public MtaGroupingDistributionChartCreator(
            UsualIntakeDistributionPerCategoryModelSection section,
            string intakeUnit
        ) {
            Height = 400;
            Width = 400;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string Title => "OIM exposure distribution";
        public override string ChartId {
            get {
                var pictureId = "d9bd0d7f-08e6-4d9e-8df4-f828088d9c04";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            return create(
                _section.IntakeDistributionBins,
                _intakeUnit
            );
        }

        private PlotModel create(
            List<HistogramBin> binsTransformed,
            string intakeUnit
        ) {
            var xtitle = $"Exposure ({intakeUnit})";
            return createPlotModel(
                binsTransformed,
                string.Empty,
                xtitle
            );
        }
    }
}

