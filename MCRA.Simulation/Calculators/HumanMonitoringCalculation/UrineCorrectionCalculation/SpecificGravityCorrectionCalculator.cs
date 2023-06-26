using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.General.UnitDefinitions.Enums;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation {
    public class SpecificGravityCorrectionCalculator : IUrineCorrectionCalculator {

        public List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ConcentrationUnit targetUnit,
            BiologicalMatrix defaultCompartment,
            CompartmentUnitCollector compartmentUnitCollector
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                if (sampleCollection.SamplingMethod.IsUrine) {
                    var newSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample.HumanMonitoringSample.SpecificGravity,
                                    sample.HumanMonitoringSample.SpecificGravityCorrectionFactor,
                                    targetUnit,
                                    defaultCompartment,
                                    compartmentUnitCollector
                                 ))
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
                        sampleCollection.TriglycConcentrationUnit,
                        sampleCollection.CholestConcentrationUnit,
                        sampleCollection.LipidConcentrationUnit,
                        sampleCollection.CreatConcentrationUnit
                    )
                    );
                } else {
                    result.Add(sampleCollection);
                }
            }
            return result;
        }

        private SampleCompound getSampleSubstance(
           SampleCompound sampleSubstance,
           double? specificGravity,
           double? specificGravityCorrectionFactor,
           ConcentrationUnit targetUnit,
           BiologicalMatrix defaultBiologicalMatrix,
           CompartmentUnitCollector compartmentUnitCollector
       ) {
            var clone = sampleSubstance.Clone();
            if (specificGravityCorrectionFactor.HasValue) {
                clone.Residue = specificGravityCorrectionFactor.Value * sampleSubstance.Residue;
            } else if (specificGravity.HasValue) {
                clone.Residue = (1.024 - 1) / (specificGravity.Value - 1) * sampleSubstance.Residue;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }
            compartmentUnitCollector.EnsureUnit(targetUnit.GetSubstanceAmountUnit(), targetUnit.GetConcentrationMassUnit(), defaultBiologicalMatrix);
            return clone;
        }
    }
}
