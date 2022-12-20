using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public class HbmMissingValueRandomImputationCalculator : IHbmMissingValueImputationCalculator {

        public List<HumanMonitoringSampleSubstanceCollection> ImputeMissingValues(
           ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
           IRandom random
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();

            foreach (var sampleCollection in hbmSampleSubstanceCollections) {

                var hbmSampleSubstances = sampleCollection.HumanMonitoringSampleSubstanceRecords
                    .SelectMany(r => r.HumanMonitoringSampleSubstances)
                    .ToLookup(c => c.Key);

                var substances = hbmSampleSubstances.Select(c => c.Key).ToList();
                var imputationValues = new Dictionary<Compound, List<double>>();
                foreach (var substance in substances) {
                    var residues = hbmSampleSubstances[substance]
                        .Where(c => !double.IsNaN(c.Value.Residue))
                        .Select(c => c.Value.Residue)
                        .ToList();
                    imputationValues[substance] = residues;
                }

                var newSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                    .Select(sample => {
                        var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                            .Select(r => getSampleSubstance(r, imputationValues[r.MeasuredSubstance], random))
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

            return result;
        }

        private SampleCompound getSampleSubstance(
            SampleCompound sampleSubstance,
            List<double> imputationValues,
            IRandom random
        ) {
            if (!imputationValues.Any()) {
                return sampleSubstance; 
            }
            var ix =  random.Next(imputationValues.Count);
            var clone = sampleSubstance.Clone();
            if (sampleSubstance.IsMissingValue) {
                clone.Residue = imputationValues[ix];
                clone.ResType = ResType.VAL;
            }
            return clone;
        }
    }
}
