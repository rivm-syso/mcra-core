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
        public IDictionary<ExposureRouteType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance { get; set; }

        public static ExternalIndividualDayExposure FromSingleDose(
            ExposureRouteType route,
            Compound compound,
            double dose,
            ExposureUnitTriple targetDoseUnit,
            Individual individual
        ) {
            var absoluteDose = targetDoseUnit.IsPerBodyWeight() ? dose * individual.BodyWeight : dose;
            var exposuresPerRouteCompound = new AggregateIntakePerCompound() {
                Compound = compound,
                Exposure = absoluteDose,
            };
            var result = new ExternalIndividualDayExposure() {
                ExposuresPerRouteSubstance = new Dictionary<ExposureRouteType, ICollection<IIntakePerCompound>>(),
                IndividualSamplingWeight = 1D,
                Individual = individual,
            };
            result.ExposuresPerRouteSubstance[route] = new List<IIntakePerCompound>() { exposuresPerRouteCompound };
            return result;
        }
    }
}
