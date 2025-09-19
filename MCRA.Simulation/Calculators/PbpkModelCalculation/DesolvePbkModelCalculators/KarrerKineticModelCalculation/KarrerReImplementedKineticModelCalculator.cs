using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.PbpkModelCalculation.ExposureEventsGeneration;

namespace MCRA.Simulation.Calculators.PbpkModelCalculation.DesolvePbkModelCalculators.KarrerKineticModelCalculation {

    public sealed class KarrerReImplementedKineticModelCalculator : DesolvePbkModelCalculator {

        public KarrerReImplementedKineticModelCalculator(
            KineticModelInstance kineticModelInstance,
            PbkSimulationSettings simulationSettings
        ) : base(kineticModelInstance, simulationSettings) {
        }

        protected override(List<double> timings, Dictionary<ExposureRoute, List<double>> dosesPerRoute) computeDoses(
            ICollection<ExposureRoute> modelExposureRoutes,
            List<IExposureEvent> exposureEvents,
            double timeUnitMultiplier
        ) {
            (var timings, var dosesPerRoute) = base.computeDoses(modelExposureRoutes, exposureEvents, timeUnitMultiplier);

            var newTimings = new List<double>();
            var newDosesPerRoute = new Dictionary<ExposureRoute, List<double>>();
            foreach (var route in dosesPerRoute.Keys) {
                newDosesPerRoute[route] = [];
            }
            var newEventsCounter = 0;
            for (var i = 0; i < timings.Count; i++) {
                newTimings.Add(timings[i]);
                foreach (var route in dosesPerRoute.Keys) {
                    newDosesPerRoute[route].Add(dosesPerRoute[route][i]);
                }
                newEventsCounter++;
                if (i + 1 == timings.Count || timings[i+1] != timings[i] + 1) {
                    newTimings.Add(timings[i] + 1);
                    foreach (var route in dosesPerRoute.Keys) {
                        newDosesPerRoute[route].Add(0);
                    }
                    newEventsCounter++;
                }
            }
            return (newTimings, newDosesPerRoute);
        }
    }
}
