using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {

    /// <summary>
    /// HBM concentrations standardization calculator for blood to total lipid content based 
    /// on gravimetric analysis.
    /// </summary>
    public class LipidGravimetricCorrectionCalculator : BloodCorrectionCalculatorBase, IBloodCorrectionCalculator {

        public LipidGravimetricCorrectionCalculator(List<string> substancesExcludedFromStandardisation)
           : base(substancesExcludedFromStandardisation) {
        }

        public List<HumanMonitoringSampleSubstanceCollection> ComputeTotalLipidCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var substanceAmountUnit = sampleCollection.ConcentrationUnit.GetSubstanceAmountUnit();
                var totalLipidAlignmentFactor = getAlignmentFactor(
                    ConcentrationUnit.mgPerdL,
                    sampleCollection.ConcentrationUnit.GetConcentrationMassUnit()
                );
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
                        sampleCollection.ConcentrationUnit,
                        ExpressionType.None, // Corrected for lipis, but concentration remains expressed per L blood
                        sampleCollection.TriglycConcentrationUnit,
                        sampleCollection.CholestConcentrationUnit,
                        sampleCollection.LipidConcentrationUnit,
                        sampleCollection.CreatConcentrationUnit
                    ));
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
            double? lipidGrav
        ) {
            if (sampleSubstance.IsMissingValue) {
                return sampleSubstance;
            }

            if (sampleSubstance.MeasuredSubstance.IsLipidSoluble != true 
                || SubstancesExcludedFromStandardisation.Contains(sampleSubstance.MeasuredSubstance.Code)
            ) {
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
    }
}
