using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators.KarrerKineticModelCalculation {

    public sealed class KarrerReImplementedKineticModelCalculator : DesolvePbkModelCalculator {

        public KarrerReImplementedKineticModelCalculator(
            KineticModelInstance kineticModelInstance
        ) : base(kineticModelInstance) {
        }

        protected override IDictionary<string, double> drawParameters(IDictionary<string, KineticModelInstanceParameter> parameters, IRandom random, bool IsNominal = false, bool useParameterVariability = false) {
            var result = base.drawParameters(parameters, random, IsNominal, KineticModelInstance.UseParameterVariability);
            return result;
        }

        /// <summary>
        /// In the Karrer PBKP model the oral dose is applied in the first 3 minutes (3/60 = 0.05 hour) so for 1 dose a day during 3 minutes it is X /  0.05
        /// Dermal doses are applied for 24 hours, so for 1 dose a day during 24 hours it is X / 24
        /// MCRA supplies for dietary daily doses
        /// Duration (in hours) 3 minutes = 0.05 hour: X / (3/60) = X / 0.05
        /// Duration (in hours) 24 hours: X / (1440/60) = X / 24
        /// For multiple dosings: X / NumberOfDosings /duration. Note duration is 24/NumberOfDosings
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="doses"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        protected override List<double> getUnitDoses(
            IDictionary<string, KineticModelInstanceParameter> parameters,
            List<double> doses,
            ExposurePathType route
        ) {
            var result = new List<double>();
            switch (route) {
                case ExposurePathType.Oral:
                    // is also dermal for Karrer model, based on PCPs;
                    doses.ForEach(c => result.Add(c / KineticModelInstance.NumberOfDosesPerDay));
                    break;
                case ExposurePathType.Dermal:
                    // is dermal for Karrer model, based on Thermal Paper;
                    doses.ForEach(c => result.Add(c / KineticModelInstance.NumberOfDosesPerDayNonDietaryDermal));
                    break;
                case ExposurePathType.Inhalation:
                    doses.ForEach(c => result.Add(c / KineticModelInstance.NumberOfDosesPerDayNonDietaryInhalation));
                    break;
                default:
                    throw new Exception("Route not recognized");
            }
            return result;
        }

        /// <summary>
        /// In Karrer model not state variables are update but local parameters, 
        /// therefore doses for time points that are not events need to set back to zero, so implement all events for every timepoint.
        /// </summary>
        /// <param name="eventsDictionary"></param>
        /// <returns></returns>
        protected override List<int> calculateCombinedEventTimings(IDictionary<ExposurePathType, List<int>> eventsDictionary) {
            var endEvaluationPeriod = KineticModelInstance.NumberOfDays * _timeUnitMultiplier - 1;
            return Enumerable.Range(0, endEvaluationPeriod).ToList();
        }
    }
}
