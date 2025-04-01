using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {

    public class CreatinineCorrectionCalculator : CorrectionCalculator {

        public CreatinineCorrectionCalculator(
            List<string> substancesExcludedFromStandardisation
        )
           : base(substancesExcludedFromStandardisation) {
        }

        protected override bool AppliesComputeResidueCorrection (
           HumanMonitoringSampleSubstanceCollection sampleCollection
           ) {
            return sampleCollection.SamplingMethod.IsUrine;
        }

        /// <summary>
        /// Calculates the factor to convert from a source concentration unit, as expressed per volume,
        /// to a target unit that is expressed per mass.
        /// </summary>
        protected override double getUnitAlignment(
            HumanMonitoringSampleSubstanceCollection sampleCollection,
            out ConcentrationUnit targetConcentrationUnit,
            out ExpressionType targetExpressionType
        ) {
            var substanceAmountUnit = sampleCollection.ConcentrationUnit.GetSubstanceAmountUnit();
            targetConcentrationUnit = ConcentrationUnitExtensions.Create(substanceAmountUnit, ConcentrationMassUnit.Grams);
            targetExpressionType = ExpressionType.Creatinine;
            return getAlignmentFactor(
                        sampleCollection.CreatConcentrationUnit,
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

            double? creatinine = sampleSubstanceRecord?.HumanMonitoringSample.Creatinine / unitAlignmentFactor;

            var clone = sampleSubstance.Clone();
            if (creatinine.HasValue) {
                clone.Residue = sampleSubstance.Residue / creatinine.Value;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }

            return clone;
        }
    }
}
