using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.NonDietaryIntakeCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class NonDietaryUpperDistributionRouteSection : NonDietaryDistributionRouteSectionBase {
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public List<NonDietaryDistributionRouteRecord> Records { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NRecords { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <param name="nonDietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="nonDietaryExposureRoutes"></param>
        /// <param name="exposureType"></param>
        /// <param name="percentageForUpperTail"></param>
        /// <param name="uncertaintyLowerBound"></param>
        /// <param name="uncertaintyUpperBound"></param>
        /// <param name="isPerPerson"></param>
        ///
        public void Summarize(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposurePathType> nonDietaryExposureRoutes,
            ExposureType exposureType,
            double percentageForUpperTail,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            Percentages = [lowerPercentage, 50, upperPercentage];
            UpperPercentage = 100 - percentageForUpperTail;
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
                Records = SummarizeAcute(upperIntakes, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
                NRecords = upperIntakes.Count;
                if (NRecords > 0) {
                    var nonDietaryUpperIntakes = upperIntakes
                        .Select(c => c.ExternalTotalNonDietaryIntakePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson))
                        .ToList();
                    LowPercentileValue = nonDietaryUpperIntakes.Min();
                    HighPercentileValue = nonDietaryUpperIntakes.Max();
                }
            } else {
                Records = SummarizeChronic(upperIntakes, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
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

        /// <summary>
        /// Summarize uncertainty
        /// </summary>
        /// <param name="nonDietaryIndividualDayIntakes"></param>
        /// <param name="relativePotencyFactors"></param>
        /// <param name="membershipProbabilities"></param>
        /// <param name="nonDietaryExposureRoutes"></param>
        /// <param name="exposureType"></param>
        /// <param name="isPerPerson"></param>
        ///
        public void SummarizeUncertainty(
            ICollection<NonDietaryIndividualDayIntake> nonDietaryIndividualDayIntakes,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<ExposurePathType> nonDietaryExposureRoutes,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var upperIntakeCalculator = new NonDietaryUpperExposuresCalculator();
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(nonDietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, exposureType, percentageForUpperTail, isPerPerson);
            if (exposureType == ExposureType.Acute) {
                var records = SummarizeAcuteUncertainty(upperIntakes, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
                updateContributions(records);

            } else {
                var records = SummarizeChronicUncertainty(upperIntakes, relativePotencyFactors, membershipProbabilities, nonDietaryExposureRoutes, isPerPerson);
                updateContributions(records);
            }
        }
        private void setUncertaintyBounds(
            List<NonDietaryDistributionRouteRecord> records,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            foreach (var item in records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }

        private void updateContributions(List<NonDietaryDistributionRouteRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
