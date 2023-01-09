using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Utils;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CensoredLogNormalImputationCalculation {
    public sealed class HbmNonDetectCensoredLognormalImputationCalculator : IHbmNonDetectImputationConcentrationCalculator {
        private readonly IHbmIndividualDayConcentrationsCalculatorSettings _settings;

        public HbmNonDetectCensoredLognormalImputationCalculator(IHbmIndividualDayConcentrationsCalculatorSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Replace censored values (below LOQ, LOD) for a draw from censored lognormal model (note only from censored/ lower tail of distribution)
        /// </summary>
        /// <param name="hbmSampleSubstanceCollections"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public (List<HumanMonitoringSampleSubstanceCollection>, IDictionary<Compound, ConcentrationModel>) ImputeNonDetects(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            IRandom random
        ) {
            var concentrationModels = calculateConcentrationModels(hbmSampleSubstanceCollections);
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var newSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                    .Select(sample => {
                        var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                            .Select(r => getSampleSubstance(r, concentrationModels[r.MeasuredSubstance], random))
                            .ToDictionary(c => c.MeasuredSubstance, c => c);
                        return new HumanMonitoringSampleSubstanceRecord() {
                            HumanMonitoringSampleSubstances = sampleCompounds,
                            HumanMonitoringSample = sample.HumanMonitoringSample
                        };
                    })
                .ToList();

                result.Add(new HumanMonitoringSampleSubstanceCollection(
                    sampleCollection.SamplingMethod,
                    newSampleSubstanceRecords)
                );
            }
            return (result, concentrationModels);
        }

        private IDictionary<Compound, ConcentrationModel> calculateConcentrationModels(
           ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
       ) {
            var concentrationModels = new Dictionary<Compound, ConcentrationModel>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var hbmSampleSubstances = sampleCollection.HumanMonitoringSampleSubstanceRecords
                    .SelectMany(r => r.HumanMonitoringSampleSubstances)
                    .ToLookup(c => c.Key);
                var substances = hbmSampleSubstances.Select(c => c.Key).ToList();
                foreach (var substance in substances) {
                    var concentrationModel = getConcentrationModel(hbmSampleSubstances[substance]);
                    concentrationModels[substance] = concentrationModel;
                }
            }
            return concentrationModels;
        }


        /// <summary>
        /// Fit CensoredLognormal model, fallback (currently) is Empirical distribution
        /// </summary>
        /// <param name="hbmSampleSubstances"></param>
        /// <returns></returns>
        private ConcentrationModel getConcentrationModel(
            IEnumerable<KeyValuePair<Compound, SampleCompound>> hbmSampleSubstances
        ) {
            var positiveResidues = hbmSampleSubstances
                .Where(c => c.Value.IsPositiveResidue)
                .Select(c => c.Value.Residue)
                .ToList();

            var censoredValues = hbmSampleSubstances
                .Where(c => c.Value.IsCensoredValue)
                .Select(c => new CensoredValueCollection() { LOD = c.Value.Lor, LOQ = c.Value.Lor })
                .ToList();

            var substanceResidueCollection = new CompoundResidueCollection() {
                Positives = positiveResidues,
                CensoredValuesCollection = censoredValues
            };

            var concentrationModel = new CMCensoredLogNormal() {
                NonDetectsHandlingMethod = _settings.NonDetectsHandlingMethod,
                Residues = substanceResidueCollection,
                FractionOfLOR = _settings.LorReplacementFactor
            };

            if (!concentrationModel.CalculateParameters()) {
                var empiricalModel = new CMEmpirical() {
                    NonDetectsHandlingMethod = _settings.NonDetectsHandlingMethod,
                    Residues = substanceResidueCollection,
                    FractionOfLOR = _settings.LorReplacementFactor
                };
                empiricalModel.CalculateParameters();
                return empiricalModel;
            }
            return concentrationModel;

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
                if (concentrationModel.ModelType == ConcentrationModelType.CensoredLogNormal) {
                    //Force to draw from lower tail
                    var censoredModel = (concentrationModel as CMCensoredLogNormal);
                    clone.Residue = UtilityFunctions.ExpBound(NormalDistribution.InvCDF(0, 1, random.NextDouble() * censoredModel.FractionCensored) * censoredModel.Sigma + censoredModel.Mu);
                } else {
                    //Same method as nondetects imputation form empirical
                    if (_settings.NonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByZero) {
                        clone.Residue = 0D;
                    } else if (_settings.NonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLOR) {
                        clone.Residue = _settings.LorReplacementFactor * sampleSubstance.Lor;
                    } else if (_settings.NonDetectsHandlingMethod == NonDetectsHandlingMethod.ReplaceByLODLOQSystem) {
                        if (sampleSubstance.IsNonDetect) {
                            clone.Residue = _settings.LorReplacementFactor * sampleSubstance.Lod;
                        } else {
                            clone.Residue = sampleSubstance.Lod + _settings.LorReplacementFactor * (sampleSubstance.Loq - sampleSubstance.Lod);
                        }
                    }
                }
                clone.ResType = ResType.VAL;
            }
            return clone;
        }
    }
}
