using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Simulation.Calculators.UpperIntakesCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class UpperDistributionCompoundSection : DistributionCompoundSectionBase {
        public double UpperPercentage { get; set; } = double.NaN;

        public double LowPercentileValue { get; set; }
        public double HighPercentileValue { get; set; }
        public int NumberOfIntakes { get; set; }

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
            if (substances.Count ==1 || relativePotencyFactors != null) {
                var cancelToken = ProgressState?.CancellationToken ?? new CancellationToken();
                Percentages = [lowerPercentage, 50, upperPercentage];
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
                SetUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
            } 
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
            Percentages = [lowerPercentage, 50, upperPercentage];
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
            SetUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
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
            if (substances.Count == 1 || relativePotencyFactors != null) {
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
                UpdateContributions(records);
            }
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
                UpdateContributions(records);
            } else {
                var records = SummarizeUncertaintyChronic(upperIntakes, substances, relativePotencyFactors, membershipProbabilities, isPerPerson);
                UpdateContributions(records);
            }
        }
    }
}
