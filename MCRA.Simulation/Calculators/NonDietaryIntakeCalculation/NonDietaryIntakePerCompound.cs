using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.NonDietaryIntakeCalculation {

    public sealed class NonDietaryIntakePerCompound : IIntakePerCompound {

        public Compound Compound { get; set; }

        public ExposureRouteType Route { get; set; }

        public double Exposure { get; set; }

        /// <summary>
        /// The total (substance) intake, calculated by weighted summing over all exposure routes.
        /// </summary>
        /// <param name="rpf"></param>
        /// <param name="membershipProbability"></param>
        /// <returns></returns>
        public double Intake(double rpf, double membershipProbability) {
            return Exposure * rpf * membershipProbability;
        }
    }
}
