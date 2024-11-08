using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.CompoundResidueCollectionCalculation;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmConcentrationModelCalculation {

    /// <summary>
    /// Builder class for HBM concentration models.
    /// </summary>
    public class HbmConcentrationModelBuilder {

        /// <summary>
        /// Creates concentration models for all sampling methodes and substances
        /// in the HBM sample substance collections.
        /// </summary>
        /// <param name="hbmSampleSubstanceCollections"></param>
        /// <param name="nonDetectsHandlingMethod"></param>
        /// <param name="lorReplacementFactor"></param>
        /// <returns></returns>
        public IDictionary<(HumanMonitoringSamplingMethod, Compound), ConcentrationModel> Create(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) {
            var concentrationModels = new Dictionary<(HumanMonitoringSamplingMethod, Compound), ConcentrationModel>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var hbmSampleSubstances = sampleCollection.HumanMonitoringSampleSubstanceRecords
                    .SelectMany(r => r.HumanMonitoringSampleSubstances)
                    .ToLookup(c => c.Key);
                var substances = hbmSampleSubstances.Select(c => c.Key).ToList();
                foreach (var substance in substances) {
                    var concentrationModel = createConcentrationModel(
                        hbmSampleSubstances[substance],
                        nonDetectsHandlingMethod,
                        lorReplacementFactor
                    );
                    concentrationModels[(sampleCollection.SamplingMethod, substance)] = concentrationModel;
                }
            }
            return concentrationModels;
        }

        /// <summary>
        /// Fit CensoredLognormal model, fallback (currently) is Empirical distribution
        /// </summary>
        /// <param name="hbmSampleSubstances"></param>
        /// <returns></returns>
        private ConcentrationModel createConcentrationModel(
            IEnumerable<KeyValuePair<Compound, SampleCompound>> hbmSampleSubstances,
            NonDetectsHandlingMethod nonDetectsHandlingMethod,
            double lorReplacementFactor
        ) {
            var positiveResidues = hbmSampleSubstances
                .Where(c => c.Value.IsPositiveResidue)
                .Select(c => c.Value.Residue)
                .ToList();

            var censoredValues = hbmSampleSubstances
                .Where(c => c.Value.IsCensoredValue)
                .Select(c => new CensoredValue() {
                    LOD = c.Value.Lod,
                    LOQ = c.Value.Loq,
                    ResType = c.Value.ResType
                })
                .ToList();

            var substanceResidueCollection = new CompoundResidueCollection() {
                Positives = positiveResidues,
                CensoredValuesCollection = censoredValues
            };

            var concentrationModel = new CMCensoredLogNormal() {
                NonDetectsHandlingMethod = nonDetectsHandlingMethod,
                Residues = substanceResidueCollection,
                FractionOfLor = lorReplacementFactor
            };

            if (!concentrationModel.CalculateParameters()) {
                var empiricalModel = new CMEmpirical() {
                    NonDetectsHandlingMethod = nonDetectsHandlingMethod,
                    Residues = substanceResidueCollection,
                    FractionOfLor = lorReplacementFactor
                };
                empiricalModel.CalculateParameters();
                return empiricalModel;
            }
            return concentrationModel;
        }
    }
}
