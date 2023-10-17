using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed  class HbmRiskDriverSection : SummarySection {

        public void Summarize(
            SectionHeader header,
            ICollection<HbmIndividualDayCollection> hbmIndividualDayCollections,
            ICollection<HbmIndividualCollection> hbmIndividualCollections,
            HbmCumulativeIndividualDayCollection hbmCumulativeIndividualDayConcentrations,
            HbmCumulativeIndividualCollection hbmCumulativeIndividualConcentrations,
            ICollection<Compound> activeSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            ExposureType exposureType,
            double upperPercentage
        ) {
            SectionHeader subHeader;
            var order = 0;
            if (hbmIndividualCollections != null ||  hbmIndividualDayCollections != null) {
                var section = new HbmTotalDistributionRiskDriversSection() { ProgressState = ProgressState };
                subHeader = header.AddSubSectionHeaderFor(section, "HBM total distribution risk drivers", order++);
                section.Summarize(
                    hbmIndividualDayCollections,
                    hbmIndividualCollections,
                    hbmCumulativeIndividualDayConcentrations,
                    hbmCumulativeIndividualConcentrations,
                    activeSubstances,
                    relativePotencyFactors,
                    exposureType
                );
                subHeader.SaveSummarySection(section);
            }

            if (hbmIndividualCollections != null || hbmIndividualDayCollections != null) {
                var section = new HbmUpperDistributionRiskDriversSection() { ProgressState = ProgressState };
                subHeader = header.AddSubSectionHeaderFor(section, "HBM upper distribution risk drivers", order++);
                section.Summarize(
                    hbmIndividualDayCollections,
                    hbmIndividualCollections,
                    hbmCumulativeIndividualDayConcentrations,
                    hbmCumulativeIndividualConcentrations,
                    activeSubstances,
                    relativePotencyFactors,
                    exposureType,
                    upperPercentage
                );
                subHeader.SaveSummarySection(section);
            }
        }
    }
}
