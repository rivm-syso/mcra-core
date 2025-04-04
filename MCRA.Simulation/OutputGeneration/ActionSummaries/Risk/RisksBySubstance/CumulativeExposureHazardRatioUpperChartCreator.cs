﻿using MCRA.Utils.ExtensionMethods;
using OxyPlot;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CumulativeExposureHazardRatioUpperChartCreator : CumulativeExposureHazardRatioChartCreatorBase {

        public CumulativeExposureHazardRatioUpperChartCreator(
            CumulativeExposureHazardRatioSection section,
            bool isUncertainty
        ) : base(section, isUncertainty) {
        }

        public override string ChartId {
            get {
                var pictureId = "8edc3312-6386-4054-9e95-c571b8beef25";
                return StringExtensions.CreateFingerprint(_section.SectionId + pictureId);
            }
        }

        public override PlotModel Create() {
            var riskRecords = _section.RiskRecords.SelectMany(c => c.Records).ToList();
            var cumulativeRiskRecord = riskRecords
                .FirstOrDefault(c => c.IsCumulativeRecord);
            var cumulativeRisk = cumulativeRiskRecord != null
                ? (_isUncertainty ? cumulativeRiskRecord.PUpperRiskUncP50 : cumulativeRiskRecord.PUpperRiskNom)
                : double.NaN;
            var orderedHazardRecords = riskRecords
                .Where(c => !c.IsCumulativeRecord)
                .OrderByDescending(c => c.PUpperRiskNom)
                .ToList();

            var orderedExposureHazardRatios = _isUncertainty
                ? orderedHazardRecords.CumulativeWeights(c => c.PUpperRiskUncP50).ToList()
                : orderedHazardRecords.CumulativeWeights(c => c.PUpperRiskNom).ToList();

            var substances = orderedHazardRecords.Select(c => c.SubstanceName).ToList();
            var percentage = 100 - (100 - _section.ConfidenceInterval) / 2;
            return create(
                orderedExposureHazardRatios,
                substances,
                cumulativeRisk,
                percentage
            );
        }
    }
}
