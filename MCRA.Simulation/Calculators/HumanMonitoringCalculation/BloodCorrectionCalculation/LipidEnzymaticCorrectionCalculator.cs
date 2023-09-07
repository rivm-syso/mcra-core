using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.BloodCorrectionCalculation {

    /// <summary>
    /// HBM concentrations standardization calculator for blood to total lipid content based 
    /// on enzymatic summation analysis.
    /// </summary>
    public class LipidEnzymaticCorrectionCalculator : BloodCorrectionCalculatorBase, IBloodCorrectionCalculator {

        public LipidEnzymaticCorrectionCalculator(List<string> substancesExcludedFromStandardisation)
           : base(substancesExcludedFromStandardisation) {
        }

        public List<HumanMonitoringSampleSubstanceCollection> ComputeTotalLipidCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {

                // Only do correction for the HBM sample substance collection(s) with matrix blood
                if (sampleCollection.SamplingMethod.IsBlood) {
                    // Split sample substance collection in two collections:
                    // - one for lipid soluble substances with concentrations expressed per g lipid
                    // - and one for the substances that are not lipid soluble.

                    // The target substance amount unit is the same as that of the sample substance collection
                    var substanceAmountUnit = sampleCollection.ConcentrationUnit.GetSubstanceAmountUnit();

                    // Get substances for which we want to apply lipid correction
                    var substancesForLipidCorrection = getSubstancesWithLipidCorrection(sampleCollection);

                    // Create lipid adjusted sample substance records.
                    var totalLipidAlignmentFactor = getAlignmentFactor(
                        ConcentrationUnit.mgPerdL,
                        sampleCollection.ConcentrationUnit.GetConcentrationMassUnit()
                    );

                    // Create lipid adjusted sample substance records
                    var lipidAdjustedSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample.HumanMonitoringSample.LipidEnz / totalLipidAlignmentFactor
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
                                    sample.HumanMonitoringSample.LipidEnz / totalLipidAlignmentFactor
                                 ))
                                .Where(c => !substancesForLipidCorrection.Contains(c.MeasuredSubstance))
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

        private SampleCompound getSampleSubstance(
           SampleCompound sampleSubstance,
           double? lipidEnz
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
            if (lipidEnz.HasValue && lipidEnz.Value != 0D) {
                clone.Residue = sampleSubstance.Residue / lipidEnz.Value;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }

            return clone;
        }
    }
}
