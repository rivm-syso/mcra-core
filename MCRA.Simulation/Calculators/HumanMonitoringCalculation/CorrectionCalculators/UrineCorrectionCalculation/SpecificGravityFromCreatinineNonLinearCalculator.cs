using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringSampleCompoundCollections;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.CorrectionCalculators.UrineCorrectionCalculation {
    /// <summary>
    /// A cross-validation based approach for estimating specific gravity in elementary-school aged children using a nonlinear model.
    /// Stefanie A. Busgang et al. https://doi.org/10.1016/j.envres.2022.114793
    /// </summary>
    public class SpecificGravityFromCreatinineNonLinearCalculator : CorrectionCalculator {

        public static class BusgangSpecificGravity {
            private const double m2_a = 0.0293;
            private const double m2_b = 0.0195;
            private const double m2_d = 32.7;
            private const double m2_th_age = 0.0283;
            private const double m2_th_sex = 0.068;
            private const double m2_g_age = 0.00137;
            private const double m2_g_sex = 0.000768;
            private const double m1_a = 0.0281;
            private const double m1_b = 0.0194;
            private const double m1_d = 34.5;
            private const double _ageMean = 9.79;

            public static double Calculate(double cr, double? age, GenderType genderType) {
                double specificGravity;
                if (age.HasValue && genderType != GenderType.Undefined) {
                    var ageCent = age.Value - _ageMean;
                    var sex = genderType == GenderType.Male ? 0 : 1;
                    var var_age = (m2_th_age * ageCent + m2_g_age * ageCent * (cr - m2_d));
                    var var_sex = (m2_th_sex * sex + m2_g_sex * sex * (cr - m2_d));
                    specificGravity = 1 + m2_a * Math.Exp(-Math.Exp(-(m2_b * (cr - m2_d) + var_age + var_sex)));
                } else {
                    specificGravity = 1 + m1_a * Math.Exp(-Math.Exp(-(m1_b * (cr - m1_d))));
                }
                return specificGravity;
            }
        }

        public SpecificGravityFromCreatinineNonLinearCalculator(
            List<string> substancesExcludedFromStandardisation
        ) : base(substancesExcludedFromStandardisation) {
        }

        public override List<HumanMonitoringSampleSubstanceCollection> ComputeResidueCorrection(
            ICollection<HumanMonitoringSampleSubstanceCollection> hbmSampleSubstanceCollections
        ) {
            var result = new List<HumanMonitoringSampleSubstanceCollection>();
            foreach (var sampleCollection in hbmSampleSubstanceCollections) {
                if (sampleCollection.SamplingMethod.IsUrine) {
                    var unitAlignmentFactor = getUnitAlignment(
                        sampleCollection,
                        out ConcentrationUnit correctedConcentrationUnit,
                        out ExpressionType correctedExpressionType);

                    var creatinineAlignmentFactor = getCreatinineAlignmentFactor(sampleCollection.CreatConcentrationUnit);

                    var correctedSampleSubstanceRecords = sampleCollection.HumanMonitoringSampleSubstanceRecords
                        .Select(sample => {
                            var sampleCompounds = sample.HumanMonitoringSampleSubstances.Values
                                .Select(r => getSampleSubstanceImpl(
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
                    result.Add(new HumanMonitoringSampleSubstanceCollection(
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
            targetExpressionType = sampleCollection.ExpressionType;

            return 1D;
        }

        private SampleCompound getSampleSubstanceImpl(
            SampleCompound sampleSubstance,
            HumanMonitoringSampleSubstanceRecord sampleSubstanceRecord,
            double creatinineAlignmentFactor
        ) {
            if (sampleSubstance.IsMissingValue) {
                return sampleSubstance;
            }

            if (SubstancesExcludedFromStandardisation.Contains(sampleSubstance.MeasuredSubstance.Code)) {
                return sampleSubstance;
            }

            double? cr = sampleSubstanceRecord?.HumanMonitoringSample.Creatinine / creatinineAlignmentFactor;
            double? specificGravity = null;
            if (cr.HasValue) {
                var age = sampleSubstanceRecord.Individual.GetAge();
                var genderType = sampleSubstanceRecord.Individual.GetGender();
                specificGravity = BusgangSpecificGravity.Calculate(cr.Value, age, genderType);
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
