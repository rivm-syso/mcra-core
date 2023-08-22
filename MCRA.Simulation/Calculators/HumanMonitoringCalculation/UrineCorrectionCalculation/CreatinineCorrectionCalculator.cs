using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation {
    public class CreatinineCorrectionCalculator : UrineCorrectionCalculator, IUrineCorrectionCalculator {

        public CreatinineCorrectionCalculator(List<string> substancesExcludedFromStandardisation)
           : base(substancesExcludedFromStandardisation) {
        }

        public List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ConcentrationUnit targetUnit,
            TimeScaleUnit timeScaleUnit,
            TargetUnitsModel substanceTargetUnits
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var creatinineAlignmentFactor = getAlignmentFactor(targetUnit.GetConcentrationMassUnit(), ConcentrationUnit.mgPerdL);
                if (sampleCollection.SamplingMethod.IsUrine) {
                    var newSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample.HumanMonitoringSample.Creatinine / creatinineAlignmentFactor,
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
           double? creatinine,
           ConcentrationUnit concentrationUnit,
           BiologicalMatrix biologicalMatrix,
           TimeScaleUnit timeScaleUnit,
           TargetUnitsModel substanceTargetUnits
        ) {
            if (sampleSubstance.IsMissingValue) {
                return sampleSubstance;
            }

            if (SubstancesExcludedFromStandardisation.Contains(sampleSubstance.MeasuredSubstance.Code)) {
                return sampleSubstance;
            }

            var clone = sampleSubstance.Clone();
            if (creatinine.HasValue) {
                clone.Residue = sampleSubstance.Residue / creatinine.Value;
            }
            else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }

            substanceTargetUnits.Update(sampleSubstance.ActiveSubstance,
                biologicalMatrix,
                new TargetUnit(concentrationUnit.GetSubstanceAmountUnit(), ConcentrationMassUnit.Grams, timeScaleUnit, biologicalMatrix, ExpressionType.Creatinine));

            return clone;
        }

        /// <summary>
        /// Express results always in gram lipids (g lipid)
        /// </summary>
        private double getAlignmentFactor(ConcentrationMassUnit targetMassUnit, ConcentrationUnit unit) {
            var massUnit = unit.GetConcentrationMassUnit();
            var amountUnit = unit.GetSubstanceAmountUnit();
            var multiplier1 = massUnit.GetMultiplicationFactor(targetMassUnit);
            var multiplier2 = amountUnit.GetMultiplicationFactor(SubstanceAmountUnit.Grams, 1);
            var multiplier = multiplier1 / multiplier2;
            return multiplier;
        }
    }
}
