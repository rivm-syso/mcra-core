using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {

    /// <summary>
    /// HBM concentrations standardization calculator for blood to total lipid content based 
    /// on gravimetric analysis.
    /// </summary>
    public class LipidGravimetricCorrectionCalculator : BloodCorrectionCalculator, IBloodCorrectionCalculator {

        public LipidGravimetricCorrectionCalculator(List<string> substancesExcludedFromStandardisation)
           : base(substancesExcludedFromStandardisation) {
        }
        public List<HumanMonitoringSampleSubstanceCollection> ComputeTotalLipidCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ConcentrationUnit targetUnit) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var totalLipidAlignmentFactor = getAlignmentFactor(targetUnit.GetConcentrationMassUnit(), ConcentrationUnit.mgPerdL);
                if (sampleCollection.SamplingMethod.IsBlood) {
                    var newSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample.HumanMonitoringSample.LipidGrav / totalLipidAlignmentFactor
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
                        targetUnit,
                        ExpressionType.Lipids,
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
        /// Not corrected for units other than mg/dL.
        /// </summary>
        private SampleCompound getSampleSubstance(
           SampleCompound sampleSubstance,
           double? lipidGrav) {
            if (sampleSubstance.IsMissingValue) {
                return sampleSubstance;
            }

            if (sampleSubstance.MeasuredSubstance.IsLipidSoluble != true || SubstancesExcludedFromStandardisation.Contains(sampleSubstance.MeasuredSubstance.Code)) {
                return sampleSubstance;
            }
            var clone = sampleSubstance.Clone();
            if (lipidGrav.HasValue && lipidGrav.Value != 0D) {
                clone.Residue = sampleSubstance.Residue / lipidGrav.Value;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }
            return clone;
        }

        /// <summary>
        /// Express results always in gram lipids (g lipid).
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
