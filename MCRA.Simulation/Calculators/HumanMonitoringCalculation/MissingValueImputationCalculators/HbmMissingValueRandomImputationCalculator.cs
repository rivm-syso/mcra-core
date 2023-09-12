using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.RandomGenerators;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public class HbmMissingValueRandomImputationCalculator : IHbmMissingValueImputationCalculator {

        /// <summary>
        /// Missing value imputation by random sampling.
        /// </summary>
        /// <param name="hbmSampleSubstanceCollections"></param>
        /// <param name="missingValueCutOff"></param>
        /// <param name="randomSeed"></param>
        /// <returns></returns>
        public List<HumanMonitoringSampleSubstanceCollection> ImputeMissingValues(
           ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
           double missingValueCutOff,
           int randomSeed
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var hbmSampleSubstances = sampleCollection.HumanMonitoringSampleSubstanceRecords
                    .SelectMany(r => r.HumanMonitoringSampleSubstances)
                    .ToLookup(c => c.Key);

                var substances = hbmSampleSubstances.Select(c => c.Key).ToList();

                // Create random generator for this sampling method and each substance
                var randomGenerators = substances.ToDictionary(
                    s => s,
                    s => new McraRandomGenerator(
                        RandomUtils.CreateSeed(randomSeed, sampleCollection.SamplingMethod.GetHashCode(), s.GetHashCode())
                    ) as IRandom
                );

                // Get imputation values per substance
                var imputationValues = new Dictionary<Compound, List<double>>();
                foreach (var substance in substances) {
                    var residues = hbmSampleSubstances[substance]
                        .Where(c => !double.IsNaN(c.Value.Residue))
                        .Select(c => c.Value.Residue)
                        .ToList();
                    imputationValues[substance] = residues;
                }

                // Create new (imputed) sample substance records
                var totalNumberOfSamples = sampleCollection.HumanMonitoringSampleSubstanceRecords.Count;
                var newSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                    .OrderBy(s => s.Individual.Code)
                    .ThenBy(s => s.HumanMonitoringSample.Code)
                    .Select(sample => {
                        var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                            .Select(r => getSampleSubstance(
                                r,
                                imputationValues[r.MeasuredSubstance],
                                totalNumberOfSamples * missingValueCutOff / 100d,
                                randomGenerators))
                            .ToDictionary(c => c.MeasuredSubstance);
                        return new HumanMonitoringSampleSubstanceRecord() {
                            HumanMonitoringSampleSubstances = sampleCompounds,
                            HumanMonitoringSample = sample.HumanMonitoringSample
                        };
                    })
                    .ToList();

                result.Add(new HumanMonitoringSampleSubstanceCollection(
                    sampleCollection.SamplingMethod,
                    newSampleSubstanceRecords,
                    sampleCollection.ConcentrationUnit,
                    sampleCollection.ExpressionType,
                    sampleCollection.TriglycConcentrationUnit,
                    sampleCollection.CholestConcentrationUnit,
                    sampleCollection.LipidConcentrationUnit,
                    sampleCollection.CreatConcentrationUnit
                ));
            }

            return result;
        }

        private SampleCompound getSampleSubstance(
            SampleCompound sampleSubstance,
            List<double> imputationValues,
            double minimumNumberOfImputationValues,
            Dictionary<Compound, IRandom> randomGenerators
        ) {
            if (!imputationValues.Any() || imputationValues.Count< minimumNumberOfImputationValues) {
                return sampleSubstance;
            }
            var ix = randomGenerators[sampleSubstance.ActiveSubstance].Next(imputationValues.Count);
            var clone = sampleSubstance.Clone();
            if (sampleSubstance.IsMissingValue) {
                clone.Residue = imputationValues[ix];
                clone.ResType = ResType.VAL;
            }
            return clone;
        }
    }
}
