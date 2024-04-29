using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators.KarrerKineticModelCalculation {

    public class KarrerKineticModelCalculator : DesolvePbkModelCalculator {

        public KarrerKineticModelCalculator(
            KineticModelInstance kineticModelInstance,
            IDictionary<ExposurePathType, double> defaultAbsorptionFactors
        ) : base(kineticModelInstance, defaultAbsorptionFactors) {
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
                case ExposurePathType.Dietary:
                    doses.ForEach(c => result.Add(c / parameters["period_O"].Value / KineticModelInstance.NumberOfDosesPerDay));
                    break;
                case ExposurePathType.Oral:
                    //  is also dermal for Karrer model, based on PCPs;
                    doses.ForEach(c => result.Add(c / parameters["period_O"].Value / KineticModelInstance.NumberOfDosesPerDay));
                    break;
                case ExposurePathType.Dermal:
                    // is dermal for Karrer model, based on Thermal Paper;
                    doses.ForEach(c => result.Add(c / parameters["period_D"].Value / KineticModelInstance.NumberOfDosesPerDayNonDietaryDermal));
                    break;
                case ExposurePathType.Inhalation:
                    doses.ForEach(c => result.Add(c / parameters["period_D2"].Value / KineticModelInstance.NumberOfDosesPerDayNonDietaryInhalation));
                    break;
                default:
                    throw new Exception("Route not recognized");
            }
            return result;
        }

        /// <summary>
        /// Dedicated events pattern Karrer model. Note all starting points are initialized with zero.
        /// That is t0.O1, t0.O2, t0.O3, t0.D1, t0.D2, t0.D21 and t0.D22 for day1...day4 are set to zero
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected override IDictionary<string, double> setStartingEvents(IDictionary<string, double> parameters) {
            //day 1
            if (KineticModelInstance.NumberOfDays >= 1) {
                parameters["t0_O1_day1"] = 0;
                parameters["t0_D1_day1"] = 0;
                parameters["t0_D21_day1"] = 0;
                if (KineticModelInstance.NumberOfDosesPerDay >= 2) {
                    parameters["t0_O2_day1"] = 12;
                }
                if (KineticModelInstance.NumberOfDosesPerDayNonDietaryDermal >= 2) {
                    parameters["t0_D2_day1"] = 12;
                    parameters["t0_D22_day1"] = 12;
                }
                if (KineticModelInstance.NumberOfDosesPerDay >= 3) {
                    parameters["t0_O3_day1"] = 6;
                }
            }
            //day 2
            if (KineticModelInstance.NumberOfDays >= 2) {
                parameters["t0_O1_day2"] = 24;
                parameters["t0_D1_day2"] = 24;
                parameters["t0_D21_day2"] = 24;
                if (KineticModelInstance.NumberOfDosesPerDay >= 2) {
                    parameters["t0_O2_day2"] = 36;
                }
                if (KineticModelInstance.NumberOfDosesPerDayNonDietaryDermal >= 2) {
                    parameters["t0_D2_day2"] = 36;
                    parameters["t0_D22_day2"] = 36;
                }
                if (KineticModelInstance.NumberOfDosesPerDay >= 3) {
                    parameters["t0_O3_day2"] = 30;
                }
            }
            //day 3
            if (KineticModelInstance.NumberOfDays >= 3) {
                parameters["t0_O1_day3"] = 48;
                parameters["t0_D1_day3"] = 48;
                parameters["t0_D21_day3"] = 48;
                if (KineticModelInstance.NumberOfDosesPerDay >= 2) {
                    parameters["t0_O2_day3"] = 60;
                }
                if (KineticModelInstance.NumberOfDosesPerDayNonDietaryDermal >= 2) {
                    parameters["t0_D2_day3"] = 60;
                    parameters["t0_D22_day3"] = 60;
                }
                if (KineticModelInstance.NumberOfDosesPerDay >= 3) {
                    parameters["t0_O3_day3"] = 54;
                }
            }
            //day 4
            if (KineticModelInstance.NumberOfDays >= 4) {
                parameters["t0_O1_day4"] = 72;
                parameters["t0_D1_day4"] = 72;
                parameters["t0_D21_day4"] = 72;
                if (KineticModelInstance.NumberOfDosesPerDay >= 2) {
                    parameters["t0_O2_day4"] = 84;
                }
                if (KineticModelInstance.NumberOfDosesPerDayNonDietaryDermal >= 2) {
                    parameters["t0_D2_day4"] = 84;
                    parameters["t0_D22_day4"] = 84;
                }
                if (KineticModelInstance.NumberOfDosesPerDay >= 3) {
                    parameters["t0_O3_day4"] = 78;
                }
            }
            return parameters;
        }
        protected override List<int> calculateCombinedEventTimings(IDictionary<ExposurePathType, List<int>> eventsDictionary) {
            return new List<int> { 0 };
        }

        /// <summary>
        /// Only one dose per route needed because within c-code the dosing pattern in implemented based on starting times en application period
        /// </summary>
        /// <param name="allEvents"></param>
        /// <param name="events"></param>
        /// <param name="doses"></param>
        /// <returns></returns>
        protected override List<double> combineDosesWithEvents(List<int> allEvents, List<int> events, List<double> doses) {
            var dosesDict = allEvents.ToDictionary(r => r, r => 0D);
            dosesDict[0] = doses[0];
            return dosesDict.Values.ToList();
        }

        protected override double getRelativeCompartmentWeight(
            KineticModelOutputDefinition outputParameter,
            IDictionary<string, double> parameters
        ) {
            return 1;
        }
    }
}
