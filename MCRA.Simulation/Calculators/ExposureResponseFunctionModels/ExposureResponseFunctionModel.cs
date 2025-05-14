using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;
using NCalc;

namespace MCRA.Simulation.Calculators.ExposureResponseFunctions {

    public enum FunctionLevel {
        Nominal,
        Lower,
        Upper
    }
    public class ExposureResponseFunctionModel : IExposureResponseFunctionModel {

        private double _draw;
        public bool IsNominal { get; private set; }

        public ExposureResponseFunction ExposureResponseFunction { get; set; }

        public ExposureResponseFunctionModel(
            ExposureResponseFunction exposureResponseFunction
        ) {
            IsNominal = true;
            ExposureResponseFunction = exposureResponseFunction;
        }

        public virtual void ResampleModelParameters(IRandom random) {
            var rnd = new McraRandomGenerator(random.Next());
            _draw = rnd.NextDouble();
            IsNominal = false;
        }

        public double Compute(double x) {
            if (IsNominal) {
                return compute(x, FunctionLevel.Nominal);
            } else {
                if (ExposureResponseFunction.ExposureResponseType == ExposureResponseType.PerDoubling) {
                    var lower = Math.Log2(compute(x, FunctionLevel.Lower));
                    var upper = Math.Log2(compute(x, FunctionLevel.Upper));
                    return Math.Pow(2, (lower + _draw * (upper - lower)));
                } else {
                    var lower = compute(x, FunctionLevel.Lower);
                    var upper = compute(x, FunctionLevel.Upper);
                    return lower + _draw * (upper - lower);
                }
            }
        }

        public double compute(double x, FunctionLevel functionLevel) {
            var erf = ExposureResponseFunction;
            if (x <= erf.Baseline) {
                return erf.EffectMetric == EffectMetric.NegativeShift |
                    erf.EffectMetric == EffectMetric.PositiveShift ? 0D : 1D;
            }

            Expression erfSpecification;
            if (erf.HasErfSubGroups) {
                var erfSubGroups = erf.ErfSubgroups
                    .OrderBy(r => r.ExposureUpper ?? double.PositiveInfinity)
                    .ToList();
                var i = 0;
                while (i < erfSubGroups.Count && erfSubGroups[i].ExposureUpper < x) {
                    i++;
                }
                if (i == erfSubGroups.Count) {
                    throw new Exception($"Subgroup not defined for {erfSubGroups.First().idModel} for exposure of {x}.");
                }
                erfSpecification = getSubgroupErfSpecification(functionLevel, erfSubGroups, i);
            } else {
                erfSpecification = getErfSpecification(functionLevel, erf);
            }

            if (erf.ExposureResponseType == ExposureResponseType.Function) {
                erfSpecification.Parameters["x"] = x;
                return Convert.ToDouble(erfSpecification.Evaluate());
            } else if (erf.ExposureResponseType == ExposureResponseType.PerDoubling) {
                var doubFac = Convert.ToDouble(erfSpecification.Evaluate());
                return Math.Pow(doubFac, Math.Log2(x / erf.Baseline));
            } else if (erf.ExposureResponseType == ExposureResponseType.PerUnit) {
                var a = Convert.ToDouble(erfSpecification.Evaluate());
                var b = 1 - a * erf.Baseline;
                return a * x + b;
            } else if (erf.ExposureResponseType == ExposureResponseType.Constant) {
                return Convert.ToDouble(erfSpecification.Evaluate());
            } else {
                throw new NotImplementedException();
            }
        }

        private static Expression getErfSpecification(FunctionLevel functionLevel, ExposureResponseFunction erf) {
            Expression erfSpecification;
            switch (functionLevel) {
                case FunctionLevel.Lower:
                    erfSpecification = erf.ExposureResponseSpecificationLower.ExpressionString.Length > 0
                        ? erf.ExposureResponseSpecificationLower
                        : erf.ExposureResponseSpecification;
                    break;
                case FunctionLevel.Upper:
                    erfSpecification = erf.ExposureResponseSpecificationUpper.ExpressionString.Length > 0 ?
                        erf.ExposureResponseSpecificationUpper
                        : erf.ExposureResponseSpecification;
                    break;
                default:
                    erfSpecification = erf.ExposureResponseSpecification;
                    break;
            }

            return erfSpecification;
        }

        private static Expression getSubgroupErfSpecification(FunctionLevel functionLevel, List<ErfSubgroup> erfSubGroups, int i) {
            Expression erfSpecification;
            switch (functionLevel) {
                case FunctionLevel.Lower:
                    erfSpecification = erfSubGroups[i].ExposureResponseSpecificationLower.ExpressionString.Length > 0
                        ? erfSubGroups[i].ExposureResponseSpecificationLower
                        : erfSubGroups[i].ExposureResponseSpecification;
                    break;
                case FunctionLevel.Upper:
                    erfSpecification = erfSubGroups[i].ExposureResponseSpecificationUpper.ExpressionString.Length > 0
                        ? erfSubGroups[i].ExposureResponseSpecificationUpper
                        : erfSubGroups[i].ExposureResponseSpecification;
                    break;
                default:
                    erfSpecification = erfSubGroups[i].ExposureResponseSpecification;
                    break;
            }

            return erfSpecification;
        }
    }
}

