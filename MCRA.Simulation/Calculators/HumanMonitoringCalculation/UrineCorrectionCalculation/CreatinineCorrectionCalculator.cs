using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.UrineCorrectionCalculation {

    public class CreatinineCorrectionCalculator : UrineCorrectionCalculator, IUrineCorrectionCalculator {

        public CreatinineCorrectionCalculator(List<string> substancesExcludedFromStandardisation)
           : base(substancesExcludedFromStandardisation) {
        }

        public List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                var creatinineUnit = sampleCollection.CreatConcentrationUnit; // default is mg creatinine per dL urine
                var creatinineAlignmentFactor = getAlignmentFactor(
                    sampleCollection.ConcentrationUnit.GetConcentrationMassUnit(), 
                    creatinineUnit
                );

                // This conversion will always express creatinine as grams
                // May need to be changed in the future
                var substanceAmountUnit = sampleCollection.ConcentrationUnit.GetSubstanceAmountUnit();
                var concentrationMassUnit = ConcentrationMassUnit.Grams;
                var concentrationUnit = ConcentrationUnitExtensions.Create(substanceAmountUnit, concentrationMassUnit);

                if (sampleCollection.SamplingMethod.IsUrine) {
                    var newSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
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
                    result.Add(
                        new HumanMonitoringSampleSubstanceCollection(
                            sampleCollection.SamplingMethod,
                            newSampleSubstanceRecords,
                            concentrationUnit,
                            ExpressionType.Creatinine,
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
            }
            else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }

            return clone;
        }

        /// <summary>
        /// Express results always in gram lipids (g lipid)
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
