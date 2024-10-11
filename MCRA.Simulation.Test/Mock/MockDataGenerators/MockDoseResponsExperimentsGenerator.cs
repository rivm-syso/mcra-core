using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.DoseResponseModels;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock dose response experiments
    /// </summary>
    public class MockDoseResponseExperimentsGenerator {
        /// <summary>
        /// Creates dose response experiments
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="responses"></param>
        /// <param name="mixture"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static List<DoseResponseExperiment> Create(
            ICollection<Compound> substances,
            ICollection<Response> responses,
            bool mixture = false,
            int seed = 1
        ) {
            if (mixture) {
                return new List<DoseResponseExperiment>() {
                    Create($"Mixture-{seed}", substances, responses, true, seed)
                };
            } else {
                var result = new List<DoseResponseExperiment>();
                var rnd = new McraRandomGenerator(seed);
                foreach (var substance in substances) {
                    var idExperiment = $"Experiment-{substance.Code}";
                    var records = Create(idExperiment, new List<Compound>() { substance }, responses, false, rnd.Next());
                    result.Add(records);
                }
                return result;
            }
        }

        /// <summary>
        /// Creates a mock dose response experiment.
        /// </summary>
        /// <param name="idExperiment"></param>
        /// <param name="substances"></param>
        /// <param name="responses"></param>
        /// <param name="addMixtureDoses"></param>
        /// <param name="seed"></param>
        /// <returns></returns>
        public static DoseResponseExperiment Create(
            string idExperiment,
            ICollection<Compound> substances,
            ICollection<Response> responses,
            bool addMixtureDoses,
            int seed = 1
        ) {
            var random = new McraRandomGenerator(seed);
            var design = new List<string>();
            var covariates = new List<string>();
            var experiment = new DoseResponseExperiment() {
                Code = idExperiment,
                Description = $"Artificial dose response experiment {idExperiment}",
                Name = $"Experiment {idExperiment}",
                Substances = substances.ToList(),
                Responses = responses.ToList(),
                Reference = "References",
                DoseRoute = "oral",
                DoseUnit = DoseUnit.mM,
                Date = new DateTime(),
                Design = design,
                Covariates = covariates,
                Time = "Time",
                TimeUnit = "Hour",
                ExperimentalUnits = mockExperimentalUnits(substances, responses, addMixtureDoses, random)
            };
            return experiment;
        }
        /// <summary>
        /// Creates experimental units
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="responses"></param>
        /// <param name="addMixtureDoses"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static List<ExperimentalUnit> mockExperimentalUnits(
            ICollection<Compound> substances,
            ICollection<Response> responses,
            bool addMixtureDoses,
            IRandom random
        ) {
            var result = new List<ExperimentalUnit>();
            var doses = new double[] { 0, 0.05, 0.1, 0.5, 1, 5, 10, 20, 50, 100 };
            var models = responses.ToDictionary(r => r, r => mockDoseResponseModelFunction(r, random));

            var logNormal = new LogNormalDistribution(0, 0.5);
            var rpfs = logNormal.Draws(random, substances.Count);
            var rpfRef = rpfs.Median();
            rpfs = rpfs.Select(r => r / rpfRef).ToList();

            var ix = 0;

            // Single substance doses
            for (int j = 0; j < substances.Count; j++) {
                for (int i = 0; i < doses.Length; i++) {
                    var substance = substances.ElementAt(j);
                    var record = new ExperimentalUnit() {
                        Code = $"Unit-{ix++}",
                        Doses = substances.ToDictionary(r => r, r => substance == r ? doses[i] : 0D),
                        Covariates = new Dictionary<string, string>(),
                        DesignFactors = new Dictionary<string, string>(),
                        Responses = responses
                            .ToDictionary(r => r, r => mockDoseResponseMeasurement(r, models[r], doses[i] / rpfs[j], random)),
                    };
                    result.Add(record);
                }
            }

            // Add mixture doses
            if (addMixtureDoses) {
                for (int i = 0; i < doses.Length; i++) {
                    // TODO: mixtures generation could be improved
                    // add deviation of dose addition and choose dose-combinations
                    // from some sort of design
                    var cumulativeDose = rpfs.Sum(r => r * doses[i]);
                    var record = new ExperimentalUnit() {
                        Code = $"Unit-{ix++}",
                        Doses = substances.ToDictionary(r => r, r => doses[i]),
                        Covariates = new Dictionary<string, string>(),
                        DesignFactors = new Dictionary<string, string>(),
                        Responses = responses
                            .ToDictionary(r => r, r => mockDoseResponseMeasurement(r, models[r], cumulativeDose, random)),
                    };
                    result.Add(record);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates dose response measurements
        /// </summary>
        /// <param name="response"></param>
        /// <param name="model"></param>
        /// <param name="dose"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static DoseResponseExperimentMeasurement mockDoseResponseMeasurement(
            Response response,
            IDoseResponseModelFunction model,
            double dose,
            IRandom random
        ) {
            var result = new DoseResponseExperimentMeasurement() {
                idResponse = response.Code,
            };
            if (response.ResponseType == ResponseType.Quantal) {
                var p = model.Calculate(dose);
                var n = 100;
                result.ResponseN = n;
                var bernoulli = new BernoulliDistribution(p);
                result.ResponseValue = bernoulli.Draws(random, n).Sum();
            } else if (response.ResponseType == ResponseType.ContinuousMultiplicative) {
                var normal = new NormalDistribution(0, 0.01);
                result.ResponseValue = model.Calculate(dose) + normal.Draw(random);
            } else {
                throw new NotImplementedException();
            }
            return result;
        }

        /// <summary>
        /// Creates a dose response model function
        /// </summary>
        /// <param name="response"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static IDoseResponseModelFunction mockDoseResponseModelFunction(
            Response response,
            IRandom random
        ) {
            if (response.ResponseType == ResponseType.Quantal) {
                return mockLogisticModel(random);
            } else if (response.ResponseType == ResponseType.ContinuousMultiplicative) {
                return mockExponentialModel5(random);
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a logistic model
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        private static LogisticModel mockLogisticModel(IRandom random) {
            var a = 0 - 2 * random.NextDouble();
            var b = 0.1 + 0.1 * random.NextDouble();
            return new LogisticModel(a, b, 1);
        }
        /// <summary>
        /// Creates a exponential model
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        private static ExponentialModel5 mockExponentialModel5(IRandom random) {
            var a = 1;
            var b = 0.0005 + random.NextDouble() * 0.005;
            var c = 2;
            var d = 2;
            return new ExponentialModel5(a, b, c, d, 1);
        }
    }
}
