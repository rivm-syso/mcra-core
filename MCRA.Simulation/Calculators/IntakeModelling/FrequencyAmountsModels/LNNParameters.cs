using System.Collections.Generic;
using MCRA.Utils;
using MCRA.General;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    /// <summary>
    /// Parameters and transformed parameters of the LNN model.
    /// </summary>
    public class LNNParameters {

        /// <summary>
        /// Regression Coefficients for the Frequency model (including the constant).
        /// </summary>
        public List<double> FreqEstimates { get; set; }

        /// <summary>
        /// Regression Coefficients for the Amount model (including the constant).
        /// </summary>
        public List<double> AmountEstimates { get; set; }
        
        /// <summary>
        /// Power transformation parameter (0 is for the Log transform).
        /// </summary>
        public double Power { get; set; }

        /// <summary>
        /// Between individuals variance for frequency model
        /// </summary>
        public double Dispersion { get; set; }
        
        /// <summary>
        /// Between individuals variance for amount model
        /// </summary>
        public double VarianceBetween { get; set; }
        
        /// <summary>
        /// Between days within individuals variance for amount model
        /// </summary>
        public double VarianceWithin { get; set; }
        
        /// <summary>
        /// Correlation between individual effects of frequency and amounts
        /// </summary>
        public double Correlation { get; set; }
        
        /// <summary>
        /// Log Transformed Power transformation parameter (0 is for the Log transform).
        /// </summary>
        public double PowerT { get; set; }
        
        /// <summary>
        /// Log Transformed Between individuals variance for frequency model
        /// </summary>
        public double DispersionT { get; set; }
        
        /// <summary>
        /// Log Transformed Between individuals variance for amount model
        /// </summary>
        public double VarianceBetweenT { get; set; }
        
        /// <summary>
        /// Log Transformed Between days within individuals variance for amount model
        /// </summary>
        public double VarianceWithinT { get; set; }
        
        /// <summary>
        /// Special Transformed Correlation between individual effects of frequency and amounts
        /// </summary>
        public double CorrelationT { get; set; }
        
        /// <summary>
        /// Whether to estimate the Regression coefficients for the Frequency Model
        /// </summary>
        public bool EstimateFrequency { get; set; }
        
        /// <summary>
        /// Whether to estimate the Regression coefficients for the Amount Model
        /// </summary>
        public bool EstimateAmount { get; set; }
        
        /// <summary>
        /// Whether to estimate the Power transformation parameter
        /// </summary>
        public bool EstimatePower { get; set; }
        
        /// <summary>
        /// Whether to estimate the Between individuals variance for frequency model
        /// </summary>
        public bool EstimateDispersion { get; set; }
        
        /// <summary>
        /// Whether to estimate the Between individuals variance for amount model
        /// </summary>
        public bool EstimateVarianceBetween { get; set; }
        
        /// <summary>
        /// Whether to estimate the Between days within individuals variance for amount model
        /// </summary>
        public bool EstimateVarianceWithin { get; set; }
        
        /// <summary>
        /// Whether to estimate the Correlation between individual effects of frequency and amounts
        /// </summary>
        public bool EstimateCorrelation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TransformType TransformType { get; set; }

        /// <summary>
        /// Transforms parameters.
        /// </summary>
        public void Transform() {
            DispersionT = EstimateDispersion ? UtilityFunctions.LogBound(Dispersion) : DispersionT;
            VarianceBetweenT = EstimateVarianceBetween ? UtilityFunctions.LogBound(VarianceBetween) : VarianceBetweenT;
            VarianceWithinT = EstimateVarianceWithin ? UtilityFunctions.LogBound(VarianceWithin) : VarianceWithinT;
            CorrelationT = EstimateCorrelation ? UtilityFunctions.LogBound((1.0 + Correlation) / (1.0 - Correlation)) : CorrelationT;
            PowerT = EstimatePower ? UtilityFunctions.LogBound(Power) : PowerT;
        }

        /// <summary>
        /// Back-transforms parameters.
        /// </summary>
        public void InverseTransform() {
            Dispersion = EstimateDispersion ? UtilityFunctions.ExpBound(DispersionT) : Dispersion;
            VarianceBetween = EstimateVarianceBetween ? UtilityFunctions.ExpBound(VarianceBetweenT) : VarianceBetween;
            VarianceWithin = EstimateVarianceWithin ? UtilityFunctions.ExpBound(VarianceWithinT) : VarianceWithin;
            Correlation = EstimateCorrelation ? (2.0 / (1.0 + UtilityFunctions.ExpBound(-CorrelationT)) - 1.0) : Correlation;
            Power = EstimatePower ? UtilityFunctions.ExpBound(PowerT) : Power;
        }

        /// <summary>
        /// Creates a  copy.
        /// </summary>
        /// <returns>A copy of LNNparameters.</returns>
        public LNNParameters Clone() {
            LNNParameters clone = (LNNParameters)this.MemberwiseClone();
            clone.FreqEstimates = this.FreqEstimates;
            clone.AmountEstimates = this.AmountEstimates;

            //clone.FreqEstimates = new double[this.FreqEstimates.Length];
            //for (int i = 0; i < this.FreqEstimates.Length; i++) {
            //    clone.FreqEstimates[i] = this.FreqEstimates[i];
            //}
            //clone.AmountEstimates = new double[this.AmountEstimates.Length];
            //for (int i = 0; i < this.AmountEstimates.Length; i++) {
            //    clone.AmountEstimates[i] = this.AmountEstimates[i];
            //}
            return clone;
        }
    }
}
