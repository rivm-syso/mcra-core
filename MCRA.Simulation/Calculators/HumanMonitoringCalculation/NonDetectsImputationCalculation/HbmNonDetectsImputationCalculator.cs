using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.NonDetectsImputationCalculation {

    /// <summary>
    /// Calculator for imputation of non-detects in the HBM sample substance collection.
    /// </summary>
    public sealed class HbmNonDetectsImputationCalculator {

        /// <summary>
        /// Imputation method (e.g., via fixed value or stochastic model).
        /// </summary>
        public NonDetectImputationMethod NonDetectImputationMethod { get; private set; }

        /// <summary>
        /// Handling/replacement method (in case of fixed values).
        /// </summary>
        public NonDetectsHandlingMethod NonDetectsHandlingMethod { get; private set; }

        /// <summary>
        /// Censored value replacement factor for replacement by fixed value (factor x LOQ/LOD).
        /// </summary>
        public double LorReplacementFactor { get; private set; }

        public HbmNonDetectsImputationCalculator(
            NonDetectImputationMethod nonDetectImputationMethod,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) {
            NonDetectsHandlingMethod = nonDetectsHandlingMethod;
            NonDetectImputationMethod = nonDetectImputationMethod;
            LorReplacementFactor = lorReplacementFactor;
        }

        /// <summary>
        /// Replace censored values (below LOQ, LOD) for a draw from censored lognormal model (note only from censored/ lower tail of distribution)
        /// </summary>
        public List<HumanMonitoringSampleSubstanceCollection> ImputeNonDetects(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            IDictionary<(HumanMonitoringSamplingMethod, Compound), ConcentrationModel> concentrationModels,
            int randomSeed
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleSubstanceCollection in hbmSampleSubstanceCollections) {

                // Get the substances
                var substances = sampleSubstanceCollection.HumanMonitoringSampleSubstanceRecords
                    .SelectMany(r => r.HumanMonitoringSampleSubstances.Keys)
                    .Distinct()
                    .ToList();

                // Create random generator for this sampling method and each substance
                var randomGenerators = substances.ToDictionary(
                    s => s,
                    s => new McraRandomGenerator(
                        RandomUtils.CreateSeed(randomSeed, sampleSubstanceCollection.SamplingMethod.GetHashCode(), s.GetHashCode())
                    ) as IRandom
                );

                // Create new sample substance records with imputed non-detects
                var newSampleSubstanceRecords = sampleSubstanceCollection.HumanMonitoringSampleSubstanceRecords
                    .OrderBy(s => s.Individual.Code)
                    .Select(sampleSubstanceRecord => {
                        var sampleCompounds = sampleSubstanceRecord.HumanMonitoringSampleSubstances.Values
                            .Select(r => getSampleSubstance(r, concentrationModels?[(sampleSubstanceRecord.SamplingMethod, r.MeasuredSubstance)] ?? null, randomGenerators))
                            .ToDictionary(c => c.MeasuredSubstance);
                        return new HumanMonitoringSampleSubstanceRecord() {
                            HumanMonitoringSampleSubstances = sampleCompounds,
                            HumanMonitoringSample = sampleSubstanceRecord.HumanMonitoringSample
                        };
                    })
                    .ToList();
                result.Add(new HumanMonitoringSampleSubstanceCollection(
                    sampleSubstanceCollection.SamplingMethod,
                    newSampleSubstanceRecords,
                    sampleSubstanceCollection.Unit,
                    sampleSubstanceCollection.ExpressionType,
                    sampleSubstanceCollection.TriglycConcentrationUnit,
                    sampleSubstanceCollection.CholestConcentrationUnit,
                    sampleSubstanceCollection.LipidConcentrationUnit,
                    sampleSubstanceCollection.CreatConcentrationUnit
                )
            );
            }
            return result;
        }

        /// <summary>
        /// Draw residue for censored values or from fallback model
        /// </summary>
        /// <param name="sampleSubstance"></param>
        /// <param name="concentrationModel"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private SampleCompound getSampleSubstance(
            SampleCompound sampleSubstance,
            ConcentrationModel concentrationModel,
            Dictionary<Compound, IRandom> randomGenerators
        ) {
            var clone = sampleSubstance.Clone();
            if (sampleSubstance.IsCensoredValue) {
                if (NonDetectImputationMethod != NonDetectImputationMethod.ReplaceByLimit
                    && concentrationModel != null
                ) {
                    clone.Residue = concentrationModel.GetImputedCensoredValue(clone, randomGenerators[sampleSubstance.ActiveSubstance]);
                } else {
                    clone.Residue = ConcentrationModel.GetDeterministicImputationValue(
                        sampleSubstance,
                        NonDetectsHandlingMethod,
                        LorReplacementFactor
                    );
                }
                clone.ResType = ResType.VAL;
            }
            return clone;
        }
    }
}
