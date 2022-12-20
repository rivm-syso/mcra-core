using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NormalAmountsModelGraphicsSection : SummarySection {
        public ConditionalPredictionResults Predictions = new ConditionalPredictionResults();
        public string CofactorName { get; set; }
        public string CovariableName { get; set; }

        public void Summarize(ActionData data, BBNModel bbnModel) {
            Predictions = bbnModel.AmountModel.GetConditionalPredictions();
            CofactorName = data.Cofactor?.Name;
            CovariableName = data.Covariable?.Name;
        }

        public void Summarize(ActionData data, LNN0Model lnn0Model) {
            Predictions = lnn0Model.AmountModel.GetConditionalPredictions();
            CofactorName = data.Cofactor?.Name;
            CovariableName = data.Covariable?.Name;
        }
    }
}
