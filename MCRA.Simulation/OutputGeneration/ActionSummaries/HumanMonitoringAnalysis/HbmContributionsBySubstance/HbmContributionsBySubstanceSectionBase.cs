using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.Stratification;

namespace MCRA.Simulation.OutputGeneration {
    public abstract class HbmContributionsBySubstanceSectionBase
        : HbmContributionsSectionBase<HbmSubstanceContributorKey, HbmContributionBySubstanceRecord> {

        /// <summary>
        /// Acute concentrations by substance.
        /// </summary>
        protected static List<HbmConcentrationsByDescriptor<HbmSubstanceContributorKey>> getConcentrations(
            PopulationStratifier stratifier,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            HbmIndividualDayCollection collection,
            HbmSubstanceContributorKey descriptor,
            HashSet<int> upperIndividuals = null
        ) {
            return [.. collection.HbmIndividualDayConcentrations
                .Where(c => upperIndividuals?.Contains(c.SimulatedIndividualDayId) ?? true)
                .Select(c => new HbmConcentrationsByDescriptor<HbmSubstanceContributorKey> {
                    SamplingWeight = c.SimulatedIndividual.SamplingWeight,
                    TotalEndpointExposure = c.GetSubstanceExposure(descriptor.Substance, relativePotencyFactors, assessmentGroupMemberships),
                    TotalExposure = c.GetCumulativeExposure(relativePotencyFactors, assessmentGroupMemberships),
                    StratificationLevel = stratifier?.GetLevel(c.SimulatedIndividual),
                })];
        }

        /// <summary>
        /// Chronic concentrations by substance.
        /// </summary>
        protected static List<HbmConcentrationsByDescriptor<HbmSubstanceContributorKey>> getConcentrations(
            PopulationStratifier stratifier,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> assessmentGroupMemberships,
            HbmIndividualCollection collection,
            HbmSubstanceContributorKey descriptor,
            HashSet<int> upperIndividuals = null
        ) {
            return [..collection.HbmIndividualConcentrations
                .Where(c => upperIndividuals?.Contains(c.SimulatedIndividual.Id) ?? true)
                .Select(c => new HbmConcentrationsByDescriptor<HbmSubstanceContributorKey> {
                    SamplingWeight = c.SimulatedIndividual.SamplingWeight,
                    TotalEndpointExposure = c.GetSubstanceExposure(descriptor.Substance, relativePotencyFactors, assessmentGroupMemberships),
                    TotalExposure = c.GetCumulativeExposure(relativePotencyFactors, assessmentGroupMemberships),
                    StratificationLevel = stratifier?.GetLevel(c.SimulatedIndividual),
                })];
        }
    }
}
