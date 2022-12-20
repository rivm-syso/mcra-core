using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.IntraSpeciesConversion {

    public sealed class IntraSpeciesFactorModelBuilder {

        /// <summary>
        /// Creates a dictionary of intra-species factor models.
        /// </summary>
        /// <param name="effects"></param>
        /// <param name="substances"></param>
        /// <param name="intraSpeciesFactors"></param>
        /// <param name="defaultIntraSpeciesFactor"></param>
        /// <returns></returns>
        public Dictionary<(Effect, Compound), IntraSpeciesFactorModel> Create(
            ICollection<Effect> effects,
            ICollection<Compound> substances,
            ICollection<IntraSpeciesFactor> intraSpeciesFactors,
            double defaultIntraSpeciesFactor
        ) {
            var intraSpeciesConversionModels = new Dictionary<(Effect, Compound), IntraSpeciesFactorModel>();
            if (substances != null && effects != null) {
                foreach (var substance in substances) {
                    foreach (var effect in effects) {

                        // Order find intra-species factor (prefer specific over generic and
                        // prefer effect-specific-substance-generic over substance-specific-effect-generic.
                        var intraSpeciesFactor = intraSpeciesFactors?
                            .Where(c => (c.Compound == substance || c.Compound == null)
                                && (c.Effect == effect || c.Effect == null))
                            .OrderBy(r => (r.Compound == null ? 2 : 1) + (r.Effect == null ? 2 : 0))
                            .FirstOrDefault();
                        (var factor, var gsd, var df) = calculateParameters(
                            intraSpeciesFactor?.LowerVariationFactor,
                            intraSpeciesFactor?.UpperVariationFactor,
                            defaultIntraSpeciesFactor
                        );
                        var model = new IntraSpeciesFactorModel() {
                            Effect = effect,
                            Substance = substance,
                            Factor = factor,
                            DegreesOfFreedom = df,
                            GeometricStandardDeviation = gsd,
                            IntraSpeciesFactor = intraSpeciesFactor
                        };
                        intraSpeciesConversionModels.Add((effect, substance), model);
                    }
                }
            }

            // Add null model / default model
            intraSpeciesConversionModels.Add((null, null), new IntraSpeciesFactorModel() {
                Factor = defaultIntraSpeciesFactor,
            });

            return intraSpeciesConversionModels;
        }

        /// <summary>
        /// Creates a resampled collection of intra-species factor models based
        /// on the nominal collection of intra-species factor models.
        /// </summary>
        /// <param name="intraSpeciesFactorModels"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        public Dictionary<(Effect, Compound), IntraSpeciesFactorModel> Resample(
            IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesFactorModels,
            IRandom generator
        ) {
            var result = new Dictionary<(Effect, Compound), IntraSpeciesFactorModel>();
            var draw = generator.NextDouble();
            foreach (var intraSpeciesFactorModel in intraSpeciesFactorModels) {
                var model = intraSpeciesFactorModel.Value;
                var uncertaintyFactor = !double.IsNaN(model.GeometricStandardDeviation)
                    ? Math.Sqrt(model.DegreesOfFreedom / ChiSquaredDistribution.InvCDF(model.DegreesOfFreedom, draw))
                    : double.NaN;
                var record = new IntraSpeciesFactorModel() {
                    Effect = model.Effect,
                    Substance = model.Substance,
                    DegreesOfFreedom = model.DegreesOfFreedom,
                    GeometricStandardDeviation = uncertaintyFactor * model.GeometricStandardDeviation,
                    Factor = model.Factor,
                    IntraSpeciesFactor = model.IntraSpeciesFactor
                };
                result.Add(intraSpeciesFactorModel.Key, record);
            }
            return result;
        }

        /// <summary>
        /// Computes the intra-species factor/geometric mean, geometric standard deviation
        /// and degrees of freedom from specified lower, upper, and default factor. If lower
        /// and/or upper are null or NaN, then the default factor is used.
        /// </summary>
        /// <param name="intraLower"></param>
        /// <param name="intraUpper"></param>
        /// <param name="defaultFactor"></param>
        /// <returns></returns>
        private (double, double, double) calculateParameters(double? intraLower, double? intraUpper, double defaultFactor) {
            if (intraLower != null && !double.IsNaN((double)intraLower)
                && intraUpper != null && !double.IsNaN((double)intraUpper)) {
                return calculateParameters((double)intraLower, (double)intraUpper);
            } else {
                return (defaultFactor, double.NaN, double.NaN);
            }
        }

        /// <summary>
        /// Computes the intra-species factor/geometric mean, geometric standard deviation
        /// and degrees of freedom from specified lower and upper. If lower and upper are
        /// assumed NOT to be NAN.
        /// </summary>
        /// <param name="intraLower"></param>
        /// <param name="intraUpper"></param>
        /// <returns></returns>
        private (double, double, double) calculateParameters(double intraLower, double intraUpper) {
            var dfMin = 1D;
            var dfMax = 1000D;
            var degreesOfFreedom = dfMax;
            var criterium = Math.Pow(Math.Log(intraUpper) / Math.Log(intraLower), 2);
            for (int i = 1; i < 1000; i++) {
                degreesOfFreedom = (dfMin + dfMax) / 2;
                var ratio = ChiSquaredDistribution.InvCDF(degreesOfFreedom, 0.975) / ChiSquaredDistribution.InvCDF(degreesOfFreedom, 0.025);
                if ((dfMax - dfMin) / degreesOfFreedom < 1e-6) {
                    break;
                }
                if (ratio < criterium) {
                    dfMax = degreesOfFreedom;
                } else {
                    dfMin = degreesOfFreedom;
                }
            }
            var geometricStandardDeviation = Math.Pow(intraUpper, Math.Sqrt(ChiSquaredDistribution.InvCDF(degreesOfFreedom, 0.025) / degreesOfFreedom) / 1.645);
            return (1D, geometricStandardDeviation, degreesOfFreedom);
        }
    }
}
