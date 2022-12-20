using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics.Histograms;
using OxyPlot;
using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelBasedChartCreator : HistogramChartCreatorBase {

        private ModelBasedDistributionSection _section;
        private string _intakeUnit;

        public ModelBasedChartCreator(ModelBasedDistributionSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }


        public override string Title => "Model based usual exposure distribution";

        public override string ChartId {
            get {
                var pictureId = "eba2bf40-07af-47e9-b4d6-f635aeedd713";
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

