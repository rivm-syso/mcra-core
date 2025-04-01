using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators {
    public abstract class CorrectionCalculator : ICorrectionCalculator {

        public CorrectionCalculator(List<string> substancesExcludedFromStandardisation) {
            SubstancesExcludedFromStandardisation = substancesExcludedFromStandardisation;
        }

        protected List<string> SubstancesExcludedFromStandardisation { get; private set; }

        public virtual List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
          ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                if (AppliesComputeResidueCorrection(sampleCollection)) {

                    var unitAlignmentFactor = getUnitAlignment(
                        sampleCollection,
                        out ConcentrationUnit correctedConcentrationUnit,
                        out ExpressionType correctedExpressionType);

                    // Split sample substance collection in two collections:
                    // - one for corrected substances
                    // - one for non-corrected substances, i.e. those substances that are excluded from correction.
                    var substancesCorrection = getSubstancesForCorrection(sampleCollection);
                    var correctedSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Where(c => substancesCorrection.Contains(c.MeasuredSubstance))
                                .Select(r => getSampleSubstance(
                                    r,
                                    sample,
                                    unitAlignmentFactor
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

        /// <summary>
        /// Return true when a residue correction should be applied for this collection, false otherwise.
        /// To be overwritten, depends on type of corrector.
        /// </summary>
        protected virtual bool AppliesComputeResidueCorrection(
            HumanMonitoringSampleSubstanceCollection sampleCollection
            ) {
            return false;
        }

        /// <summary>
        /// Calculates and returns the corrected residue value.
        /// </summary>
        protected virtual SampleCompound getSampleSubstance(
           SampleCompound sampleSubstance,
           HumanMonitoringSampleSubstanceRecord sampleSubstanceRecord,
           double unitAlignmentFactor = 1
        ) {
            return sampleSubstance;
        }

        /// <summary>
        /// Determines the unit conversion parameters from the original sample unit to the unit associated
        /// with the corrected residue value.
        /// </summary>
        protected virtual double getUnitAlignment(
            HumanMonitoringSampleSubstanceCollection sampleCollection,
            out ConcentrationUnit targetConcentrationUnit,
            out ExpressionType expressionType
        ) {
            targetConcentrationUnit = default;
            expressionType = default;
            return 1;
        }

        /// <summary>
        /// Calculates the factor to convert from a source concentration unit, as expressed per volume,
        /// to a target unit that is expressed per mass.
        /// </summary>
        protected double getAlignmentFactor(
            ConcentrationUnit sourceConcentrationUnit,
            ConcentrationMassUnit targetMassUnit
        ) {
            var massUnit = sourceConcentrationUnit.GetConcentrationMassUnit();
            var amountUnit = sourceConcentrationUnit.GetSubstanceAmountUnit();
            var multiplier1 = massUnit.GetMultiplicationFactor(targetMassUnit);
            var multiplier2 = amountUnit.GetMultiplicationFactor(SubstanceAmountUnit.Grams, 1);
            var multiplier = multiplier1 / multiplier2;
            return multiplier;
        }

        /// <summary>
        /// Gets the substances for which correction is required.
        /// </summary>
        protected virtual HashSet<Compound> getSubstancesForCorrection(
            HumanMonitoringSampleSubstanceCollection sampleCollection
        ) {
            var allSubstances = sampleCollection.HumanMonitoringSampleSubstanceRecords
                .SelectMany(r => r.HumanMonitoringSampleSubstances.Keys)
                .Distinct();
            var substancesForCorrection = allSubstances
                .Where(r => !SubstancesExcludedFromStandardisation.Contains(r.Code))
                .ToHashSet();
            return substancesForCorrection;
        }
    }
}
