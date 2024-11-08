using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {
    public class SpecificGravityFromCreatinineCorrelationCalculator : CorrectionCalculator {

        private readonly double _specificGravityConversionFactor = double.NaN;

        public SpecificGravityFromCreatinineCorrelationCalculator(
            List<string> substancesExcludedFromStandardisation,
            double? specificGravityConversionFactor
        ) : base(substancesExcludedFromStandardisation) {
            _specificGravityConversionFactor = specificGravityConversionFactor.Value;
        }

        protected override bool AppliesComputeResidueCorrection(
         HumanMonitoringSampleSubstanceCollection sampleCollection
         ) {
            return sampleCollection.SamplingMethod.IsUrine;
        }

        protected override double getUnitAlignment(
            HumanMonitoringSampleSubstanceCollection sampleCollection,
            out ConcentrationUnit targetConcentrationUnit,
            out ExpressionType targetExpressionType
        ) {
            // For specific gravity, derived from creatinine standardised values, the unit remains the original units (gram/L)
            targetConcentrationUnit = sampleCollection.ConcentrationUnit;
            targetExpressionType = ExpressionType.SpecificGravity;

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

            double? correctionFactor = sampleSubstanceRecord?.HumanMonitoringSample.Creatinine / (unitAlignmentFactor * _specificGravityConversionFactor);

            var clone = sampleSubstance.Clone();
            if (correctionFactor.HasValue) {
                clone.Residue = sampleSubstance.Residue / correctionFactor.Value;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }

            return clone;
        }
    }
}