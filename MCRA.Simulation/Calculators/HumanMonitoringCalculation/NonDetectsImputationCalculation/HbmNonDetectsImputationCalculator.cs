using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.NonDetectsImputationCalculation {
    public sealed class HbmNonDetectsImputationCalculator {

        public NonDetectImputationMethod NonDetectImputationMethod { get; private set; }
        public double LorReplacementFactor { get; private set; }
        public NonDetectsHandlingMethod NonDetectsHandlingMethod { get; private set; }

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
        /// <param name="hbmSampleSubstanceCollections"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public List<HumanMonitoringSampleSubstanceCollection> ImputeNonDetects(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            IDictionary<(HumanMonitoringSamplingMethod, Compound), ConcentrationModel> concentrationModels,
            IRandom random
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var newSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                    .Select(sample => {
                        var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                            .Select(r => getSampleSubstance(r, concentrationModels?[(sample.SamplingMethod, r.MeasuredSubstance)] ?? null, random))
                            .ToDictionary(c => c.MeasuredSubstance, c => c);
                        return new HumanMonitoringSampleSubstanceRecord() {
                            HumanMonitoringSampleSubstances = sampleCompounds,
                            HumanMonitoringSample = sample.HumanMonitoringSample
                        };
                    })
                    .ToList();
                    result.Add(new HumanMonitoringSampleSubstanceCollection(
                        sampleCollection.SamplingMethod,
                        newSampleSubstanceRecords,
                        sampleCollection.TriglycConcentrationUnit,
                        sampleCollection.CholestConcentrationUnit,
                        sampleCollection.LipidConcentrationUnit,
                        sampleCollection.CreatConcentrationUnit
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
            IRandom random
        ) {
            var clone = sampleSubstance.Clone();
            if (sampleSubstance.IsCensoredValue) {
                if (NonDetectImputationMethod != NonDetectImputationMethod.ReplaceByLimit
                    && concentrationModel != null
                ) {
                    clone.Residue = concentrationModel.GetImputedCensoredValue(clone, random);
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
