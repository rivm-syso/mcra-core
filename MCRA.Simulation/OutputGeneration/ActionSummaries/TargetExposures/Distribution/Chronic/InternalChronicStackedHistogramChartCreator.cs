﻿using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class InternalChronicStackedHistogramChartCreator : StackedHistogramChartCreatorBase {

        private readonly InternalDistributionTotalSection _section;
        private readonly string _intakeUnit;

        public InternalChronicStackedHistogramChartCreator(InternalDistributionTotalSection section, string intakeUnit) {
            ShowContributions = false;
            Width = 500;
            Height = 350;
            _section = section;
            _intakeUnit = intakeUnit;
        }

        public override string ChartId {
            get {
                var pictureId = "4244e496-1685-4711-91f0-abed63c19c41";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override string Title => $"Stacked transformed exposure distribution ({100 - _section.PercentageZeroIntake:F1}% positives).";

        public override PlotModel Create() {
            return create(
                _section.CategorizedHistogramBins,
                $"Exposure ({_intakeUnit})"
            );
        }
    }
}