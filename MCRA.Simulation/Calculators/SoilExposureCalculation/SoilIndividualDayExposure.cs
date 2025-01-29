using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.SoilExposureCalculation {
    public sealed class SoilIndividualDayExposure : ExternalIndividualDayExposureBase {

        public Dictionary<ExposureRoute, List<SoilExposurePerSubstance>> ExposurePerSubstanceRoute { get; set; }

        public override Dictionary<ExposurePathType, ICollection<IIntakePerCompound>> ExposuresPerRouteSubstance =>
            ExposurePerSubstanceRoute
                .ToDictionary(
                    item => item.Key.GetExposurePath(),
                    item => item.Value
                        .Cast<IIntakePerCompound>()
                        .ToList() as ICollection<IIntakePerCompound>
                );

        public SoilIndividualDayExposure Clone() {
            return new SoilIndividualDayExposure() {
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


