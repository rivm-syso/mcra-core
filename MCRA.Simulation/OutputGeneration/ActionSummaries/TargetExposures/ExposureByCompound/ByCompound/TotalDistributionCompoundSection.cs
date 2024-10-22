using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDistributionCompoundSection : DistributionCompoundSectionBase {
       
        public int NumberOfIntakes { get; set; }
        public ExposureTarget ExposureTarget { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            ICollection<Compound> substances,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            ExposureUnitTriple externalExposureUnit,
            TargetUnit targetUnit
        ) {
            ExposureTarget = targetUnit.Target;
            Percentages = [lowerPercentage, 50, upperPercentage];
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();
            NumberOfIntakes = aggregateExposures.Count;
            Records = Summarize(
                aggregateExposures,
                substances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit
            );
            SetUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
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
            Percentages = [lowerPercentage, 50, upperPercentage];
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
            SetUncertaintyBounds(Records, uncertaintyLowerBound, uncertaintyUpperBound);
        }

        public void SummarizeUncertainty(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            ICollection<Compound> substances,
            ExposureUnitTriple externalExposureUnit
        ) {
            var aggregateExposures = aggregateIndividualExposures != null
                ? aggregateIndividualExposures
                : aggregateIndividualDayExposures.Cast<AggregateIndividualExposure>().ToList();
            var records = SummarizeUncertainty(
                aggregateExposures,
                substances,
                relativePotencyFactors,
                membershipProbabilities,
                kineticConversionFactors,
                externalExposureUnit
            );
            UpdateContributions(records);
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
                var records = SummarizeUncertaintyAcute(
                    dietaryIndividualDayIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                UpdateContributions(records);
            } else {
                var records = SummarizeUncertaintyChronic(
                    dietaryIndividualDayIntakes,
                    substances,
                    relativePotencyFactors,
                    membershipProbabilities,
                    isPerPerson
                );
                UpdateContributions(records);
            }
        }

        //private void setUncertaintyBounds(
        //    List<DistributionCompoundRecord> records,
        //    double uncertaintyLowerBound,
        //    double uncertaintyUpperBound
        //) {
        //    foreach (var item in records) {
        //        item.UncertaintyLowerBound = uncertaintyLowerBound;
        //        item.UncertaintyUpperBound = uncertaintyUpperBound;
        //    }
        //}
        //private void updateContributions(List<DistributionCompoundRecord> records) {
        //    records = records.Where(r => !double.IsNaN(r.Contribution)).ToList();
        //    foreach (var record in Records) {
        //        var contribution = records.FirstOrDefault(c => c.CompoundCode == record.CompoundCode)
        //            ?.Contribution * 100
        //            ?? 0;
        //        record.Contributions.Add(contribution);
        //    }
        //}
    }
}
