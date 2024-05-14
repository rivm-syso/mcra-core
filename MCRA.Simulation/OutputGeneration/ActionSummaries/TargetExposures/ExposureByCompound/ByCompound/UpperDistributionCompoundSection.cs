using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class UpperDistributionCompoundSection : DistributionCompoundSectionBase {
        public List<DistributionCompoundRecord> Records { get; set; }
        public double UpperPercentage { get; set; }
        public double CalculatedUpperPercentage { get; set; }
        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }

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
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            double percentageForUpperTail,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
        ) {
            var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
            Percentages = new double[] { lowerPercentage, 50, upperPercentage };
            UpperPercentage = 100 - percentageForUpperTail;
            var upperIntakeCalculator = new UpperAggregateIntakeCalculator();
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();
            var upperIntakes = upperIntakeCalculator
                .GetUpperTargetIndividualExposures(
                    aggregateExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    percentageForUpperTail,
                    externalExposureUnit,
                    targetUnit
                );
            Records = Summarize(
                upperIntakes,
                substances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit
            );
            NumberOfIntakes = upperIntakes.Count;
            if (NumberOfIntakes > 0) {
                var upperAggregateExposures = upperIntakes
                    .Select(c => c.GetTotalExternalExposure(
                        relativePotencyFactors,
                        membershipProbabilities,
                        kineticConversionFactors,
                        externalExposureUnit.IsPerUnit()
                    ))
                    .ToList();
                LowPercentileValue = upperAggregateExposures.Min();
                HighPercentileValue = upperAggregateExposures.Max();
            }
            CalculatedUpperPercentage = upperIntakes.Sum(c => c.IndividualSamplingWeight)
                / aggregateExposures.Sum(c => c.IndividualSamplingWeight) * 100;

            var substanceCodes = Records.Select(c => c.CompoundCode).ToList();
            foreach (var substance in substances) {
                if (!substanceCodes.Contains(substance.Code)) {
                    Records.Add(new DistributionCompoundRecord() {
                        CompoundCode = substance.Code,
                        CompoundName = substance.Name,
                    });
                }
            }

            setUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }

        public void Summarize(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
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
            Percentages = new double[] { lowerPercentage, 50, upperPercentage };
            UpperPercentage = 100 - percentageForUpperTail;
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(
                    dietaryIndividualDayIntakes,
                    relativePotencyFactors,
                    membershipProbabilities,
                    percentageForUpperTail,
                    isPerPerson
                );

            if (exposureType == ExposureType.Acute) {
                Records = SummarizeDietaryAcute(
                    upperIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                NumberOfIntakes = upperIntakes.Count;
                if (NumberOfIntakes > 0) {
                    var aggregateUpperIntakes = upperIntakes
                        .Select(c => c.TotalExposurePerMassUnit(
                            relativePotencyFactors, 
                            membershipProbabilities, 
                            isPerPerson
                        ))
                        .ToList();
                    LowPercentileValue = aggregateUpperIntakes.Min();
                    HighPercentileValue = aggregateUpperIntakes.Max();
                }
            } else {
                Records = SummarizeDietaryChronic(
                    upperIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                NumberOfIntakes = upperIntakes
                    .Select(c => c.SimulatedIndividualId)
                    .Distinct()
                    .Count();
                if (NumberOfIntakes > 0) {
                    var oims = upperIntakes
                        .GroupBy(c => c.SimulatedIndividualId)
                        .Select(c => c.Average(i => i.TotalExposurePerMassUnit(relativePotencyFactors, membershipProbabilities, isPerPerson)))
                        .ToList();
                    LowPercentileValue = oims.Min();
                    HighPercentileValue = oims.Max();
                }
            }

            var substanceCodes = Records.Select(c => c.CompoundCode).ToHashSet();
            foreach (var substance in substances) {
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
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            ICollection<Compound> substances,
            double percentageForUpperTail,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
         ) {
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();
            var upperIntakeCalculator = new UpperAggregateIntakeCalculator();
            var upperIntakes = upperIntakeCalculator
                .GetUpperTargetIndividualExposures(
                    aggregateExposures,
                    relativePotencyFactors,
                    membershipProbabilities,
                    kineticConversionFactors,
                    percentageForUpperTail,
                    externalExposureUnit,
                    targetUnit
                );
            var records = SummarizeUncertainty(
                upperIntakes,
                substances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit
            );
            updateContributions(records);
        }

        public void SummarizeUncertainty(
            ICollection<DietaryIndividualDayIntake> dietaryIndividualDayIntakes,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            ExposureType exposureType,
            double percentageForUpperTail,
            bool isPerPerson
        ) {
            var upperIntakeCalculator = new UpperDietaryIntakeCalculator(exposureType);
            var upperIntakes = upperIntakeCalculator.GetUpperIntakes(dietaryIndividualDayIntakes, relativePotencyFactors, membershipProbabilities, percentageForUpperTail, isPerPerson);
            if (exposureType == ExposureType.Acute) {
                var records = SummarizeUncertaintyAcute(upperIntakes, substances, relativePotencyFactors, membershipProbabilities, isPerPerson);
                updateContributions(records);
            } else {
                var records = SummarizeUncertaintyChronic(upperIntakes, substances, relativePotencyFactors, membershipProbabilities, isPerPerson);
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
