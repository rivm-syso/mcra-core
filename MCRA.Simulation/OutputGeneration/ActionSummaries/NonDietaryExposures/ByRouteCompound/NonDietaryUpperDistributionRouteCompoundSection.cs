using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class NonDietaryUpperDistributionRouteCompoundSection : NonDietaryDistributionRouteCompoundSectionBase {

        public List<NonDietaryDistributionRouteCompoundRecord> Records { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public int NRecords { get; set; }

        public void Summarize(
            ICollection<Compound> selectedSubstances,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposurePathType> nonDietaryExposureRoutes,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double percentageForUpperTail,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            Percentages = new double[] { lowerPercentage, 50, upperPercentage };
            UpperPercentage = 100 - percentageForUpperTail;
            var upperIntakes = new List<NonDietaryIndividualDayIntake>();
            var upperIntakeCalculator = new NonDietaryUpperExposuresCalculator();
            upperIntakes = upperIntakeCalculator.GetUpperIntakes(
                    nonDietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    exposureType,
                    percentageForUpperTail,
                    isPerPerson
                );

            if (exposureType == ExposureType.Acute) {
                Records = SummarizeAcute(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
                NRecords = upperIntakes.Count;
                if (NRecords > 0) {
                    var nonDietaryUpperIntakes = upperIntakes
                        .Select(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                        .ToList();  
                    LowPercentileValue = nonDietaryUpperIntakes.Min();
                    HighPercentileValue = nonDietaryUpperIntakes.Max();
                }
            } else {
                //Upper deugt nog niet moet op basis van individuals (niet days)
                Records = SummarizeChronic(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
                NRecords = upperIntakes.Select(c => c.SimulatedIndividualId).Distinct().Count();
                if (NRecords > 0) {
                    var oims = upperIntakes
                        .GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => c.Average(i => i.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)))
                        .ToList();
                    LowPercentileValue = oims.Min();
                    HighPercentileValue = oims.Max();
                }
            }
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / nonDietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight) * 100;
            setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }


        public void SummarizeUncertainty(
            ICollection<Compound> selectedSubstances,
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposurePathType> nonDietaryExposureRoutes,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var upperIntakeCalculator = new NonDietaryUpperExposuresCalculator();
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(
                    nonDietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    exposureType,
                    percentageForUpperTail,
                    isPerPerson
                );
            if (exposureType == ExposureType.Acute) {
                var records = SummarizeAcuteUncertainty(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
                updateContributions(records);
            } else {
                var records = SummarizeChronicUncertainty(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
                updateContributions(records);

            }
        }
        private void setUncertaintyBounds(
            List<NonDietaryDistributionRouteCompoundRecord> records,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            foreach (var item in records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }

        private void updateContributions(List<NonDietaryDistributionRouteCompoundRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.CompoundCode == record.CompoundCode && c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
