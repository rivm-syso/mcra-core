using MCRA.Utils;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.PopulationGeneration {

    public class AcutePopulationGenerator : PopulationGeneratorBase {

        private bool _isSurveySampling;
        private int _numberOfSimulatedIndividualDays;

        public AcutePopulationGenerator(bool isSurveySampling, int numberOfSimulatedIndividualDays) {
            _isSurveySampling = isSurveySampling;
            _numberOfSimulatedIndividualDays = numberOfSimulatedIndividualDays;
        }

        public override List<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            ICollection<Individual> individuals,
            ICollection<IndividualDay> individualDays,
            IRandom individualsRandomGenerator
        ) {
            var sampledIndividualDays = new List<IndividualDay>();
            if (_isSurveySampling) {
                var nSurvey = BMath.Ceiling(_numberOfSimulatedIndividualDays / (individualDays.Count * 1D));
                for (int i = 0; i < nSurvey; i++) {
                    sampledIndividualDays.AddRange(individualDays);
                }
            } else {
                var totalWeight = individualDays.Sum(indDay => indDay.Individual.SamplingWeight);
                if (individualDays.Any(indDay => indDay.Individual.SamplingWeight != 1)) {
                    // use sampling weight
                    sampledIndividualDays = individualDays.DrawRandom(
                        individualsRandomGenerator,
                        indDay => indDay.Individual.SamplingWeight, _numberOfSimulatedIndividualDays).ToList();
                } else {
                    // draw uniform
                    sampledIndividualDays = individualDays.DrawRandom(individualsRandomGenerator, _numberOfSimulatedIndividualDays).ToList();
                }
            }

            // Construct the selected individual days as value types.
            var simulatedIndividualDays = new List<SimulatedIndividualDay>(sampledIndividualDays.Count);
            for (int i = 0; i < sampledIndividualDays.Count; i++) {
                var individualSamplingWeight = _isSurveySampling ? sampledIndividualDays[i].Individual.SamplingWeight : 1D;
                simulatedIndividualDays.Add(
                    new SimulatedIndividualDay() {
                        Individual = sampledIndividualDays[i].Individual,
                        Day = sampledIndividualDays[i].IdDay,
                        SimulatedIndividualId = i,
                        SimulatedIndividualDayId = i,
                        IndividualSamplingWeight = individualSamplingWeight,
                        IndividualBodyWeight = sampledIndividualDays[i].Individual.BodyWeight,
                    });
            }
            return simulatedIndividualDays;
        }
    }
}
