using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.DietaryExposures {
    public sealed class SubstancesOverviewSection : SummarySection {
        public override bool SaveTemporaryData => true;

        public void Summarize(
                SectionHeader header,
                ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
                ICollection<Compound> activeSubstances,
                Compound referenceSubstance,
                ExposureType exposureType,
                double[] selectedPercentiles,
                bool isPerPerson,
                ExposureMethod exposureMethod,
                double[] selectedExposureLevels
            ) {
            var count = 0;
            foreach (var substance in activeSubstances) {
                (var intakes, var weights) = getIntakes(exposureType, dietaryIndividualDayIntakes, substance, isPerPerson);
                if (intakes.Sum() > 0) {
                    var section = new SubstanceDetailSection();
                    var subHeader = header.AddSubSectionHeaderFor(section, getSubSectionTitle(substance), count++);
                    section.Summarize(subHeader, selectedPercentiles, exposureMethod, selectedExposureLevels, referenceSubstance, substance, intakes, weights);
                    subHeader.SaveSummarySection(section);
                }
            }
        }

        public void SummarizeUncertain(
            SectionHeader header,
            ICollection<Compound> activeSubstances,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ExposureType exposureType,
            bool isPerPerson,
            double uncertaintyLowerLimit,
            double uncertaintyUpperLimit
        ) {
            foreach (var substance in activeSubstances) {
                var title = getSubSectionTitle(substance);
                var subHeader = header.GetSubSectionHeaderFromTitleString<SubstanceDetailSection>(title);
                var section = subHeader?.GetSummarySection() as SubstanceDetailSection ?? null;
                if (section != null) {
                    (var intakes, var weights) = getIntakes(exposureType, dietaryIndividualDayIntakes, substance, isPerPerson);
                    section.SummarizeUncertainty(subHeader, intakes, weights, uncertaintyLowerLimit, uncertaintyUpperLimit);
                }
            }
        }

        public string getSubSectionTitle(Compound substance) {
            return $"{substance.Name}, {substance.Code}";
        }

        private (List<double>, List<double>) getIntakes(
            ExposureType exposureType,
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            Compound substance,
            bool isPerPerson
        ) {
            if (exposureType == ExposureType.Acute) {
                var weights = dietaryIndividualDayIntakes.Select(r => r.IndividualSamplingWeight).ToList();
                var exposures = isPerPerson
                              ? dietaryIndividualDayIntakes
                                    .AsParallel()
                                    .Select(c => c.GetSubstanceTotalExposure(substance))
                                    .ToList()
                              : dietaryIndividualDayIntakes
                                    .AsParallel()
                                    .Select(c => c.GetSubstanceTotalExposure(substance) / c.Individual.BodyWeight)
                                    .ToList();
                return (exposures, weights);
            } else {
                var groupedIndividualDayIntakes = dietaryIndividualDayIntakes
                    .GroupBy(c => c.SimulatedIndividualId);
                var weights = groupedIndividualDayIntakes
                    .Select(r => r.First().IndividualSamplingWeight).ToList();
                var exposures = groupedIndividualDayIntakes
                    .Select(c => c.Sum(s => s.GetSubstanceTotalExposure(substance))
                            / c.Count()
                            / (isPerPerson ? 1 : c.First().Individual.BodyWeight)
                    )
                    .ToList();
                return (exposures, weights);
            }
        }
    }
}
