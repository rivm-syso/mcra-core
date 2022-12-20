using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.NonDetectImputationCalculators {
    public sealed class HbmNonDetectImputationCalculator {
        private readonly IHbmIndividualDayConcentrationsCalculatorSettings _settings;

        public HbmNonDetectImputationCalculator(IHbmIndividualDayConcentrationsCalculatorSettings settings) {
            _settings = settings;
        }

        public List<HumanMonitoringSampleSubstanceCollection> ImputeNonDetects(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var hbmSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                    .Select(sample => {
                        var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                            .Select(r => getSampleSubstance(r))
                            .ToDictionary(c => c.MeasuredSubstance, c => c);
                        return new HumanMonitoringSampleSubstanceRecord() {
                            HumanMonitoringSampleSubstances = sampleCompounds,
                            HumanMonitoringSample = sample.HumanMonitoringSample
                        };
                    })
                    .ToList();
                result.Add(
                    new HumanMonitoringSampleSubstanceCollection(sampleCollection.SamplingMethod, 
                    hbmSampleSubstanceRecords)
                );
            }
            return result;
        }

        private SampleCompound getSampleSubstance(
            SampleCompound sampleSubstance
        ) {
            var clone = sampleSubstance.Clone();
            if (sampleSubstance.IsCensoredValue) {
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
                clone.ResType = ResType.VAL;
            }
            return clone;
        }
    }
}
