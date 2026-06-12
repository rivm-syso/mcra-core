using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmVsModelledIndividualDayConcentrationsBySubstanceSection : CumulativeConcentrationSectionBase {

        public void Summarize(
            ICollection<AggregateIndividualDayExposure> targetExposures,
            ICollection<HbmIndividualDayCollection> concentrationsCollections,
            ICollection<Compound> substances,
            TargetUnit targetUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            setPercentages(lowerPercentage, upperPercentage);
            foreach (var collection in concentrationsCollections) {
                TargetUnit = collection.TargetUnit;
                foreach (var substance in substances) {
                    var concentrationAlignmentFactor = collection.TargetUnit
                        .GetAlignmentFactor(targetUnit, substance.MolecularMass, double.NaN);

                    var hbmConcentrations = collection.HbmIndividualDayConcentrations
                        .Select(c => (
                            SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                            concentration: c.AverageEndpointSubstanceExposure(substance) * concentrationAlignmentFactor
                        ))
                        .ToList();
                    var record = getPercentileRecord(
                        hbmConcentrations,
                        substance,
                        collection.TargetUnit,
                        "Monitoring"
                    );

                    Records.Add(record);
                    var targetConcentrations = targetExposures
                        .Select(r => (
                            SamplingWeight: r.SimulatedIndividual.SamplingWeight,
                            targetExposure: r.GetSubstanceTargetExposure(targetUnit.Target, substance).Exposure
                        )).ToList();

                    record = getPercentileRecord(
                        targetConcentrations,
                        substance,
                        targetUnit,
                        "Modelled"
                    );
                    Records.Add(record);
                }
            }
        }
    }
}
