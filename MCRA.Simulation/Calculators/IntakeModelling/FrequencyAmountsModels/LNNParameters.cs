using MCRA.Utils;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    /// <summary>
    /// Parameters and transformed parameters of the LNN model.
    /// </summary>
    public class LNNParameters {

        /// <summary>
        /// Parameters LNN
        /// </summary>
        public LNNParameterEstimate Parameters { get; set; } = new();

        /// <summary>
        /// Transformed base parameters LNN. The set of parameters that is always needed.
        /// </summary>
        public LNNParameterEstimate ParametersT { get; set; } = new();

        /// <summary>
        /// Regression Coefficients for the Frequency model (including the constant).
        /// </summary>
        public List<double> FreqEstimates { get; set; }

        /// <summary>
        /// Regression Coefficients for the Amount model (including the constant).
        /// </summary>
        public List<double> AmountEstimates { get; set; }

        public LNNParameters() { }

        public LNNParameters(
            FrequencyModelSummary frequencyModelSummary,
            NormalAmountsModelSummary normalAmountsModelSummary,
            double power
        ) {
            FreqEstimates = [.. frequencyModelSummary.FrequencyModelEstimates.Select(c => c.Estimate)];
            AmountEstimates = [.. normalAmountsModelSummary.AmountModelEstimates.Select(c => c.Estimate)];
            Parameters.Power = power;
            Parameters.Dispersion = frequencyModelSummary.DispersionEstimates.Estimate;
            Parameters.VarianceBetween = normalAmountsModelSummary.VarianceBetween.Estimate;
            Parameters.VarianceWithin = normalAmountsModelSummary.VarianceWithin.Estimate;
            ParametersT.Dispersion = UtilityFunctions.LogBound(Parameters.Dispersion);
            ParametersT.VarianceBetween = UtilityFunctions.LogBound(Parameters.VarianceBetween);
            ParametersT.VarianceWithin = UtilityFunctions.LogBound(Parameters.VarianceWithin);
            ParametersT.Correlation = UtilityFunctions.LogBound((1.0 + Parameters.Correlation) / (1.0 - Parameters.Correlation));
        }

        /// <summary>
        /// Back-transformed parameters.
        /// </summary>
        public void InverseTransform() {
            Parameters.Dispersion = UtilityFunctions.ExpBound(ParametersT.Dispersion);
            Parameters.VarianceBetween = UtilityFunctions.ExpBound(ParametersT.VarianceBetween);
            Parameters.VarianceWithin = UtilityFunctions.ExpBound(ParametersT.VarianceWithin);
            Parameters.Correlation = 2.0 / (1.0 + UtilityFunctions.ExpBound(-ParametersT.Correlation)) - 1.0;
        }
    }
}
