using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;
using MCRA.Simulation.Units;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {

    /// <summary>
    /// HBM concentrations standardization calculator for blood to total lipid content using
    /// method of Bernet et al 2007.
    /// </summary>
    public class LipidBernertCorrectionCalculator : IBloodCorrectionCalculator {

        /// <summary>
        /// Default unit for Bernert Lipid correction because of regression of PL on TC with intercept 62.3 is in mg/dL.
        /// </summary>
        public List<HumanMonitoringSampleSubstanceCollection> ComputeTotalLipidCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ConcentrationUnit targetUnit,
            TimeScaleUnit timeScaleUnit,
            TargetUnitsModel substanceTargetUnits
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var defaultTriglycerideAlignmentFactor = getBernertAlignmentFactor(sampleCollection.TriglycConcentrationUnit);
                var defaultCholesterolAlignmentFactor = getBernertAlignmentFactor(sampleCollection.CholestConcentrationUnit);
                var overallAlignmentFactor = getAlignmentFactor(targetUnit.GetConcentrationMassUnit(), ConcentrationUnit.mgPerdL);

                if (sampleCollection.SamplingMethod.IsBlood) {
                    var newSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample.HumanMonitoringSample.Cholesterol * defaultCholesterolAlignmentFactor,
                                    sample.HumanMonitoringSample.Triglycerides * defaultTriglycerideAlignmentFactor,
                                    overallAlignmentFactor,
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

        /// <summary>
        /// Bernert et al 2007:
        /// Calculation of serum ‘‘total lipid’’ concentrations for the adjustment of persistent organohalogen 
        /// toxicant measurements in human samples. Chemosphere 68 (2007) 824–831.
        /// </summary>
        /// <param name="sampleSubstance"></param>
        /// <param name="cholesterol"></param>
        /// <param name="triglycerides"></param>
        /// <param name="overallAlignmentFactor">intercept of regression of PL on TC</param>
        /// <param name="concentrationUnit"></param>
        /// <param name="biologicalMatrix"></param>
        /// <param name="compartmentUnitCollector"></param>
        /// <returns></returns>
        private SampleCompound getSampleSubstance(
            SampleCompound sampleSubstance,
            double? cholesterol,
            double? triglycerides,
            double overallAlignmentFactor,
            ConcentrationUnit concentrationUnit,
            BiologicalMatrix biologicalMatrix,
            TimeScaleUnit timeScaleUnit,
            TargetUnitsModel substanceTargetUnits
        ) {
            if (sampleSubstance.IsMissingValue) {
                return sampleSubstance;
            }

            if (sampleSubstance.MeasuredSubstance.IsLipidSoluble != true) {
                return sampleSubstance;
            }
            var clone = sampleSubstance.Clone();
            if (cholesterol.HasValue && triglycerides.HasValue) {
                clone.Residue = sampleSubstance.Residue / (2.27 * cholesterol.Value + triglycerides.Value + 62.3) * overallAlignmentFactor;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }

            substanceTargetUnits.Update(sampleSubstance.ActiveSubstance,
                        biologicalMatrix,
                        new TargetUnit(concentrationUnit.GetSubstanceAmountUnit(), ConcentrationMassUnit.Grams, timeScaleUnit, biologicalMatrix, ExpressionType.Lipids));

            return clone;
        }

        /// <summary>
        /// Expres results always in gram lipids (g lipid)
        /// For the Bernert method, calculate correction factor on mg/dL scale, then align.
        /// </summary>
        /// <param name="targetMassUnit"></param>
        /// <param name="unit"></param>
        /// <returns></returns>
        private double getAlignmentFactor(ConcentrationMassUnit targetMassUnit, ConcentrationUnit unit) {
            var massUnit = unit.GetConcentrationMassUnit();
            var amountUnit = unit.GetSubstanceAmountUnit();
            var multiplier1 = massUnit.GetMultiplicationFactor(targetMassUnit);
            var multiplier2 = amountUnit.GetMultiplicationFactor(SubstanceAmountUnit.Grams, 1);
            var multiplier = multiplier1 / multiplier2;
            return multiplier;
        }

        /// <summary>
        /// Express results always in gram lipids (g lipid)
        /// For the Bernert method, calculate correction factor on mg/dL scale, then align.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        private double getBernertAlignmentFactor(ConcentrationUnit unit) {
            var massUnit = unit.GetConcentrationMassUnit();
            var amountUnit = unit.GetSubstanceAmountUnit();
            var multiplier1 = massUnit.GetMultiplicationFactor(ConcentrationMassUnit.Deciliter);
            var multiplier2 = amountUnit.GetMultiplicationFactor(SubstanceAmountUnit.Milligrams, 1);
            var multiplier = multiplier1 / multiplier2;
            return multiplier;
        }
    }
}
