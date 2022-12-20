using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public class HbmMissingValueZeroImputationCalculator : IHbmMissingValueImputationCalculator {

        public List<HumanMonitoringSampleSubstanceCollection> ImputeMissingValues(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            IRandom random
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var hbmSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                    .Select(s => {
                        var sampleCompounds = s.HumanMonitoringSampleSubstances.Values
                            .Select(r => createSampleSubstanceRecord(r))
                            .ToDictionary(c => c.MeasuredSubstance, c => c);
                        return new HumanMonitoringSampleSubstanceRecord() {
                            HumanMonitoringSampleSubstances = sampleCompounds,
                            HumanMonitoringSample = s.HumanMonitoringSample
                        };
                    })
                    .ToList();
                result.Add(new HumanMonitoringSampleSubstanceCollection(
                    sampleCollection.SamplingMethod, 
                    hbmSampleSubstanceRecords)
                );
            }
            return result;
        }

        private SampleCompound createSampleSubstanceRecord(
            SampleCompound sampleSubstance
        ) {
            var clone = sampleSubstance.Clone();
            if (sampleSubstance.IsMissingValue) {
                clone.Residue = 0;
                clone.ResType = ResType.VAL;
            }
            return clone;
        }
    }
}
