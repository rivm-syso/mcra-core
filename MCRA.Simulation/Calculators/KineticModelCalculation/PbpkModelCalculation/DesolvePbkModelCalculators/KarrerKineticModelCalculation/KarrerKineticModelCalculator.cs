using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators.KarrerKineticModelCalculation {

    public class KarrerKineticModelCalculator : DesolvePbkModelCalculator {

        public KarrerKineticModelCalculator(
            KineticModelInstance kineticModelInstance,
            PbkSimulationSettings simulationSettings
        ) : base(kineticModelInstance, simulationSettings) {
        }

        /// <summary>
        /// Dedicated events pattern Karrer model. Note all starting points are initialized with zero.
        /// That is t0.O1, t0.O2, t0.O3, t0.D1, t0.D2, t0.D21 and t0.D22 for day1...day4 are set to zero.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected override void setStartingEvents(IDictionary<string, double> parameters) {
            //day 1
            if (SimulationSettings.NumberOfSimulatedDays >= 1) {
                parameters["t0_O1_day1"] = 0;
                parameters["t0_D1_day1"] = 0;
                parameters["t0_D21_day1"] = 0;
                if (SimulationSettings.NumberOfOralDosesPerDay >= 2) {
                    parameters["t0_O2_day1"] = 12;
                }
                if (SimulationSettings.NumberOfDermalDosesPerDay >= 2) {
                    parameters["t0_D2_day1"] = 12;
                    parameters["t0_D22_day1"] = 12;
                }
                if (SimulationSettings.NumberOfOralDosesPerDay >= 3) {
                    parameters["t0_O3_day1"] = 6;
                }
            }
            //day 2
            if (SimulationSettings.NumberOfSimulatedDays >= 2) {
                parameters["t0_O1_day2"] = 24;
                parameters["t0_D1_day2"] = 24;
                parameters["t0_D21_day2"] = 24;
                if (SimulationSettings.NumberOfOralDosesPerDay >= 2) {
                    parameters["t0_O2_day2"] = 36;
                }
                if (SimulationSettings.NumberOfDermalDosesPerDay >= 2) {
                    parameters["t0_D2_day2"] = 36;
                    parameters["t0_D22_day2"] = 36;
                }
                if (SimulationSettings.NumberOfOralDosesPerDay >= 3) {
                    parameters["t0_O3_day2"] = 30;
                }
            }
            //day 3
            if (SimulationSettings.NumberOfSimulatedDays >= 3) {
                parameters["t0_O1_day3"] = 48;
                parameters["t0_D1_day3"] = 48;
                parameters["t0_D21_day3"] = 48;
                if (SimulationSettings.NumberOfOralDosesPerDay >= 2) {
                    parameters["t0_O2_day3"] = 60;
                }
                if (SimulationSettings.NumberOfDermalDosesPerDay >= 2) {
                    parameters["t0_D2_day3"] = 60;
                    parameters["t0_D22_day3"] = 60;
                }
                if (SimulationSettings.NumberOfOralDosesPerDay >= 3) {
                    parameters["t0_O3_day3"] = 54;
                }
            }
            //day 4
            if (SimulationSettings.NumberOfSimulatedDays >= 4) {
                parameters["t0_O1_day4"] = 72;
                parameters["t0_D1_day4"] = 72;
                parameters["t0_D21_day4"] = 72;
                if (SimulationSettings.NumberOfOralDosesPerDay >= 2) {
                    parameters["t0_O2_day4"] = 84;
                }
                if (SimulationSettings.NumberOfDermalDosesPerDay >= 2) {
                    parameters["t0_D2_day4"] = 84;
                    parameters["t0_D22_day4"] = 84;
                }
                if (SimulationSettings.NumberOfOralDosesPerDay >= 3) {
                    parameters["t0_O3_day4"] = 78;
                }
            }
        }
    }
}
