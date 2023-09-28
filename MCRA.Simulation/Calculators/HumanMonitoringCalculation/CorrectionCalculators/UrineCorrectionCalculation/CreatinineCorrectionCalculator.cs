using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {

    public class CreatinineCorrectionCalculator : CorrectionCalculator {

        public CreatinineCorrectionCalculator(List<string> substancesExcludedFromStandardisation)
           : base(substancesExcludedFromStandardisation) {
        }

        public override List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                if (sampleCollection.SamplingMethod.IsUrine) {

                    var creatinineAlignmentFactor = getAlignmentFactor(
                        sampleCollection.CreatConcentrationUnit,
                        sampleCollection.ConcentrationUnit.GetConcentrationMassUnit()
                    );

                    // This conversion will always express creatinine as grams
                    // May need to be changed in the future
                    var substanceAmountUnit = sampleCollection.ConcentrationUnit.GetSubstanceAmountUnit();

                    // Split sample substance collection in two collections:
                    // - one for creatinine standardised substances, with concentrations expressed per g creatinine
                    // - and one for the substances that are excluded from creatinine standardisation.
                    var substancesForCreatinineCorrection = getSubstancesForCreatinineCorrection(sampleCollection);
                    var creatinineAdjustedSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Where(c => substancesForCreatinineCorrection.Contains(c.MeasuredSubstance))
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample.HumanMonitoringSample.Creatinine / creatinineAlignmentFactor
                                 ))
                                .ToDictionary(c => c.MeasuredSubstance);
                            return new HumanMonitoringSampleSubstanceRecord() {
                                HumanMonitoringSampleSubstances = sampleCompounds,
                                HumanMonitoringSample = sample.HumanMonitoringSample
                            };
                        })
                        .ToList();

                    // If we have any adjusted sample substance record, then add this collection
                    if (creatinineAdjustedSampleSubstanceRecords.Any(r => r.HumanMonitoringSampleSubstances.Any())) {
                        result.Add(
                            new HumanMonitoringSampleSubstanceCollection(
                                sampleCollection.SamplingMethod,
                                creatinineAdjustedSampleSubstanceRecords,
                                ConcentrationUnitExtensions.Create(substanceAmountUnit, ConcentrationMassUnit.Grams),
                                ExpressionType.Creatinine,
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
                                .Where(c => !substancesForCreatinineCorrection.Contains(c.MeasuredSubstance))
                                .Select(r => getSampleSubstance(r, null))
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
           double? creatinine
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
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }

            return clone;
        }

        /// <summary>
        /// Gets the collection (HashSet) of substances for which creatinine correction is required.
        /// </summary>
        /// <param name="sampleCollection"></param>
        /// <returns></returns>
        protected HashSet<Compound> getSubstancesForCreatinineCorrection(
            HumanMonitoringSampleSubstanceCollection sampleCollection
        ) {
            var allSubstances = sampleCollection.HumanMonitoringSampleSubstanceRecords
                .SelectMany(r => r.HumanMonitoringSampleSubstances.Keys)
                .Distinct();
            var creatinineCorrection = allSubstances
                .Where(r => !SubstancesExcludedFromStandardisation.Contains(r.Code))
                .ToHashSet();
            return creatinineCorrection;
        }
    }
}
