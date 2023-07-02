using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.General.UnitDefinitions.Enums;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation {
    public class CreatinineCorrectionCalculator : IUrineCorrectionCalculator {

        public List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections,
            ConcentrationUnit targetUnit,
            BiologicalMatrix defaultCompartment,
            CompartmentUnitCollector compartmentUnitCollector
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
           double? creatinine,
           ConcentrationUnit targetUnit,
           BiologicalMatrix defaultBiologicalMatrix,
           CompartmentUnitCollector compartmentUnitCollector
        ) {
            var clone = sampleSubstance.Clone();
            if (creatinine.HasValue) {
                clone.Residue = sampleSubstance.Residue / creatinine.Value;
            }
            else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }
            compartmentUnitCollector.EnsureUnit(targetUnit.GetSubstanceAmountUnit(), ConcentrationMassUnit.Grams, defaultBiologicalMatrix, ExpressionType.Creatinine);
            return clone;
        }

        /// <summary>
        /// Express results always in gram lipids (g lipid)
        /// </summary>
        /// <param name="targetUnit"></param>
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