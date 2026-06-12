using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation.AggregateExposures;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class CumulativeDayConcentrationsSection : CumulativeConcentrationSectionBase {

        public void Summarize(
            ICollection<AggregateIndividualDayExposure> targetExposures,
            HbmCumulativeIndividualDayCollection dayCollection,
            Compound referenceSubstance,
            IDictionary<Compound, double> relativePotencyFactors,
            IDictionary<Compound, double> membershipProbabilities,
            TargetUnit targetUnit,
            double lowerPercentage,
            double upperPercentage
        ) {
            setPercentages(lowerPercentage, upperPercentage);
            TargetUnit = dayCollection.TargetUnit;
            var concentrationAlignmentFactor = TargetUnit
                .GetAlignmentFactor(targetUnit, referenceSubstance.MolecularMass, double.NaN);

            var hbmConcentrations = dayCollection.IndividualDayConcentrations
                .Select(c => (
                    SamplingWeight: c.SimulatedIndividual.SamplingWeight,
                    Concentration: c.CumulativeConcentration * concentrationAlignmentFactor
                ))
                .ToList();

            var record = getPercentileRecord(
                hbmConcentrations,
                new Compound() { Code = "Cumulative" },
                TargetUnit,
                "Monitoring"
            );

            Records.Add(record);
            var exposuresXX = targetExposures
                .Select(r => (
                    SamplingWeight: r.SimulatedIndividual.SamplingWeight,
                    Concentration: r.GetTotalExposureAtTarget(targetUnit.Target, relativePotencyFactors, membershipProbabilities)
                ))
                .ToList();

            var exposures = targetExposures
               .Select(r => (
                   SamplingWeight: r.SimulatedIndividual.SamplingWeight,
                   Concentration: r.GetTotalExposureAtTarget(targetUnit.Target, relativePotencyFactors, membershipProbabilities)
               ))
               .ToList();

            record = getPercentileRecord(
                exposures,
                new Compound() { Code = "Cumulative" },
                targetUnit,
                "Modelled"
            );
            Records.Add(record);

            Records = [.. Records
                .Where(r => r.MeanPositives > 0)
                .OrderBy(s => s.ExposureRoute)
            ];
        }
    }
}
