using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {
    public class SpecificGravityCorrectionCalculator : CorrectionCalculator {

        public SpecificGravityCorrectionCalculator(
            List<string> substancesExcludedFromStandardisation
        )
           : base(substancesExcludedFromStandardisation) {
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
            targetConcentrationUnit = sampleCollection.ConcentrationUnit;
            targetExpressionType = ExpressionType.SpecificGravity;

            return 1D;
        }

        protected override SampleCompound getSampleSubstance(
           SampleCompound sampleSubstance,
           HumanMonitoringSampleSubstanceRecord sampleSubstanceRecord,
           double unitAlignmentFactor = 1
       ) {
            if (sampleSubstance.IsMissingValue) {
                return sampleSubstance;
            }

            var specificGravity = sampleSubstanceRecord?.HumanMonitoringSample.SpecificGravity;
            var specificGravityCorrectionFactor = sampleSubstanceRecord?.HumanMonitoringSample.SpecificGravityCorrectionFactor;

            var clone = sampleSubstance.Clone();
            if (specificGravityCorrectionFactor.HasValue) {
                clone.Residue = specificGravityCorrectionFactor.Value * sampleSubstance.Residue;
            } else if (specificGravity.HasValue) {
                clone.Residue = (1.024 - 1) / (specificGravity.Value - 1) * sampleSubstance.Residue;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }
            return clone;
        }
    }
}
