using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock dose response models
    /// </summary>
    public static class MockDoseResponseModelGenerator {

        /// <summary>
        /// Creates a list of dose response models
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="responses"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<DoseResponseModel> Create(
            ICollection<Compound> substances,
            ICollection<Response> responses,
            IRandom random
        ) {
            var doseResponseModels = new List<DoseResponseModel>();

            foreach (var response in responses) {
                var i = 0;
                var doseResponseModelBenchmarkDoses = substances
                    .Select((r, ix) => new DoseResponseModelBenchmarkDose() {
                        Substance = r,
                        BenchmarkDose = 1D / (ix + 1),
                        BenchmarkDoseLower = 1D / (ix + 1) * .9,
                        BenchmarkDoseUpper = 1D / (ix + 1) * 1.1,
                        Rpf = (ix + 1),
                        RpfLower = (ix + 1) * .9,
                        RpfUpper = (ix + 1) * 1.1,
                        IdDoseResponseModel = $"DRM-{response.Code}",
                        CovariateLevel = string.Empty,
                        ModelParameterValues = "a=1,b=2,c=3,d=4",
                    })
                    .ToDictionary(r => $"{i}" + r.Key);
                var model = new DoseResponseModel() {
                    BenchmarkResponseTypeString = BenchmarkResponseType.Percentage.ToString(),
                    Description = "description",
                    DoseResponseModelType = DoseResponseModelType.Expm1,
                    CriticalEffectSize = 0.05,
                    IdDoseResponseModel = $"DRM-{response.Code}",
                    Name = $"DRM-{response.Code}",
                    Substances = substances.ToList(),
                    Response = response,
                    LogLikelihood = -129898.99,
                    ModelEquation = "xxx",
                    ProastVersion = "x.xx",
                    DoseResponseModelBenchmarkDoses = doseResponseModelBenchmarkDoses,
                    IdExperiment = $"Experiment-{response.Code}",
                    Covariates = null,
                };
                doseResponseModels.Add(model);
            };

            return doseResponseModels;
        }

        /// <summary>
        /// Creates a single dose response model for the specified response and substances.
        /// If specified, then the provided benchmarkDoses are used. Otherwise these are drawn
        /// randomly.
        /// </summary>
        /// <param name="response"></param>
        /// <param name="substances"></param>
        /// <param name="benchmarkDoses"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static DoseResponseModel Create(
            Response response,
            ICollection<Compound> substances,
            IRandom random,
            double[] benchmarkDoses = null
        ) {
            var seed = random.Next();
            var idDoseResponseModel = $"DRM-{response.Code}-{seed}";
            var doses = benchmarkDoses ?? substances.Select(r => 100 * random.NextDouble()).ToArray();
            var doseResponseModelBenchmarkDoses = substances
                .Select((r, ix) => new DoseResponseModelBenchmarkDose() {
                    Substance = r,
                    IdDoseResponseModel = idDoseResponseModel,
                    BenchmarkDose = doses[ix],
                    BenchmarkDoseLower = doses[ix] * .9,
                    BenchmarkDoseUpper = doses[ix] * 1.1,
                    Rpf = (doses[0] / doses[ix]),
                    RpfLower = ((doses[0] * .9) / (doses[ix] * .9)),
                    RpfUpper = ((doses[0] * 1.1) / (doses[ix] * 1.1)),
                })
                .ToDictionary(r => r.Key);

            var result = new DoseResponseModel() {
                IdDoseResponseModel = idDoseResponseModel,
                Name = idDoseResponseModel,
                Description = $"Artificial dose response model for response {response.Code} generated with seed {seed}",
                Substances = substances.ToList(),
                Response = response,
                LogLikelihood = random.NextDouble() * 1000,
                DoseResponseModelBenchmarkDoses = doseResponseModelBenchmarkDoses,
                IdExperiment = $"Experiment-{response.Code}-{seed}",
                Covariates = null,
            };

            return result;
        }
    }
}
