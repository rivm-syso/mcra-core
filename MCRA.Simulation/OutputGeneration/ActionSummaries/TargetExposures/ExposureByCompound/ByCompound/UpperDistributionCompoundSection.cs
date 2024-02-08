using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class UpperDistributionCompoundSection : DistributionCompoundSectionBase {

        public List<DistributionCompoundRecord> Records { get; set; }
        public int NumberOfIntakes { get; set; }
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NRecords { get; set; }

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

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> selectedCompounds,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            double percentageForUpperTail,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            UpperPercentage = 100 - percentageForUpperTail;
            var upperIntakeCalculator = new UpperAggregateIntakeCalculator();
            if (aggregateIndividualExposures != null) {
                var upperIntakes = upperIntakeCalculator.GetUpperTargetIndividualExposures(aggregateIndividualExposures, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);
                Records = Summarize(
                    upperIntakes,
                    selectedCompounds,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                NumberOfIntakes = upperIntakes.Count;
                NRecords = NumberOfIntakes;
                if (NRecords > 0) {
                    LowPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                    HighPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
                }
                CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / aggregateIndividualExposures.Sum(c => c.IndividualSamplingWeight) * 100;

                var substanceCodes = Records.Select(c => c.CompoundCode).ToList();
                foreach (var substance in selectedCompounds) {
                    if (!substanceCodes.Contains(substance.Code)) {
                        Records.Add(new DistributionCompoundRecord() {
                            CompoundCode = substance.Code,
                            CompoundName = substance.Name,
                        });
                    }
                }
            } else {
                var upperIntakes = upperIntakeCalculator.GetUpperTargetIndividualDayExposures(aggregateIndividualDayExposures, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);
                Records = Summarize(
                    upperIntakes,
                    selectedCompounds,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                NumberOfIntakes = upperIntakes.Count;
                NRecords = NumberOfIntakes;
                if (NumberOfIntakes > 0) {
                    LowPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                    HighPercentileValue = upperIntakes.Select(c => c.TotalConcentrationAtTarget(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
                }
                var substanceCodes = Records.Select(c => c.CompoundCode).ToList();
                foreach (var substance in selectedCompounds) {
                    if (!substanceCodes.Contains(substance.Code)) {
                        Records.Add(new DistributionCompoundRecord() {
                            CompoundCode = substance.Code,
                            CompoundName = substance.Name,
                        });
                    }
                }
                CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / aggregateIndividualDayExposures.Sum(c => c.IndividualSamplingWeight) * 100;

            }
            setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double percentageForUpperTail,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            bool isPerPerson
        ) {
            _lowerPercentage = lowerPercentage;
            _upperPercentage = upperPercentage;
            UpperPercentage = 100 - percentageForUpperTail;
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);

            if (exposureType == ExposureType.Acute) {
                Records = SummarizeDietaryAcute(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, isPerPerson);
                NumberOfIntakes = upperIntakes.Count;
                NRecords = NumberOfIntakes;
                if (NumberOfIntakes > 0) {
                    LowPercentileValue = upperIntakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).Min();
                    HighPercentileValue = upperIntakes.Select(c => c.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)).Max();
                }
            } else {
                Records = SummarizeDietaryChronic(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, isPerPerson);
                NumberOfIntakes = upperIntakes.Select(c => c.SimulatedIndividualId).Distinct().Count();
                NRecords = NumberOfIntakes;
                if (NRecords > 0) {
                    var oims = upperIntakes
                        .GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => c.Average(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)))
                        .ToList();
                    LowPercentileValue = oims.Min();
                    HighPercentileValue = oims.Max();
                }
            }

            var substanceCodes = Records.Select(c => c.CompoundCode).ToList();
            foreach (var substance in selectedSubstances) {
                if (!substanceCodes.Contains(substance.Code)) {
                    Records.Add(new DistributionCompoundRecord() {
                        CompoundCode = substance.Code,
                        CompoundName = substance.Name,
                    });
                }
            }
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight) / dietaryIndividualDayIntakes.Sum(c => c.IndividualSamplingWeight) * 100;
            setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ICollection<Compound> selectedSubstances,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            List<DistributionCompoundRecord> records;
            if (aggregateIndividualExposures != null) {
                var upperIntakeCalculator = new UpperAggregateIntakeCalculator();
                var upperIntakes = upperIntakeCalculator.GetUpperTargetIndividualExposures(aggregateIndividualExposures, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);
                records = SummarizeUncertainty(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, isPerPerson);
            } else {
                var upperIntakeCalculator = new UpperAggregateIntakeCalculator();
                var upperIntakes = upperIntakeCalculator.GetUpperTargetIndividualDayExposures(aggregateIndividualDayExposures, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);
                records = SummarizeUncertainty(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, isPerPerson);
            }
            updateContributions(records);
        }

        public void SummarizeUncertainty(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> selectedSubstances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);
            if (exposureType == ExposureType.Acute) {
                var records = SummarizeUncertaintyAcute(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, isPerPerson);
                updateContributions(records);
            } else {
                var records = SummarizeUncertaintyChronic(upperIntakes, selectedSubstances, relativePotencyFactors, membershipProbabilities, isPerPerson);
                updateContributions(records);
            }
        }

        private void updateContributions(List<DistributionCompoundRecord> records) {
            if (records.All(r => double.IsNaN(r.Contribution))) {
                records = records.Where(r => !double.IsNaN(r.Contribution)).ToList();
            }
            foreach (var record in Records) {
                var contribution = records
                    .FirstOrDefault(c => c.CompoundCode == record.CompoundCode)
                    ?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
