﻿using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalCumulativeDistributionChartCreator : CumulativeLineChartCreatorBase {

        private readonly InternalDistributionTotalSection _section;
        private readonly string _intakeUnit;

        public InternalCumulativeDistributionChartCreator(InternalDistributionTotalSection section, string intakeUnit) {
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "ec158786-6264-4f6d-9873-a35777214334";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Cumulative exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives)";

        public override PlotModel Create() {
            return base.createPlotModel(
               _section.Percentiles,
               _section.UncertaintyLowerLimit,
               _section.UncertaintyUpperLimit,
               $"exposure ({_intakeUnit})"
           );
        }
    }
}
