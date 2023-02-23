using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.UnitVariabilityCalculation {

    /// <summary>
    /// Unit variability based on the Bernoulli distribution
    /// </summary>
    public sealed class BernoulliDistributionModel : UnitVariabilityModel {
        private EstimatesNature _estimatesNature;
        public BernoulliDistributionModel(Food food, UnitVariabilityFactor unitVariabilityFactor, EstimatesNature estimatesNature)
            : base(food, unitVariabilityFactor) {
            _estimatesNature = estimatesNature;
        }

        public override List<IntakePortion> DrawFromDistribution(Food foodAsMeasured, float amount, float residue, IRandom random) {
            var unitIntakePortions = new List<IntakePortion>();
            var intake = 0D;
            if (residue > 0 && amount > 0) {
                // Divide consumption into unitweight portions (count) and a rest term (rest)
                var unitWeight = (float)foodAsMeasured.Properties.UnitWeight;
                var unitsInCompositeSample = this.VariabilityFactor.UnitsInCompositeSample;
                var nPortions = 0;
                if (unitsInCompositeSample > 1) {
                    if (!double.IsNaN(unitWeight)) {
                        nPortions = BMath.Floor(amount / unitWeight);
                    }
                } else {
                    nPortions = 0;
                }
                var rest = amount;
                if (!double.IsNaN(unitWeight)) {
                    rest = amount - nPortions * unitWeight;
                }

                var mu = residue;
                var drawnResidue = 0D;
                for (int i = 0; i < nPortions + 1; i++) {
                    var portion = new IntakePortion();
                    if (random.NextDouble() < 1 / unitsInCompositeSample) {
                        drawnResidue = mu * unitsInCompositeSample;
                        //drawnResidue = mu * this.VariabilityFactor.Factor;
                    } else {
                        drawnResidue = mu;
                        if (_estimatesNature == EstimatesNature.Realistic) {
                            drawnResidue = 0;
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
                //throw new UnitVariabilityFitException(string.Format("Number of units in composite sample information is missing for {0}", this.Food.Name));
                return false;
            }
            if (this.Food.Properties == null) {
                //throw new UnitVariabilityFitException(string.Format("Unitweight is missing for {0}", this.Food.Name));
                return false;
            }
            if (this.Food.Properties.UnitWeight == null) {
                //throw new UnitVariabilityFitException(string.Format("Unitweight is missing for {0}", this.Food.Name));
                return false;
            }
            return true;
        }
    }
}
