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
                if (sampleCollection.SamplingMethod.IsBlood) {

                    // The target substance amount unit is the same as that of the sample substance collection
                    var substanceAmountUnit = sampleCollection.ConcentrationUnit.GetSubstanceAmountUnit();

                    // Create lipid adjusted sample substance records.
                    var totalLipidAlignmentFactor = getAlignmentFactor(
                        ConcentrationUnit.mgPerdL,
                        sampleCollection.ConcentrationUnit.GetConcentrationMassUnit()
                    );

                    // Get substances for which we want to apply lipid correction
                    var substancesForLipidCorrection = getSubstancesWithLipidCorrection(sampleCollection);

                    // Create lipid adjusted sample substance records
                    var lipidAdjustedSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample.HumanMonitoringSample.LipidGrav / totalLipidAlignmentFactor
                                ))
                                .Where(c => substancesForLipidCorrection.Contains(c.MeasuredSubstance))
                                .ToDictionary(c => c.MeasuredSubstance);
                            return new HumanMonitoringSampleSubstanceRecord() {
                                HumanMonitoringSampleSubstances = sampleCompounds,
                                HumanMonitoringSample = sample.HumanMonitoringSample
                            };
                        })
                        .ToList();

                    // If we have any adjusted sample substance record, then add this collection
                    if (lipidAdjustedSampleSubstanceRecords.Any(r => r.HumanMonitoringSampleSubstances.Any())) {
                        result.Add(
                            new HumanMonitoringSampleSubstanceCollection(
                                sampleCollection.SamplingMethod,
                                lipidAdjustedSampleSubstanceRecords,
                                ConcentrationUnitExtensions.Create(substanceAmountUnit, ConcentrationMassUnit.Grams),
                                ExpressionType.Lipids,
                                sampleCollection.TriglycConcentrationUnit,
                                sampleCollection.CholestConcentrationUnit,
                                sampleCollection.LipidConcentrationUnit,
                                sampleCollection.CreatConcentrationUnit
                            )
                        );
                    }

                    // Create unadjusted sample substance records.
                    var unadjustedSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample.HumanMonitoringSample.LipidGrav / totalLipidAlignmentFactor
                                 ))
                                .Where(c => substancesForLipidCorrection.Contains(c.MeasuredSubstance))
                                .ToDictionary(c => c.MeasuredSubstance);
                            return new HumanMonitoringSampleSubstanceRecord() {
                                HumanMonitoringSampleSubstances = sampleCompounds,
                                HumanMonitoringSample = sample.HumanMonitoringSample
                            };
                        })
                        .ToList();

                    // If we have any unadjusted sample substance record, then add this collection
                    if (unadjustedSampleSubstanceRecords.Any(r => r.HumanMonitoringSampleSubstances.Any())) {
                        result.Add(
                            new HumanMonitoringSampleSubstanceCollection(
                                sampleCollection.SamplingMethod,
                                unadjustedSampleSubstanceRecords,
                                sampleCollection.ConcentrationUnit,
                                sampleCollection.ExpressionType,
                                sampleCollection.TriglycConcentrationUnit,
                                sampleCollection.CholestConcentrationUnit,
                                sampleCollection.LipidConcentrationUnit,
                                sampleCollection.CreatConcentrationUnit
                            )
                        );
                    }
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
