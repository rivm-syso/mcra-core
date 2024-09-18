using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.RiskCalculation;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.Risk {
    public sealed class SubstanceDetailSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public string Code { get; set; }
        public string Name { get; set; }

        public void Summarize(
            SectionHeader header,
            List<IndividualEffect> individualEffects,
            Compound substance,
            double[] selectedPercentiles,
            double confidenceInterval,
            double threshold,
            RiskMetricType riskMetricType,
            bool isInverseDistribution,
            bool skipPrivacySensitiveOutputs
        ) {
            //if (individualEffects != null && riskMetricType == RiskMetricType.HazardExposureRatio) {
            var sectionGraph = new RiskRatioDistributionSection();
            var subHeaderGraph = header.AddSubSectionHeaderFor(sectionGraph, "Graphs", 1);
            sectionGraph.Summarize(
                confidenceInterval,
                threshold,
                isInverseDistribution,
                individualEffects,
                riskMetricType
            );
            subHeaderGraph.SaveSummarySection(sectionGraph);
            //}
            //if (individualEffects != null && riskMetricType == RiskMetricType.HazardExposureRatio) {
            var sectionPercentiles = new RiskRatioPercentileSection();
            var subHeaderPercentiles = header.AddSubSectionHeaderFor(sectionPercentiles, "Percentiles", 1);
            sectionPercentiles.Summarize(
                individualEffects,
                selectedPercentiles,
                null,
                null,
                RiskMetricCalculationType.RPFWeighted,
                riskMetricType,
                isInverseDistribution,
                false,
                false,
                skipPrivacySensitiveOutputs
            );
            subHeaderPercentiles.SaveSummarySection(sectionPercentiles);
            //}
            //if (individualEffects != null && riskMetricType == RiskMetricType.ExposureHazardRatio) {
            //    var section = new ExposureHazardRatioDistributionSection();
            //    var subHeader = header.AddSubSectionHeaderFor(section, "Graphs", 1);
            //    section.SummarizeHazardExposure(
            //        confidenceInterval,
            //        threshold,
            //        isInverseDistribution,
            //        individualEffects
            //    );
            //    subHeader.SaveSummarySection(section);
            //}
            //if (individualEffects != null && riskMetricType == RiskMetricType.ExposureHazardRatio) {
            //    var section = new ExposureHazardRatioPercentileSection();
            //    var subHeader = header.AddSubSectionHeaderFor(section, "Percentiles", 1);
            //    section.Summarize(
            //        individualEffects,
            //        selectedPercentiles,
            //        null,
            //        null,
            //        RiskMetricCalculationType.RPFWeighted,
            //        isInverseDistribution,
            //        false,
            //        false,
            //        skipPrivacySensitiveOutputs
            //    );
            //    subHeader.SaveSummarySection(section);
            //}

            Code = substance.Code;
            Name = substance.Name;
        }

        public void SummarizeUncertainty(
            SectionHeader header,
            List<IndividualEffect> individualEffects,
            RiskMetricType riskMetricType,
            bool isInverseDistribution,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {

            //if (individualEffects != null && riskMetricType == RiskMetricType.HazardExposureRatio) {
            var subHeaderGraph = header.GetSubSectionHeader<RiskRatioDistributionSection>();
            if (subHeaderGraph != null) {
                var sectionGraph = subHeaderGraph.GetSummarySection() as RiskRatioDistributionSection;
                sectionGraph.SummarizeUncertainty(
                    individualEffects,
                    isInverseDistribution,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound,
                    riskMetricType
                );
                subHeaderGraph.SaveSummarySection(sectionGraph);
            }
            //}

            //if (individualEffects != null && riskMetricType == RiskMetricType.HazardExposureRatio) {
            var subHeaderPercentile = header.GetSubSectionHeader<RiskRatioPercentileSection>();
            if (subHeaderPercentile != null) {
                var sectionPercentile = subHeaderPercentile.GetSummarySection() as RiskRatioPercentileSection;
                sectionPercentile.SummarizeUncertainty(
                    individualEffects,
                    isInverseDistribution,
                    uncertaintyLowerBound,
                    uncertaintyUpperBound
                );
                subHeaderPercentile.SaveSummarySection(sectionPercentile);
            }
            //}

            //if (individualEffects != null && riskMetricType == RiskMetricType.ExposureHazardRatio) {
            //    var subHeader = header.GetSubSectionHeader<ExposureHazardRatioDistributionSection>();
            //    if (subHeader != null) {
            //        var section = subHeader.GetSummarySection() as ExposureHazardRatioDistributionSection;
            //        section.SummarizeUncertainty(
            //            individualEffects,
            //            isInverseDistribution,
            //            uncertaintyLowerBound,
            //            uncertaintyUpperBound
            //        );
            //        subHeader.SaveSummarySection(section);
            //    }
            //}

            //if (individualEffects != null && riskMetricType == RiskMetricType.ExposureHazardRatio) {
            //    var subHeader = header.GetSubSectionHeader<ExposureHazardRatioPercentileSection>();
            //    if (subHeader != null) {
            //        var section = subHeader.GetSummarySection() as ExposureHazardRatioPercentileSection;
            //        section.SummarizeUncertainty(
            //            individualEffects,
            //            isInverseDistribution,
            //            uncertaintyLowerBound,
            //            uncertaintyUpperBound
            //        );
            //        subHeader.SaveSummarySection(section);
            //    }
            //}
        }
    }
}
