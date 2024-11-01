using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {
    /// <summary>
    /// A cross-validation based approach for estimating specific gravity in elementary-school aged children using a nonlinear model.
    /// Stefanie A. Busgang et al. https://doi.org/10.1016/j.envres.2022.114793
    /// </summary>
    public class SpecificGravityFromCreatinineNonlinearModelTwoCalculator : SpecificGravityFromCreatinineNonlinearCalculator {

        public static class BusgangSpecificGravity {
            private const double m2_a = 0.0293;
            private const double m2_b = 0.0195;
            private const double m2_d = 32.7;
            private const double m2_th_age = 0.0283;
            private const double m2_th_sex = 0.068;
            private const double m2_g_age = 0.00137;
            private const double m2_g_sex = 0.000768;
            private const double _ageMean = 9.79;

            public static double Calculate(double cr, double age, GenderType genderType) {
                if (double.IsNaN(age)) {
                    var msg = "Cannot estimate specific gravity using method of Busgang et al. for individuals for which no age was specified.";
                    throw new Exception(msg);
                }
                var ageCent = age - _ageMean;
                var sex = genderType switch {
                    GenderType.Male => 0,
                    GenderType.Female => 1,
                    _ => throw new Exception("Cannot estimate specific gravity using method of Busgang et al. for individuals of which sex was not specified.")
                };
                var var_age = (m2_th_age * ageCent + m2_g_age * ageCent * (cr - m2_d));
                var var_sex = (m2_th_sex * sex + m2_g_sex * sex * (cr - m2_d));
                var specificGravity = 1 + m2_a * Math.Exp(-Math.Exp(-(m2_b * (cr - m2_d) + var_age + var_sex)));
                return specificGravity;
            }
        }

        public SpecificGravityFromCreatinineNonlinearModelTwoCalculator(
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
            var age = sampleSubstanceRecord.Individual.GetAge();
            var gender = sampleSubstanceRecord.Individual.GetGender();
            if (cr.HasValue && age.HasValue && gender != GenderType.Undefined) {
                specificGravity = BusgangSpecificGravity.Calculate(cr.Value, age.Value, gender);
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
