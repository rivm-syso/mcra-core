using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.UnitVariabilityCalculation {

    /// <summary>
    /// Unit variability based on the LogNormal distribution
    /// </summary>
    public sealed class LogNormalDistributionModel : UnitVariabilityModel
    {

        private double _standardDeviation;
        private MeanValueCorrectionType _meanValueCorrectionType;
        private EstimatesNature _estimatesNature;
        private UnitVariabilityType _unitVariabilityType;
        public LogNormalDistributionModel(
                Food food,
                UnitVariabilityFactor unitVariabilityFactor,
                MeanValueCorrectionType meanValueCorrectionType,
                EstimatesNature estimatesNature,
                UnitVariabilityType unitVariabilityType
            )
            : base(food, unitVariabilityFactor) {
            _meanValueCorrectionType = meanValueCorrectionType;
            _estimatesNature = estimatesNature;
            _unitVariabilityType = unitVariabilityType;
        }

        public override List<IntakePortion> DrawFromDistribution(Food FoodAsMeasured, float amount, float residue, IRandom random) {
            var unitIntakePortions = new List<IntakePortion>();
            var intake = 0D;
            if (residue > 0 & amount > 0) {
                // Divide consumption into unitweight portions (count) and a rest term (rest)
                var unitWeight = (float)FoodAsMeasured.Properties.UnitWeight;
                var nPortions = 0;
                var rest = amount;
                if (!Double.IsNaN(unitWeight)) {
                    nPortions = BMath.Floor(amount / unitWeight);
                    rest = amount - nPortions * unitWeight;
                }

                // Biased mean value at logscale
                var mu = UtilityFunctions.LogBound(residue);
                // Unbiased mean value at logscale
                if (_meanValueCorrectionType == MeanValueCorrectionType.Unbiased) {
                    mu = UtilityFunctions.LogBound(residue) - this._standardDeviation * this._standardDeviation / 2;
                }

                // Add equal portions and rest portion
                for (int i = 0; i < nPortions + 1; i++) {
                    var portion = new IntakePortion();
                    var drawnResidue = UtilityFunctions.ExpBound(this._standardDeviation * NormalDistribution.InvCDF(0, 1, random.NextDouble()) + mu);

                    if (_estimatesNature == EstimatesNature.Conservative) {
                        if (drawnResidue < residue) {
                            drawnResidue = residue;
                        }
                    }

                    portion.Concentration = (float)drawnResidue;

                    if (i < nPortions) {
                        intake += unitWeight * drawnResidue;
                        portion.Amount = unitWeight;
                    } else {
                        intake += rest * drawnResidue;
                        portion.Amount = rest;
                    }
                    if (portion.Amount > 0) {
                        unitIntakePortions.Add(portion);
                    }
                }

            } else {
                unitIntakePortions.Add(new IntakePortion() {
                    Amount = amount,
                    Concentration = (float)residue,
                });
            }
            return unitIntakePortions;
        }

        /// <summary>
        /// Override: calculates the parameters for this unit variability model type.
        /// </summary>
        public override bool CalculateParameters() {
            if (this.VariabilityFactor.UnitsInCompositeSample == 9999 || this.VariabilityFactor.UnitsInCompositeSample == 0) {
                //throw new UnitVariabilityFitException(string.Format("Number of units in composite sample information is missing for {0}", Food.Name));
                return false;
            }
            if (Food.Properties == null) {
                //throw new UnitVariabilityFitException(string.Format("Properties is missing for {0}", Food.Name));
                return false;
            }
            if (Food.Properties.UnitWeight == null) {
                //throw new UnitVariabilityFitException(string.Format("Unitweight is missing for {0}", Food.Name));
                return false;
            }
            try {
                // Check whether the desired variability type is actually available.
                var variabilityType = _unitVariabilityType;
                if (variabilityType == UnitVariabilityType.VariationCoefficient && this.VariabilityFactor.Coefficient == null) {
                    variabilityType = UnitVariabilityType.VariabilityFactor;
                } else if (variabilityType == UnitVariabilityType.VariabilityFactor && this.VariabilityFactor.Factor == null) {
                    variabilityType = UnitVariabilityType.VariationCoefficient;
                }
                switch (variabilityType) {
                    case UnitVariabilityType.VariationCoefficient:
                        var variabilityFactor = (double)this.VariabilityFactor.Coefficient;
                        _standardDeviation = Math.Sqrt(UtilityFunctions.LogBound(Math.Pow(variabilityFactor, 2) + 1));
                        break;
                    case UnitVariabilityType.VariabilityFactor:
                        var b = -2 * 1.96;
                        var c = 2 * UtilityFunctions.LogBound((double)this.VariabilityFactor.Factor);
                        var discriminant = Math.Pow(b, 2) - 4 * c;
                        var d = 0D;
                        if (discriminant >= 0) {
                            d = Math.Sqrt(discriminant);
                        }
                        var root1 = (-b + d) / 2;
                        var root2 = (-b - d) / 2;
                        _standardDeviation = Math.Min(root1, root2);
                        break;
                }
                return true;
            } catch {
                //throw new UnitVariabilityFitException("Lognormal model (unit variability) fails");
                return false;
            }
        }
    }
}
