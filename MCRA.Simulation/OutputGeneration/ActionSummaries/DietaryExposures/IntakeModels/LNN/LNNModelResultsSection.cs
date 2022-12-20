using System.Collections.Generic;
using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class LNNModelResultsSection : SummarySection {

        public double Power { get; set; }
        public double VarianceBetween { get; set; }
        public double VarianceWithin { get; set; }
        public double DegreesOfFreedom { get; set; }
        public double _2LogLikelihood { get; set; }
        public ParameterEstimates CorrelationEstimates { get; set; }
        public ParameterEstimates VarianceEstimates { get; set; }
        public List<ParameterEstimates> AmountsModelEstimates { get; set; }
        public List<ParameterEstimates> FrequencyModelEstimates { get; set; }
        public List<double> Residuals { get; set; }
        public ErrorMessages Message { get; set; }
        public IntakeModelType FallBackModel { get; set; }

        public void Summarize(LNNModel lnnModel) {
            FallBackModel = lnnModel.FallBackModel;
            if (FallBackModel == IntakeModelType.LNN) {
                this.CorrelationEstimates = lnnModel.FrequencyAmountModelSummary.CorrelationEstimates;
                this.VarianceEstimates = lnnModel.FrequencyAmountModelSummary.DispersionEstimates;
                this.Power = lnnModel.FrequencyAmountModelSummary.Power;
                this.VarianceBetween = lnnModel.FrequencyAmountModelSummary.VarianceBetween;
                this.VarianceWithin = lnnModel.FrequencyAmountModelSummary.VarianceWithin;
                this.AmountsModelEstimates = lnnModel.FrequencyAmountModelSummary.AmountModelEstimates;
                this.FrequencyModelEstimates = lnnModel.FrequencyAmountModelSummary.FrequencyModelEstimates;
                this.DegreesOfFreedom = lnnModel.FrequencyAmountModelSummary.DegreesOfFreedom;
                this._2LogLikelihood = lnnModel.FrequencyAmountModelSummary._2LogLikelihood;
                this.Message = lnnModel.FrequencyAmountModelSummary.ErrorMessage;
            }
        }
    }
}
