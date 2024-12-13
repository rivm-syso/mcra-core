using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation {
    public sealed class ExternalIndividualDayExposure : IExternalIndividualDayExposure {

        public Individual Individual { get; set; }
        public double IndividualSamplingWeight { get; set; }
        public string Day { get; set; }
        public int SimulatedIndividualId { get; set; }
        public int SimulatedIndividualDayId { get; set; }
        public Dictionary<ExposurePathType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }

        /// <summary>
        /// Gets the total (cumulative) external exposure expressed
        /// in reference substance equivalents by weighting by relative
        /// potency.
        /// </summary>
        public double GetTotalExternalExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        ) {
            var concentrationMassAlignmentFactor = isPerPerson ? 1D : 1D / Individual.BodyWeight;
            var result = ExposuresPerRouteSubstance
                .Sum(r => r.Value
                    .Sum(ipc => concentrationMassAlignmentFactor
                        * ipc.EquivalentSubstanceAmount(rpfs[ipc.Compound], memberships[ipc.Compound])
                    )
                );
            return result;
        }

        /// <summary>
        /// Gets the total (cumulative) external exposure expressed
        /// in reference substance equivalents by weighting by relative
        /// potency.
        /// </summary>
        public double GetTotalExternalExposure(
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var concentrationMassAlignmentFactor = isPerPerson ? 1D : 1D / Individual.BodyWeight;
            var result = ExposuresPerRouteSubstance
                .Sum(r => r.Value
                    .Sum(ipc => {
                        var exposure = ipc.EquivalentSubstanceAmount(rpfs[ipc.Compound], memberships[ipc.Compound]);
                        return exposure > 0
                            ? exposure * concentrationMassAlignmentFactor * kineticConversionFactors[(r.Key, ipc.Compound)]
                            : 0;
                    })
                );
            return result;
        }

        /// <summary>
        /// Gets the total external exposure for the specified substance.
        /// </summary>
        public double GetTotalExternalExposureForSubstance(
            Compound substance,
            bool isPerPerson
        ) {
            var concentrationMassAlignmentFactor = isPerPerson ? 1D : 1D / Individual.BodyWeight;
            var result = ExposuresPerRouteSubstance
                .Sum(r => r.Value
                    .Where(r => r.Compound == substance)
                    .Sum(ipc => concentrationMassAlignmentFactor * ipc.Amount)
                );
            return result;
        }

        /// <summary>
        /// Gets the total external exposure for the specified substance multiplied by the kinetic conversion factors.
        /// </summary>
        public double GetTotalExternalExposureForSubstance(
            Compound substance,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            var concentrationMassAlignmentFactor = isPerPerson ? 1D : 1D / Individual.BodyWeight;
            var result = ExposuresPerRouteSubstance
                .Sum(r => r.Value
                    .Where(r => r.Compound == substance)
                    .Sum(ipc => {
                        var exposure = concentrationMassAlignmentFactor * ipc.Amount;
                        return exposure > 0
                            ? exposure * kineticConversionFactors[(r.Key, substance)]
                            : 0;
                    })
                );
            return result;
        }

        /// <summary>
        /// Gets the total (cumulative) exposure for the specified route.
        /// </summary>
        /// <returns></returns>
        public double GetTotalRouteExposure(
            ExposurePathType route,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            bool isPerPerson
        ) {
            if (ExposuresPerRouteSubstance.TryGetValue(route, out var exposures)) {
                var totalIntake = exposures
                    .Sum(r => r.EquivalentSubstanceAmount(rpfs[r.Compound], memberships[r.Compound]));
                var exposure = isPerPerson ? totalIntake : totalIntake / Individual.BodyWeight;
                return exposure;
            }
            return 0;
        }

        /// <summary>
        /// Gets the total (cumulative) exposure for the specified route and use kinetic absorption factors.
        /// </summary>
        /// <returns></returns>
        public double GetTotalRouteExposure(
            ExposurePathType route,
            IDictionary<Compound, double> rpfs,
            IDictionary<Compound, double> memberships,
            IDictionary<(ExposurePathType, Compound), double> kineticConversionFactors,
            bool isPerPerson
        ) {
            if (ExposuresPerRouteSubstance.TryGetValue(route, out var exposures)) {
                var totalIntake = exposures
                    .Sum(r => {
                        var exposure = r.EquivalentSubstanceAmount(rpfs[r.Compound], memberships[r.Compound]);
                        return exposure > 0
                            ? exposure * kineticConversionFactors[(route, r.Compound)]
                            : 0;
                    });
                var exposure = isPerPerson ? totalIntake : totalIntake / Individual.BodyWeight;
                return exposure;
            }
            return 0;
        }

        /// <summary>
        /// Gets the total substance exposure for the specified route.
        /// </summary>
        public double GetSubstanceExposureForRoute(
            ExposurePathType route,
            Compound substance,
            bool isPerPerson
        ) {
            if (ExposuresPerRouteSubstance.TryGetValue(route, out var exposures)) {
                var totalIntake = exposures
                    .Where(r => r.Compound == substance)
                    .Sum(r => r.Amount);
                var exposure = isPerPerson
                    ? totalIntake
                    : totalIntake / Individual.BodyWeight;
                return exposure;
            }
            return 0;
        }

        public static ExternalIndividualDayExposure FromSingleDose(
            ExposurePathType route,
            Compound compound,
            double dose,
            ExposureUnitTriple targetDoseUnit,
            Individual individual
        ) {
            var absoluteDose = targetDoseUnit.IsPerBodyWeight() ? dose * individual.BodyWeight : dose;
            var exposuresPerRouteCompound = new AggregateIntakePerCompound() {
                Compound = compound,
                Amount = absoluteDose,
            };
            var result = new ExternalIndividualDayExposure() {
                ExposuresPerRouteSubstance = [],
                IndividualSamplingWeight = 1D,
                Individual = individual,
            };
            result.ExposuresPerRouteSubstance[route] = [exposuresPerRouteCompound];
            return result;
        }

        public ICollection<IIntakePerCompound> GetTotalExposurePerCompound() {
            throw new NotImplementedException();
        }

        public ICollection<IIntakePerCompound> GetTotalExposurePerRouteSubstance(ExposureRoute exposureRoute) {
            throw new NotImplementedException();
        }
    }
}
