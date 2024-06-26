﻿using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDistributionCompoundSection : DistributionCompoundSectionBase {

        public List<DistributionCompoundRecord> Records { get; set; }
        public List<IndividualCompoundIntakeRecord> IndividualCompoundIntakeRecords { get; set; }
        public int NumberOfIntakes { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            Percentages = new double[] { lowerPercentage, 50, upperPercentage };
            if (aggregateIndividualExposures != null) {
                NumberOfIntakes = aggregateIndividualExposures.Count;
                Records = Summarize(
                    aggregateIndividualExposures,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
            } else {
                NumberOfIntakes = aggregateIndividualDayExposures.Count;
                Records = Summarize(
                    aggregateIndividualDayExposures,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
            }
            setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            Percentages = new double[] { lowerPercentage, 50, upperPercentage };
            if (exposureType == ExposureType.Acute) {
                Records = SummarizeDietaryAcute(
                    dietaryIndividualDayIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                NumberOfIntakes = dietaryIndividualDayIntakes.Count;
            } else {
                Records = SummarizeDietaryChronic(
                    dietaryIndividualDayIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                NumberOfIntakes = dietaryIndividualDayIntakes
                    .Select(c => c.SimulatedIndividualId)
                    .Distinct()
                    .Count();
            }
            setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> substances,
            bool isPerPerson
        ) {
            List<DistributionCompoundRecord> records;
            if (aggregateIndividualExposures != null) {
                records = SummarizeUncertainty(
                    aggregateIndividualExposures,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
            } else {
                records = SummarizeUncertainty(
                    aggregateIndividualDayExposures,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
            }
            updateContributions(records);
        }

        public void SummarizeUncertainty(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            bool isPerPerson
        ) {
            if (exposureType == ExposureType.Acute) {
                var distributionCompoundRecords = SummarizeUncertaintyAcute(
                    dietaryIndividualDayIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                updateContributions(distributionCompoundRecords);
            } else {
                var distributionCompoundRecords = SummarizeUncertaintyChronic(
                    dietaryIndividualDayIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                updateContributions(distributionCompoundRecords);
            }
        }

        private void setUncertaintyBounds(
            List<DistributionCompoundRecord> records,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound
        ) {
            foreach (var item in records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
        }
        private void updateContributions(List<DistributionCompoundRecord> records) {
            if (records.All(r => double.IsNaN(r.Contribution))) {
                records = records.Where(r => !double.IsNaN(r.Contribution)).ToList();
            }
            records = records.Where(r => !double.IsNaN(r.Contribution)).ToList();
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.CompoundCode == record.CompoundCode)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
