using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class CumulativeIndividualConcentrationsSection : CumulativeConcentrationSectionBase {

        public void Summarize(
            ICollection<AggregateIndividualExposure> targetExposures,
            HbmCumulativeIndividualCollection collection,
            Compound referenceSubstance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            setPercentages(lowerPercentage, upperPercentage);
            TargetUnit = collection.TargetUnit;
            var concentrationAlignmentFactor = collection.TargetUnit
                .GetAlignmentFactor(targetUnit, referenceSubstance.MolecularMass, double.NaN);

            var hbmConcentrations = collection.HbmCumulativeIndividualConcentrations
                .Select(c => (
                    SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                    Concentration: c.CumulativeConcentration * concentrationAlignmentFactor
                ))
                .ToList();
            var substance = new Compound() { Code = "Cumulative" };

            var record = getPercentileRecord(
                hbmConcentrations,
                substance,
                collection.TargetUnit,
                "Monitoring"
            );
            Records.Add(record);

            var exposures = targetExposures
                .Select(r => (
                    SamplingWeight: r.SimulatedIndividual.SamplingWeight,
                    Concentration: r.GetTotalExposureAtTarget(targetUnit.Target, relativePotencyFactors, membershipProbabilities)
                    ))
                .ToList();

            record = getPercentileRecord(
                exposures,
                substance,
                targetUnit,
                "Modelled"
            );
            Records.Add(record);

            Records = [.. Records
                .Where(r => r.MeanPositives > 0)
                .OrderBy(s => s.ExposureRoute)];

        }
    }
}

