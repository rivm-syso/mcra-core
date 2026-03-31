using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.Stratification;
using MCRA.Simulation.OutputGeneration.ActionSummaries.TargetExposures.Generic;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourcePercentilesSection : InternalExposurePercentileSectionBase<SourceContributorKey, ExposureBySourcePercentileRecord> {

        public override string DescriptorKey => "Source";
        public override string DescriptorName => "source";

        public void Summarize(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute Route, Compound substance), double> kineticConversionFactors,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages,
            bool isPerPerson,
            PopulationStratifier outputStratifier
        ) {
            rpfs = substances.Count > 1 ? rpfs : substances.ToDictionary(r => r, r => 1D);
            memberships = substances.Count > 1 ? memberships : substances.ToDictionary(r => r, r => 1D);
            var exposureCollection = ExposureBySourceCalculator.CalculateExposures(
                externalIndividualExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                isPerPerson
            );
            summarize(uncertaintyLowerBound, uncertaintyUpperBound, percentages, outputStratifier, exposureCollection);
        }

        public void SummarizeUncertainty(
            ICollection<IExternalIndividualExposure> externalIndividualExposures,
            ICollection<Compound> substances,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposureRoute Route, Compound substance), double> kineticConversionFactors,
            List<double> percentages,
            bool isPerPerson,
            PopulationStratifier outputStratifier
        ) {
            //var Sources = kineticConversionFactors.Select(c => c.Key.Source).Distinct().ToList();
            rpfs = rpfs ?? substances.ToDictionary(r => r, r => 1D);
            memberships = memberships ?? substances.ToDictionary(r => r, r => 1D);

            var exposureCollection = ExposureBySourceCalculator.CalculateExposures(
                externalIndividualExposures,
                rpfs,
                memberships,
                kineticConversionFactors,
                isPerPerson
            );
            summarize(percentages, outputStratifier, exposureCollection);
        }
    }
}
