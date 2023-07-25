using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation {
    public class SpecificGravityCorrectionCalculator : IUrineCorrectionCalculator {

        public List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ConcentrationUnit targetUnit,
            TimeScaleUnit timeScaleUnit,
            TargetUnitsModel substanceTargetUnits
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
                                    sample.SamplingMethod.BiologicalMatrix,
                                    timeScaleUnit,
                                    substanceTargetUnits
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
           ConcentrationUnit concentrationUnit,
           BiologicalMatrix biologicalMatrix,
           TimeScaleUnit timeScaleUnit,
           TargetUnitsModel substanceTargetUnits
       ) {
            if (sampleSubstance.IsMissingValue) {
                return sampleSubstance;
            }

            var clone = sampleSubstance.Clone();
            if (specificGravityCorrectionFactor.HasValue) {
                clone.Residue = specificGravityCorrectionFactor.Value * sampleSubstance.Residue;
            } else if (specificGravity.HasValue) {
                clone.Residue = (1.024 - 1) / (specificGravity.Value - 1) * sampleSubstance.Residue;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }

            var targetUnit = new TargetUnit(
                concentrationUnit.GetSubstanceAmountUnit(),
                concentrationUnit.GetConcentrationMassUnit(),
                timeScaleUnit,
                biologicalMatrix
            );
            substanceTargetUnits.Update(sampleSubstance.ActiveSubstance, biologicalMatrix, targetUnit);

            return clone;
        }
    }
}
