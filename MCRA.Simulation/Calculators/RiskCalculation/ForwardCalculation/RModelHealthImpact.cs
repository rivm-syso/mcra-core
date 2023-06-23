using System.Globalization;
using MCRA.Utils.R.REngines;

namespace MCRA.Simulation.Calculators.RiskCalculation.ForwardCalculation {
    public class RModelHealthImpact {

        public void CalculateEffectValue(List<IndividualEffect> individualEffects, string modelEquation, string modelParameters, double intakeUnitCorrectionFactor) {
            if (!string.IsNullOrEmpty(modelEquation)) {
                var x = individualEffects
                    .Select(c => c.EquivalentTestSystemDose * intakeUnitCorrectionFactor)
                    .ToList();
                var y = new List<double>();
                var parameterValueString = modelParameters.Split(',');
                var parameterNames = new List<string>();
                var parameterValues = new List<double>();
                foreach (var item in parameterValueString) {
                    var split = item.Split('=');
                    parameterNames.Add(split[0]);
                    parameterValues.Add(Double.Parse(split[1], CultureInfo.InvariantCulture));
                }
                using (var R = new RDotNetEngine()) {
                    try {
                        R.SetSymbol("x", x.ToArray());
                        for (int i = 0; i < parameterValues.Count; i++) {
                            R.SetSymbol(parameterNames.ElementAt(i), parameterValues.ElementAt(i));
                        }
                        var formula = "MCRAResponse <- " + modelEquation;
                        y = R.EvaluateNumericVector(formula);
                        //expectedValue = y.Average();
                    } catch {
                    }
                };
                for (int i = 0; i < individualEffects.Count; i++) {
                    individualEffects.ElementAt(i).PredictedHealthEffect = y.ElementAt(i);
                }
            }
        }
    }
}
