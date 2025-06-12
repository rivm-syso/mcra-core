using DocumentFormat.OpenXml.Validation;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock populations
    /// </summary>
    public static class FakePopulationsGenerator {

        private static string[] _defaultPopulations = [
            "NL", "DE", "BE"
        ];

        /// <summary>
        /// Creates a list of populations
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static List<Population> Create(
            int n,
            List<PopulationCharacteristicType> populationCharacteristicTypes = null,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            if (n <= _defaultPopulations.Length) {
                var result = _defaultPopulations
                    .Take(n)
                    .Select(r => {
                        var record = new Population() {
                            Code = r,
                            Name = r,
                        };
                        if (populationCharacteristicTypes?.Count() > 0) {
                            record.PopulationCharacteristics = populationCharacteristicTypes
                                .Select(r => FakePopulationCharacteristic(r, random))
                                .ToList();
                        }
                        return record;
                    })
                    .ToList();
                return result;
            }
            throw new Exception($"Cannot create more than {_defaultPopulations.Length} mock populations using this method!");
        }

        public static PopulationCharacteristic FakePopulationCharacteristic(
            PopulationCharacteristicType characteristicType,
            IRandom random
        ) {
            var result = new PopulationCharacteristic() {
                Characteristic = characteristicType
            };
            switch (result.Characteristic) {
                case PopulationCharacteristicType.IQ:
                    result.DistributionType = PopulationCharacteristicDistributionType.Normal;
                    result.Value = 100;
                    result.CvVariability = 15;
                    break;
                case PopulationCharacteristicType.BirthWeight:
                    result.DistributionType = PopulationCharacteristicDistributionType.LogNormal;
                    result.Value = random.NextDouble(2500, 3500);
                    result.CvVariability = random.NextDouble(.5, 2);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }
    }
}
