using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FrequenciesModelGraphicsSection : SummarySection{
        public ConditionalPredictionResults Predictions = new();
        public string CofactorName { get; set; }
        public string CovariableName { get; set; }

        public void Summarize(ActionData data, BBNModel bbnModel) {
            Predictions = bbnModel.FrequencyModel.GetConditionalPredictions();
            CofactorName = data.Cofactor?.Name;
            CovariableName = data.Covariable?.Name;
        }

        public void Summarize(ActionData data, LNN0Model lnn0Model) {
            Predictions = lnn0Model.FrequencyModel.GetConditionalPredictions();
            CofactorName = data.Cofactor?.Name;
            CovariableName = data.Covariable?.Name;
        }
    }
}
