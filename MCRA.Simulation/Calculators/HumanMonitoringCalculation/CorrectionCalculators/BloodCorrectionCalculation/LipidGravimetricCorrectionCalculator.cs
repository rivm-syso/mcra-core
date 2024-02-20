using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.BloodCorrectionCalculation {

    /// <summary>
    /// HBM concentrations standardization calculator for blood to total lipid content based 
    /// on gravimetric analysis.
    /// </summary>
    public class LipidGravimetricCorrectionCalculator : BloodCorrectionCalculatorBase {

        public LipidGravimetricCorrectionCalculator(
            List<string> substancesExcludedFromStandardisation
        )
           : base(substancesExcludedFromStandardisation) {
        }

        public override List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
                ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
            ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                if (sampleCollection.SamplingMethod.IsBlood) {
                    // Create lipid adjusted sample substance records.
                    var unitAlignmentFactor = getUnitAlignment(
                       sampleCollection,
                       out ConcentrationUnit correctedConcentrationUnit,
                       out ExpressionType correctedExpressionType);

                    // Get substances for which we want to apply lipid correction
                    var substancesForLipidCorrection = getSubstancesForCorrection(sampleCollection);

                    // Create lipid adjusted sample substance records
                    var lipidAdjustedSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample,
                                    unitAlignmentFactor
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
                                correctedConcentrationUnit,
                                correctedExpressionType,
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
                                    sample,
                                    unitAlignmentFactor
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

        protected override double getUnitAlignment(
            HumanMonitoringSampleSubstanceCollection sampleCollection,
            out ConcentrationUnit targetConcentrationUnit,
            out ExpressionType targetExpressionType
        ) {
            var substanceAmountUnit = sampleCollection.ConcentrationUnit.GetSubstanceAmountUnit();
            targetConcentrationUnit = ConcentrationUnitExtensions.Create(substanceAmountUnit, ConcentrationMassUnit.Grams);
            targetExpressionType = ExpressionType.Lipids;

            return getAlignmentFactor(
                        ConcentrationUnit.mgPerdL,
                        sampleCollection.ConcentrationUnit.GetConcentrationMassUnit()
                    );
        }

        protected override SampleCompound getSampleSubstance(
           SampleCompound sampleSubstance,
           HumanMonitoringSampleSubstanceRecord sampleSubstanceRecord,
           double unitAlignmentFactor = 1
        ) {
            if (sampleSubstance.IsMissingValue) {
                return sampleSubstance;
            }

            if (!sampleSubstance.MeasuredSubstance.IsLipidSoluble
                || SubstancesExcludedFromStandardisation.Contains(sampleSubstance.MeasuredSubstance.Code)
            ) {
                return sampleSubstance;
            }

            double? lipidGrav = sampleSubstanceRecord?.HumanMonitoringSample.LipidGrav / unitAlignmentFactor;

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
