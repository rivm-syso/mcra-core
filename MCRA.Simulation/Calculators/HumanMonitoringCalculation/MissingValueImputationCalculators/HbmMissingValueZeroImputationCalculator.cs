using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public class HbmMissingValueZeroImputationCalculator : IHbmMissingValueImputationCalculator {

        /// <summary>
        /// When a substance is not measured in the target biological matrix, they are all missing. If they are imputed with zeros
        /// then a biological matrix conversion is useless, because this substance exists already, but with only zero, therefor check
        /// if there are values.
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
                // Get imputation values per substance
                var imputationValues = new Dictionary<Compound, List<double>>();
                foreach (var substance in substances) {
                    var residues = hbmSampleSubstances[substance]
                        .Where(c => !double.IsNaN(c.Value.Residue))
                        .Select(c => c.Value.Residue)
                        .ToList();
                    imputationValues[substance] = residues;
                }
                var totalNumberOfSamples = sampleCollection.HumanMonitoringSampleSubstanceRecords.Count;
                var hbmSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                    .OrderBy(s => s.Individual.Code)
                    .Select(s => {
                        var sampleCompounds = s.HumanMonitoringSampleSubstances.Values
                            .Select(r => createSampleSubstanceRecord(r, 
                                imputationValues[r.MeasuredSubstance],
                                totalNumberOfSamples * missingValueCutOff / 100d))
                            .ToDictionary(c => c.MeasuredSubstance);
                        return new HumanMonitoringSampleSubstanceRecord() {
                            HumanMonitoringSampleSubstances = sampleCompounds,
                            HumanMonitoringSample = s.HumanMonitoringSample
                        };
                    })
                    .ToList();
                result.Add(
                    new HumanMonitoringSampleSubstanceCollection(
                        sampleCollection.SamplingMethod,
                        hbmSampleSubstanceRecords,
                        sampleCollection.Unit,
                        sampleCollection.ExpressionType,
                        sampleCollection.TriglycConcentrationUnit,
                        sampleCollection.CholestConcentrationUnit,
                        sampleCollection.LipidConcentrationUnit,
                        sampleCollection.CreatConcentrationUnit
                    )
                );
            }
            return result;
        }

        private SampleCompound createSampleSubstanceRecord(
            SampleCompound sampleSubstance,
            List<double> imputationValues,
            double minimumNumberOfImputationValues
        ) {
            if (!imputationValues.Any() || imputationValues.Count < minimumNumberOfImputationValues) {
                return sampleSubstance;
            }
            var clone = sampleSubstance.Clone();
            if (sampleSubstance.IsMissingValue) {
                clone.Residue = 0;
                clone.ResType = ResType.VAL;
            }
            return clone;
        }
    }
}
