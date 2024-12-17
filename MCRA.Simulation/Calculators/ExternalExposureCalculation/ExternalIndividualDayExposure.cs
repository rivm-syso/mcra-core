using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.ExternalExposureCalculation {
    public sealed class ExternalIndividualDayExposure : ExternalIndividualDayExposureBase {

        public Dictionary<ExposurePathType, List<IIntakePerCompound>> ExternalExposuresPerPath { get; set; }

        public override Dictionary<ExposurePathType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance =>
            ExternalExposuresPerPath
                .ToDictionary(
                    item => item.Key,
                    item => item.Value
                        .Cast<IIntakePerCompound>()
                        .ToList() as ICollection<IIntakePerCompound>
                );

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
                ExternalExposuresPerPath = [],
                IndividualSamplingWeight = 1D,
                Individual = individual,
            };
            result.ExternalExposuresPerPath[route] = [exposuresPerRouteCompound];
            return result;
        }
    }
}
