using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.ExposureEvent;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation {
    public class ExposureEventsGenerator {

        public PbkSimulationSettings SimulationSettings { get; private set; }

        private readonly int _timeUnitMultiplier;
        private readonly ExposureUnitTriple _exposureUnit;
        private readonly Dictionary<ExposureRoute, DoseUnit> _routeDoseUnits;
        private bool _perPerson => !SimulationSettings.BodyWeightCorrected;

        public ExposureEventsGenerator(
            PbkSimulationSettings simulationSetings,
            TimeUnit timeScale,
            ExposureUnitTriple exposureUnit,
            Dictionary<ExposureRoute, DoseUnit> routeDoseUnits
        ) {
            SimulationSettings = simulationSetings;
            _exposureUnit = exposureUnit;
            _timeUnitMultiplier = (int)TimeUnit.Days.GetTimeUnitMultiplier(timeScale);
            _routeDoseUnits = routeDoseUnits;
        }

        public List<IExposureEvent> CreateExposureEvents(
            List<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<ExposureRoute> routes,
            Compound substance,
            IRandom generator
        ) {
            if (SimulationSettings.UseRepeatedDailyEvents) {
                var result = createRepeatedAverageExposureEvent(
                    externalIndividualDayExposures,
                    routes,
                    substance,
                    _timeUnitMultiplier
                );
                return result;
            } else {
                var result = createExposureRandomDailyEvents(
                    externalIndividualDayExposures,
                    routes,
                    substance,
                    generator
                );
                return result;
            }
        }

        private List<IExposureEvent> createRepeatedAverageExposureEvent(
            List<IExternalIndividualDayExposure> externalIndividualDayExposures,
            ICollection<ExposureRoute> routes,
            Compound substance,
            int timeUnitMultiplier
        ) {
            var exposureEvents = new List<IExposureEvent>();
            foreach (var route in routes) {

                // Get daily doses
                var dailyDoses = externalIndividualDayExposures
                    .Select(r => r.GetExposure(route, substance, _perPerson))
                    .ToList();

                // Compute average daily dose
                var averageDailyDose = dailyDoses.Sum() / dailyDoses.Count;

                // Get alignment factor for aligning the substance amount unit of the
                // exposure with the substance amount unit of the PBK model
                var doseUnit = _routeDoseUnits[route];
                var substanceAmountAlignmentFactor = doseUnit
                    .GetSubstanceAmountUnit()
                    .GetMultiplicationFactor(
                        _exposureUnit.SubstanceAmountUnit,
                        substance.MolecularMass
                    );

                // Timescale and exposure unit should be aligned with unit of the model
                var routeExposureEvent = new RepeatingExposureEvent() {
                    Route = route,
                    TimeStart = 0,
                    Value = averageDailyDose / substanceAmountAlignmentFactor,
                    Interval = timeUnitMultiplier
                };
                exposureEvents.Add(routeExposureEvent);
            }

            return exposureEvents;
        }

        private List<IExposureEvent> createExposureRandomDailyEvents(
            List<IExternalIndividualDayExposure> externalIndividualExposures,
            ICollection<ExposureRoute> routes,
            Compound substance,
            IRandom generator
        ) {
            var exposureEvents = new List<IExposureEvent>();
            foreach (var route in routes) {
                var doseUnit = _routeDoseUnits[route];

                // Get alignment factor for aligning the substance amount unit of the
                // exposure with the substance amount unit of the PBK model
                var substanceAmountAlignmentFactor = doseUnit
                    .GetSubstanceAmountUnit()
                    .GetMultiplicationFactor(
                        _exposureUnit.SubstanceAmountUnit,
                        substance.MolecularMass
                    );

                // Get daily doses
                var dailyDoses = externalIndividualExposures
                    .Select(e => e.GetExposure(route, substance, _perPerson) / substanceAmountAlignmentFactor)
                    .ToList();

                // Get the timings of the exposure events per day
                var eventsPerDay = getRouteExposureEventTimings(route);

                // Create exposure events
                for (int i = 0; i < SimulationSettings.NumberOfSimulatedDays; i++) {
                    var ix = generator.Next(0, dailyDoses.Count);
                    var dose = dailyDoses[ix] / eventsPerDay.Length;
                    if (dose > 0) {
                        for (int j = 0; j < eventsPerDay.Length; j++) {
                            var record = new SingleExposureEvent() {
                                Route = route,
                                Time = i * _timeUnitMultiplier + j,
                                Value = dose
                            };
                            exposureEvents.Add(record);
                        }
                    }
                }
            }

            return exposureEvents;
        }

        private int[] getRouteExposureEventTimings(ExposureRoute exposureRoute) {
            if (exposureRoute == ExposureRoute.Oral && SimulationSettings.SpecifyEvents) {
                return SimulationSettings.SelectedEvents;
            } else {
                var numEventsPerDay = SimulationSettings.GetNumberOfEventsPerDay(exposureRoute);
                var interval = (double)_timeUnitMultiplier / numEventsPerDay;
                return Enumerable
                    .Range(0, numEventsPerDay)
                    .Select(r => (int)(r * interval))
                    .ToArray();
            }
        }
    }
}
