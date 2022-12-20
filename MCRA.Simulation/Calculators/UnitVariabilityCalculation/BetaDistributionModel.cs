using MCRA.Utils;
using MCRA.Utils.Statistics;
//using MathNet.Numerics.Distributions;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using System;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.UnitVariabilityCalculation {

    /// <summary>
    /// Unit variability based on the Beta distribution
    /// </summary>
    public sealed class BetaDistributionModel : UnitVariabilityModel {

        private double _variabilityFactor;
        private EstimatesNature _estimatesNature;
        private UnitVariabilityType _unitVariabilityType;
        public double A { get; private set; }
        public double B { get; private set; }

        public BetaDistributionModel(Food food, UnitVariabilityFactor unitVariabilityFactor, EstimatesNature estimatesNature, UnitVariabilityType unitVariabilityType)
            : base(food, unitVariabilityFactor) {
            _estimatesNature = estimatesNature;
            _unitVariabilityType = unitVariabilityType;
        }

        public override List<IntakePortion> DrawFromDistribution(Food FoodAsMeasured, float amount, float residue, IRandom random) {
            var unitIntakePortions = new List<IntakePortion>();

            if (residue > 0 && amount > 0) {
                // Divide consumption into unitweight portions (count) and a rest term (rest)
                var unitWeight = (float)FoodAsMeasured.Properties.UnitWeight;
                var nPortions = 0;
                if (!Double.IsNaN(unitWeight)) {
                    nPortions = BMath.Floor(amount / unitWeight);
                }
                if (this.VariabilityFactor.UnitsInCompositeSample > 1) {
                    if (!Double.IsNaN(unitWeight)) {
                        nPortions = BMath.Floor(amount / unitWeight);
                    }
                } else {
                    nPortions = 0;
                }
                var rest = amount;
                if (!Double.IsNaN(unitWeight)) {
                    rest = amount - nPortions * unitWeight;
                }
                var stdDev = _variabilityFactor;
                var drawnResidue = residue;

                for (int i = 0; i < nPortions + 1; i++) {
                    var portion = new IntakePortion();

                    if (stdDev != 0 && this.VariabilityFactor.UnitsInCompositeSample != 1) {
                        var mu = (float)BetaDistribution.InvCDF(A, B, random.NextDouble());
                        drawnResidue = mu * residue * (float)this.VariabilityFactor.UnitsInCompositeSample;
                    }

                    if (_estimatesNature == EstimatesNature.Conservative) {
                        if (drawnResidue < residue) {
                            drawnResidue = residue;
                        }
                    }
                    portion.Concentration = drawnResidue;

                    if (i < nPortions) {
                        portion.Amount = unitWeight;
                    } else {
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
            if (this.VariabilityFactor.Factor == this.VariabilityFactor.UnitsInCompositeSample && this.VariabilityFactor.UnitsInCompositeSample > 1) {
                //var message = string.Format("variability factor for {0}, {1} is equal to the units in composite sample (={2}). Should be smaller.", Food.Code, Food.Name, this.VariabilityFactor.UnitsInCompositeSample);
                //throw new Exception(message);
                return false;
            }
            try {
                if (this.VariabilityFactor.UnitsInCompositeSample == 9999 || this.VariabilityFactor.UnitsInCompositeSample == 0) {
                    //throw new UnitVariabilityFitException(string.Format("Number of units in composite sample information is missing for {0}", Food.Name));
                    return false;
                }
                if (Food.Properties == null) {
                    //throw new UnitVariabilityFitException(string.Format("Unitweight is missing for {0}", Food.Name));
                    return false;
                }
                if (Food.Properties.UnitWeight == null) {
                    //throw new UnitVariabilityFitException(string.Format("Unitweight is missing for {0}", Food.Name));
                    return false;
                }
                // Check whether the desired variability type is actually available.
                var variabilityType = _unitVariabilityType;
                if (variabilityType == UnitVariabilityType.VariationCoefficient && this.VariabilityFactor.Coefficient == null) {
                    variabilityType = UnitVariabilityType.VariabilityFactor;
                } else if (variabilityType == UnitVariabilityType.VariabilityFactor && this.VariabilityFactor.Factor == null) {
                    variabilityType = UnitVariabilityType.VariationCoefficient;
                }

                switch (variabilityType) {
                    case UnitVariabilityType.VariationCoefficient:
                        var mu = 1 / this.VariabilityFactor.UnitsInCompositeSample;
                        var maxVariationCoef = Math.Sqrt(this.VariabilityFactor.UnitsInCompositeSample - 1);
                        if (this.VariabilityFactor.Coefficient >= maxVariationCoef) {
                            //LogFile.WriteLine(" For \"{0}\" the coefficient of variation (= {1}) is higher than the maximum possible value.<br>", foodByP.Food.FoodLabel, VariationCoef);
                            this.VariabilityFactor.Coefficient = maxVariationCoef * 0.99999;
                        }
                        _variabilityFactor = (double)this.VariabilityFactor.Coefficient;
                        break;
                    case UnitVariabilityType.VariabilityFactor:
                        _variabilityFactor = (double)this.VariabilityFactor.Factor;

                        var amin = 0.001;
                        var amax = 100D;
                        var a = 0D;
                        var p1 = double.NaN;
                        var p2 = double.NaN;
                        var coefficient = double.NaN;

                        if (this.VariabilityFactor.UnitsInCompositeSample > 1) {
                            var p975 = _variabilityFactor / this.VariabilityFactor.UnitsInCompositeSample;
                            var c = this.VariabilityFactor.UnitsInCompositeSample - 1;
                            for (int i = 0; i < 100; i++) {
                                a = (amin + amax) / 2;
                                if ((amax - amin) / a < 0.001) {
                                    break;
                                }
                                var a1 = amin;
                                var a2 = a;
                                var b1 = c * a1;
                                var b2 = c * a2;
                                p1 = BetaDistribution.CDF(a1, b1, p975) - 0.975;
                                p2 = BetaDistribution.CDF(a2, b2, p975) - 0.975;
                                if ((p1 / p2) <= 0) {
                                    amax = a;
                                } else {
                                    amin = a;
                                }
                            }
                            var b = c * a;
                            coefficient = Math.Sqrt(b / (a * (a + b + 1)));
                        } else {
                            coefficient = 0;
                        }

                        if (_variabilityFactor > 1) {
                            _variabilityFactor = (double)coefficient;
                        } else {
                            _variabilityFactor = 0;
                        }
                        break;
                }
                B = (this.VariabilityFactor.UnitsInCompositeSample - 1) * (this.VariabilityFactor.UnitsInCompositeSample - 1 - Math.Pow(_variabilityFactor, 2))
                    / (this.VariabilityFactor.UnitsInCompositeSample * Math.Pow(_variabilityFactor, 2));
                A = B / (this.VariabilityFactor.UnitsInCompositeSample - 1);

                return true;
            } catch {
                //throw new UnitVariabilityFitException("Beta model (unit variability) fails");
                return false;
            }
        }
    }
}
