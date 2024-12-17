using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {
    public sealed class DustIndividualDayExposure : ExternalIndividualDayExposureBase {

        public Dictionary<ExposureRoute, List<DustExposurePerSubstance>> ExposurePerSubstanceRoute { get; set; }

        public override Dictionary<ExposurePathType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance =>
            ExposurePerSubstanceRoute
                .ToDictionary(
                    item => item.Key.GetExposurePath(),
                    item => item.Value
                        .Cast<IIntakePerCompound>()
                        .ToList() as ICollection<IIntakePerCompound>
                );

        public DustIndividualDayExposure Clone() {
            return new DustIndividualDayExposure() {
                SimulatedIndividualId = SimulatedIndividualId,
                SimulatedIndividualDayId = SimulatedIndividualDayId,
                IndividualSamplingWeight = IndividualSamplingWeight,
                Individual = Individual,
                Day = Day,
                ExposurePerSubstanceRoute = ExposurePerSubstanceRoute
            };
        }
    }
}


