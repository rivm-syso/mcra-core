using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {
    /// <summary>
    /// A cross-validation based approach for estimating specific gravity in elementary-school aged children using a nonlinear model.
    /// Stefanie A. Busgang et al. 2023 https://doi.org/10.1016/j.envres.2022.114793
    /// </summary>
    public abstract class SpecificGravityFromCreatinineNonlinearCalculator : CorrectionCalculator {
        public SpecificGravityFromCreatinineNonlinearCalculator(
            List<string> substancesExcludedFromStandardisation
        ) : base(substancesExcludedFromStandardisation) {
        }

        protected override bool AppliesComputeResidueCorrection(
         HumanMonitoringSampleSubstanceCollection sampleCollection
         ) {
            return sampleCollection.SamplingMethod.IsUrine;
        }

        public override List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
         ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                if (AppliesComputeResidueCorrection(sampleCollection)) {

                    var unitAlignmentFactor = getUnitAlignment(
                        sampleCollection,
                        out ConcentrationUnit correctedConcentrationUnit,
                        out ExpressionType correctedExpressionType);

                    var creatinineAlignmentFactor = getCreatinineAlignmentFactor(sampleCollection.CreatConcentrationUnit);

                    // Split sample substance collection in two collections:
                    // - one for corrected substances
                    // - one for non-corrected substances, i.e. those substances that are excluded from correction.
                    var substancesCorrection = getSubstancesForCorrection(sampleCollection);
                    var correctedSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Where(c => substancesCorrection.Contains(c.MeasuredSubstance))
                                .Select(r => getSampleSubstanceBusgang(
                                    r,
                                    sample,
                                    creatinineAlignmentFactor
                                 ))
                                .ToDictionary(c => c.MeasuredSubstance);
                            return new HumanMonitoringSampleSubstanceRecord() {
                                HumanMonitoringSampleSubstances = sampleCompounds,
                                HumanMonitoringSample = sample.HumanMonitoringSample
                            };
                        })
                        .ToList();

                    // If we have any corrected sample substance record, then add this collection
                    if (correctedSampleSubstanceRecords.Any(r => r.HumanMonitoringSampleSubstances.Any())) {
                        result.Add(
                            new HumanMonitoringSampleSubstanceCollection(
                                sampleCollection.SamplingMethod,
                                correctedSampleSubstanceRecords,
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
                    var uncorrectedSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Where(c => !substancesCorrection.Contains(c.MeasuredSubstance))
                                .ToDictionary(c => c.MeasuredSubstance);
                            return new HumanMonitoringSampleSubstanceRecord() {
                                HumanMonitoringSampleSubstances = sampleCompounds,
                                HumanMonitoringSample = sample.HumanMonitoringSample
                            };
                        })
                        .ToList();

                    // If we have any unadjusted sample substance record, then add this collection
                    if (uncorrectedSampleSubstanceRecords.Any(r => r.HumanMonitoringSampleSubstances.Any())) {
                        result.Add(
                            new HumanMonitoringSampleSubstanceCollection(
                                sampleCollection.SamplingMethod,
                                uncorrectedSampleSubstanceRecords,
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
            // For this model by Busgang et al., first a specific gravity (SG) factor is derived from creatinine, and
            // then this SG factor is applied to the residue values identical as a standard specific gravity normalisation, like in SpecificGravityCorrectionCalculator.
            // In other words, the units remain the same and no unit alignment is needed.
            targetConcentrationUnit = sampleCollection.ConcentrationUnit;
            targetExpressionType = ExpressionType.SpecificGravity;

            return 1D;
        }

        protected abstract SampleCompound getSampleSubstanceBusgang(
            SampleCompound sampleSubstance,
            HumanMonitoringSampleSubstanceRecord sampleSubstanceRecord,
            double creatinineAlignmentFactor
        );

        /// <summary>
        /// For the Busgang method, the calculated SG factor uses creatinine on a mg/dL scale.
        /// So, we should align the HBM creatinine values to this unit.
        /// </summary>
        private double getCreatinineAlignmentFactor(ConcentrationUnit hbmCreatinineUnit) {
            var massUnit = hbmCreatinineUnit.GetConcentrationMassUnit();
            var amountUnit = hbmCreatinineUnit.GetSubstanceAmountUnit();
            var multiplier1 = massUnit.GetMultiplicationFactor(ConcentrationMassUnit.Deciliter);
            var multiplier2 = amountUnit.GetMultiplicationFactor(SubstanceAmountUnit.Milligrams, 1);
            var multiplier = multiplier1 / multiplier2;
            return multiplier;
        }
    }
}
