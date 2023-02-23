using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.DoseResponseDataCalculation {
    public class DoseResponseDataMerger {

        public DoseResponseExperiment Merge(ICollection<DoseResponseExperiment> experiments, Response response) {
            var allSubstances = experiments.SelectMany(r => r.Substances).Distinct().ToList();
            var allCovariates = experiments.SelectMany(r => r.Covariates).Distinct().ToList();
            var allTimes = experiments.Select(r => r.Time).Distinct().ToList();

            var doseRoutes = experiments.Select(r => r.DoseRoute).Distinct().ToList();
            if (doseRoutes.Count > 1) {
                throw new Exception("Experiments have different dose routes");
            }

            var doseUnits = experiments.Select(r => r.DoseUnitString).Distinct().ToList();
            if (doseUnits.Count > 1) {
                throw new Exception("Experiments have different dose units");
            }

            var times = experiments.Select(r => r.Time).Distinct().ToList();
            if (times.Count > 1) {
                throw new Exception("Experiments have different dose units");
            }

            var designs = experiments.Select(r => r.Design).ToList();
            var design = designs[0].Select(r => r).ToList();
            if (!designs.All(r => design.SequenceEqual(r))) {
                throw new Exception("Cannot merge experiments with different experimental designs");
            }
            design.Insert(0, "SubExperiment");

            var experimentalUnits = experiments
                .SelectMany(r => r.ExperimentalUnits.Where(eu => eu.Responses.ContainsKey(response)).ToList(), (r, u) => {
                    var unit = new ExperimentalUnit() {
                        Code = u.Code,
                        Covariates = u.Covariates.ToDictionary(c => c.Key, c => c.Value),
                        DesignFactors = u.DesignFactors.ToDictionary(df => df.Key, df => df.Value),
                        Doses = u.Doses,
                        Responses = u.Responses.Where(ur => ur.Key == response).ToDictionary(urr => urr.Key, ur => ur.Value),
                        Times = u.Times
                    };
                    unit.DesignFactors.Add("SubExperiment", r.Code);
                    return unit;
                })
                .ToList();

            var result = new DoseResponseExperiment() {
                Code = $"Merged-{response.Code}",
                Name = $"Merged-{response.Code}",
                Description = $"Merged dose response data of experiments of response {response.Code}: {string.Join(", ", experiments.Select(r => r.Code))}.",
                Responses = new List<Response>() { response },
                Substances = allSubstances,
                Covariates = allCovariates,
                Design = design,
                DoseRoute = doseRoutes.First(),
                DoseUnitString = doseUnits.First(),
                ExperimentalUnits = experimentalUnits,
            };

            return result;
        }
    }
}
