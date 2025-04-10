﻿using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators.KarrerKineticModelCalculation {

    public class KarrerKineticModelCalculator : DesolvePbkModelCalculator {

        public KarrerKineticModelCalculator(
            KineticModelInstance kineticModelInstance,
            PbkSimulationSettings simulationSettings
        ) : base(kineticModelInstance, simulationSettings) {
        }

        protected override IDictionary<string, double> drawParameters(
            IDictionary<string, KineticModelInstanceParameter> parameters,
            IRandom random,
            bool isNominal = false,
            bool useParameterVariability = false
        ) {
            var result = base.drawParameters(parameters, random, isNominal, useParameterVariability);
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
            ExposureRoute route
        ) {
            var result = new List<double>();
            switch (route) {
                case ExposureRoute.Oral:
                    //  is also dermal for Karrer model, based on PCPs;
                    doses.ForEach(c => result.Add(c / parameters["period_O"].Value / SimulationSetings.NumberOfDosesPerDay));
                    break;
                case ExposureRoute.Dermal:
                    // is dermal for Karrer model, based on Thermal Paper;
                    doses.ForEach(c => result.Add(c / parameters["period_D"].Value / SimulationSetings.NumberOfDosesPerDayNonDietaryDermal));
                    break;
                case ExposureRoute.Inhalation:
                    doses.ForEach(c => result.Add(c / parameters["period_D2"].Value / SimulationSetings.NumberOfDosesPerDayNonDietaryInhalation));
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
            if (SimulationSetings.NumberOfSimulatedDays >= 1) {
                parameters["t0_O1_day1"] = 0;
                parameters["t0_D1_day1"] = 0;
                parameters["t0_D21_day1"] = 0;
                if (SimulationSetings.NumberOfDosesPerDay >= 2) {
                    parameters["t0_O2_day1"] = 12;
                }
                if (SimulationSetings.NumberOfDosesPerDayNonDietaryDermal >= 2) {
                    parameters["t0_D2_day1"] = 12;
                    parameters["t0_D22_day1"] = 12;
                }
                if (SimulationSetings.NumberOfDosesPerDay >= 3) {
                    parameters["t0_O3_day1"] = 6;
                }
            }
            //day 2
            if (SimulationSetings.NumberOfSimulatedDays >= 2) {
                parameters["t0_O1_day2"] = 24;
                parameters["t0_D1_day2"] = 24;
                parameters["t0_D21_day2"] = 24;
                if (SimulationSetings.NumberOfDosesPerDay >= 2) {
                    parameters["t0_O2_day2"] = 36;
                }
                if (SimulationSetings.NumberOfDosesPerDayNonDietaryDermal >= 2) {
                    parameters["t0_D2_day2"] = 36;
                    parameters["t0_D22_day2"] = 36;
                }
                if (SimulationSetings.NumberOfDosesPerDay >= 3) {
                    parameters["t0_O3_day2"] = 30;
                }
            }
            //day 3
            if (SimulationSetings.NumberOfSimulatedDays >= 3) {
                parameters["t0_O1_day3"] = 48;
                parameters["t0_D1_day3"] = 48;
                parameters["t0_D21_day3"] = 48;
                if (SimulationSetings.NumberOfDosesPerDay >= 2) {
                    parameters["t0_O2_day3"] = 60;
                }
                if (SimulationSetings.NumberOfDosesPerDayNonDietaryDermal >= 2) {
                    parameters["t0_D2_day3"] = 60;
                    parameters["t0_D22_day3"] = 60;
                }
                if (SimulationSetings.NumberOfDosesPerDay >= 3) {
                    parameters["t0_O3_day3"] = 54;
                }
            }
            //day 4
            if (SimulationSetings.NumberOfSimulatedDays >= 4) {
                parameters["t0_O1_day4"] = 72;
                parameters["t0_D1_day4"] = 72;
                parameters["t0_D21_day4"] = 72;
                if (SimulationSetings.NumberOfDosesPerDay >= 2) {
                    parameters["t0_O2_day4"] = 84;
                }
                if (SimulationSetings.NumberOfDosesPerDayNonDietaryDermal >= 2) {
                    parameters["t0_D2_day4"] = 84;
                    parameters["t0_D22_day4"] = 84;
                }
                if (SimulationSetings.NumberOfDosesPerDay >= 3) {
                    parameters["t0_O3_day4"] = 78;
                }
            }
            return parameters;
        }

        protected override List<int> calculateCombinedEventTimings(IDictionary<ExposureRoute, List<int>> eventsDictionary) {
            return [0];
        }

        /// <summary>
        /// Only one dose per route needed because within c-code the dosing pattern 
        /// is implemented based on starting times en application period
        /// </summary>
        /// <param name="allEvents"></param>
        /// <param name="events"></param>
        /// <param name="doses"></param>
        /// <returns></returns>
        protected override List<double> combineDosesWithEvents(
            List<int> allEvents,
            List<int> events,
            List<double> doses
        ) {
            var dosesDict = allEvents.ToDictionary(r => r, r => 0D);
            dosesDict[0] = doses[0];
            return dosesDict.Values.ToList();
        }
    }
}
