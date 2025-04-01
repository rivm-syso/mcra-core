using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.PopulationGeneration {

    public class AcutePopulationGenerator : PopulationGeneratorBase {

        private bool _isSurveySampling;
        private int _numberOfSimulatedInstances;

        public AcutePopulationGenerator(bool isSurveySampling, int numberOfSimulatedInstances) {
            _isSurveySampling = isSurveySampling;
            _numberOfSimulatedInstances = numberOfSimulatedInstances;
        }

        public override List<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            ICollection<Individual> individuals,
            ICollection<IndividualDay> individualDays,
            IRandom individualsRandomGenerator
        ) {
            var sampledIndividualDays = new List<IndividualDay>();

            if (_isSurveySampling) {
                var nSurvey = BMath.Ceiling(_numberOfSimulatedInstances / (double)individualDays.Count);
                for (int i = 0; i < nSurvey; i++) {
                    sampledIndividualDays.AddRange(individualDays);
                }
            } else {
                var totalWeight = individualDays.Sum(indDay => indDay.Individual.SamplingWeight);
                if (individualDays.Any(indDay => indDay.Individual.SamplingWeight != 1)) {
                    // use sampling weight
                    sampledIndividualDays = individualDays.DrawRandom(
                        individualsRandomGenerator,
                        indDay => indDay.Individual.SamplingWeight, _numberOfSimulatedInstances).ToList();
                } else {
                    // draw uniform
                    sampledIndividualDays = individualDays.DrawRandom(individualsRandomGenerator, _numberOfSimulatedInstances).ToList();
                }
            }

            // Construct the selected individual days as value types.
            var simulatedIndividualDays = new List<SimulatedIndividualDay>(sampledIndividualDays.Count);
            var dayIndex = 0;
            foreach(var idvDay in sampledIndividualDays) {
                var simulatedIndividual = new SimulatedIndividual(idvDay.Individual, dayIndex);
                if(!_isSurveySampling) {
                    simulatedIndividual.SamplingWeight = 1D;
                }
                simulatedIndividualDays.Add(
                    new(simulatedIndividual) {
                        Day = idvDay.IdDay,
                        SimulatedIndividualDayId = dayIndex
                    }
                );
                dayIndex++;
            }

            return simulatedIndividualDays;
        }
    }
}
