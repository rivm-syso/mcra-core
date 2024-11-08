using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {
    /// <summary>
    /// A cross-validation based approach for estimating specific gravity in elementary-school aged children using a nonlinear model.
    /// Stefanie A. Busgang et al. https://doi.org/10.1016/j.envres.2022.114793
    /// </summary>
    public class SpecificGravityFromCreatinineNonlinearModelOneCalculator : SpecificGravityFromCreatinineNonlinearCalculator {

        public static class BusgangSpecificGravity {
            private const double m1_a = 0.0281;
            private const double m1_b = 0.0194;
            private const double m1_d = 34.5;

            public static double Calculate(double cr) {
                return 1 + m1_a * Math.Exp(-Math.Exp(-(m1_b * (cr - m1_d))));
            }
        }

        public SpecificGravityFromCreatinineNonlinearModelOneCalculator(
            List<string> substancesExcludedFromStandardisation
        ) : base(substancesExcludedFromStandardisation) {
        }

        protected override SampleCompound getSampleSubstanceBusgang(
            SampleCompound sampleSubstance,
            HumanMonitoringSampleSubstanceRecord sampleSubstanceRecord,
            double creatinineAlignmentFactor
        ) {
            if (sampleSubstance.IsMissingValue) {
                return sampleSubstance;
            }

            double? cr = sampleSubstanceRecord?.HumanMonitoringSample.Creatinine / creatinineAlignmentFactor;
            double? specificGravity = null;
            if (cr.HasValue) {
                specificGravity = BusgangSpecificGravity.Calculate(cr.Value);
            }

            var clone = sampleSubstance.Clone();
            if (specificGravity.HasValue) {
                clone.Residue = (1.024 - 1) / (specificGravity.Value - 1) * sampleSubstance.Residue;
            } else {
                clone.Residue = double.NaN;
                clone.ResType = ResType.MV;
            }
            return clone;
        }
    }
}
