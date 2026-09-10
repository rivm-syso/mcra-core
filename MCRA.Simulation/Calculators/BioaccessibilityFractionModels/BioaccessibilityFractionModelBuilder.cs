using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.BioaccessibilityFractionModels {
    public sealed class BioaccessibilityFractionModelBuilder {

        public static Dictionary<Compound, IBioaccessibilityFractionModel> Create(
            ICollection<Compound> substances,
            ICollection<DustBioaccessibilityFraction> bioaccessibilityFractions
        ) {
            var models = new Dictionary<Compound, IBioaccessibilityFractionModel>();
            foreach (var substance in substances) {
                var candidates = bioaccessibilityFractions
                    .Where(r => r.Substance == substance);
                if (candidates.Count() == 0) {
                    models.Add(substance, new BioaccessibilityFractionConstantModel(substance, 1));
                } else if (candidates.Count() == 1) {
                    var candidate = candidates.Single();
                    var model = Create(candidate);
                    models.Add(substance, model);
                } else {
                    var msg = $"Found multiple dust bioaccessibility fraction records for substance [{substance.Name} ({substance.Code})]";
                    throw new Exception(msg);
                }
            }
            return models;
        }

        public static Dictionary<Compound, IBioaccessibilityFractionModel> Create(
            ICollection<Compound> substances,
            ICollection<SoilBioaccessibilityFraction> bioaccessibilityFractions
        ) {
            var models = new Dictionary<Compound, IBioaccessibilityFractionModel>();
            foreach (var substance in substances) {
                var candidates = bioaccessibilityFractions
                    .Where(r => r.Substance == substance);
                if (candidates.Count() == 0) {
                    models.Add(substance, new BioaccessibilityFractionConstantModel(substance, 1));
                } else if (candidates.Count() == 1) {
                    var model = Create(candidates.Single());
                    models.Add(substance, model);
                } else {
                    var msg = $"Found multiple soil bioaccessibility fraction records for substance [{substance.Name} ({substance.Code})]";
                    throw new Exception(msg);
                }
            }
            return models;
        }

        public static IBioaccessibilityFractionModel Create(
            DustBioaccessibilityFraction dustBioaccessibilityFraction
        ) {
            var distributionType = dustBioaccessibilityFraction.DistributionType;
            if (distributionType == BioaccessibilityFractionDistributionType.Constant) {
                return new BioaccessibilityFractionConstantModel(
                    dustBioaccessibilityFraction.Substance,
                    dustBioaccessibilityFraction.Value
                );
            } else {
                var mean = dustBioaccessibilityFraction.Value;
                var cv = dustBioaccessibilityFraction.CvVariability.GetValueOrDefault();
                if (dustBioaccessibilityFraction.CvVariability == null) {
                    var msg = $"CvVariability cannot be missing for distribution {distributionType} for dust bioaccessibility fraction.";
                    throw new Exception(msg);
                }
                var distribution = CreateDistribution(distributionType, mean, cv);
                return new BioaccessibilityFractionDistributionModel(
                    dustBioaccessibilityFraction.Substance,
                    distribution
                );
            }
        }

        public static IBioaccessibilityFractionModel Create(
            SoilBioaccessibilityFraction soilBioaccessibilityFraction
        ) {
            var distributionType = soilBioaccessibilityFraction.DistributionType;
            if (distributionType == BioaccessibilityFractionDistributionType.Constant) {
                return new BioaccessibilityFractionConstantModel(
                    soilBioaccessibilityFraction.Substance,
                    soilBioaccessibilityFraction.Value
                );
            } else {
                var mean = soilBioaccessibilityFraction.Value;
                var cv = soilBioaccessibilityFraction.CvVariability.GetValueOrDefault();
                if (soilBioaccessibilityFraction.CvVariability == null) {
                    var msg = $"CvVariability cannot be missing for distribution {distributionType} for soil bioaccessibility fraction.";
                    throw new Exception(msg);
                }
                var distribution = CreateDistribution(distributionType, mean, cv);
                return new BioaccessibilityFractionDistributionModel(
                    soilBioaccessibilityFraction.Substance,
                    distribution
                );
            }
        }

        private static Distribution CreateDistribution(
            BioaccessibilityFractionDistributionType distributionType,
            double mean,
            double cv
        ) {
            return distributionType switch {
                BioaccessibilityFractionDistributionType.LogNormal => LogNormalDistribution.FromMeanAndCv(mean, cv),
                BioaccessibilityFractionDistributionType.Uniform => UniformDistribution.FromMeanAndCv(mean, cv),
                BioaccessibilityFractionDistributionType.Constant => null,
                _ => throw new Exception($"Undefined distribution {distributionType} for bioaccessibility fraction.")
            };
        }
    }
}
