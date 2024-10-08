using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TotalDistributionRouteCompoundSection : DistributionRouteCompoundSectionBase {

        public List<DistributionRouteCompoundRecord> Records { get; set; }
        public int NumberOfIntakes { get; set; }

        public void Summarize(
            ICollection<AggregateIndividualExposure> aggregateIndividualExposures,
            ICollection<AggregateIndividualDayExposure> aggregateIndividualDayExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            double lowerPercentage,
            double upperPercentage,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            ExposureUnitTriple externalExposureUnit
        ) {
            Percentages = new double[] { lowerPercentage, 50, upperPercentage };
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
            setUncertaintyBounds(uncertaintyLowerBound, uncertaintyUpperBound);
        }

        private void setUncertaintyBounds(double uncertaintyLowerBound, double uncertaintyUpperBound) {
            foreach (var item in Records) {
                item.UncertaintyLowerBound = uncertaintyLowerBound;
                item.UncertaintyUpperBound = uncertaintyUpperBound;
            }
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
                kineticConversionFactors,
                externalExposureUnit
             );
            updateContributions(records);
        }

        private void updateContributions(List<DistributionRouteCompoundRecord> records) {
            foreach (var record in Records) {
                var contribution = records.FirstOrDefault(c => c.ExposureRoute == record.ExposureRoute && c.CompoundCode == record.CompoundCode)?.Contribution * 100 ?? 0;
                record.Contributions.Add(contribution);
            }
        }
    }
}
