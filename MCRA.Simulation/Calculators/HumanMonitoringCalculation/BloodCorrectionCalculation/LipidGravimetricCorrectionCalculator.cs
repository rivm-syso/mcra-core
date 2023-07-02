using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.General.UnitDefinitions.Enums;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {

    /// <summary>
    /// HBM concentrations standardization calculator for blood to total lipid content based 
    /// on gravimatic analysis.
    /// </summary>
    public class LipidGravimetricCorrectionCalculator : IBloodCorrectionCalculator {
        public List<HumanMonitoringSampleSubstanceCollection> ComputeTotalLipidCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ConcentrationUnit targetUnit,
            BiologicalMatrix defaultCompartment,
            CompartmentUnitCollector compartmentUnitCollector
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var totalLipidAlignmentFactor = getAlignmentFactor(targetUnit.GetConcentrationMassUnit(), ConcentrationUnit.mgPerdL);
                if (sampleCollection.SamplingMethod.IsBlood) {
                    var newSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample.HumanMonitoringSample.LipidGrav / totalLipidAlignmentFactor,
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

        /// <summary>
        /// Not corrected for units other than mg/dL.
        /// </summary>
        /// <param name="sampleSubstance"></param>
        /// <param name="lipidGrav"></param>
        /// <returns></returns>
        private SampleCompound getSampleSubstance(
           SampleCompound sampleSubstance,
           double? lipidGrav,
           ConcentrationUnit targetUnit,
           BiologicalMatrix defaultBiologicalMatrix,
           CompartmentUnitCollector compartmentUnitCollector
       ) {
            if (sampleSubstance.MeasuredSubstance.IsLipidSoluble != true) {
                compartmentUnitCollector.EnsureUnit(targetUnit.GetSubstanceAmountUnit(), targetUnit.GetConcentrationMassUnit(), defaultBiologicalMatrix);
                return sampleSubstance;
            }
            var clone = sampleSubstance.Clone();
            if (lipidGrav.HasValue && lipidGrav.Value != 0D) {
                clone.Residue = sampleSubstance.Residue / lipidGrav.Value;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }
            compartmentUnitCollector.EnsureUnit(targetUnit.GetSubstanceAmountUnit(), ConcentrationMassUnit.Grams, defaultBiologicalMatrix, ExpressionType.Lipids);
            return clone;
        }

        /// <summary>
        /// Express results always in gram lipids (g lipid).
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
    }
}
