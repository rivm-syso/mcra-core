using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConcentrationModelsTableSection : SummarySection{

        public override bool SaveTemporaryData => true;

        public List<ConcentrationModelRecord> ConcentrationModelRecords { get; set; }

        public void SummarizeUncertain(IDictionary<(Food, Compound), ConcentrationModel> concentrationModels) {
            var modelsLookup = ConcentrationModelRecords.ToDictionary(r => (r.FoodCode, r.CompoundCode));
            foreach (var record in concentrationModels) {
                modelsLookup.TryGetValue((FoodCode: record.Key.Item1.Code, CompoundCode: record.Key.Item2.Code), out var model);
                if (model != null) {
                    if (model.MeanConcentrationUncertaintyValues == null) {
                        model.MeanConcentrationUncertaintyValues = new List<double>();
                    }
                    model.MeanConcentrationUncertaintyValues.Add(record.Value.GetDistributionMean());
                }
            }
        }

        public void SummarizeUncertain(IDictionary<Food, ConcentrationModel> cumulativeConcentrationModels) {
            var modelsLookup = ConcentrationModelRecords.ToDictionary(r => r.FoodCode);
            foreach (var record in cumulativeConcentrationModels) {
                modelsLookup.TryGetValue(record.Key.Code, out var model);
                if (model != null) {
                    if (model.MeanConcentrationUncertaintyValues == null) {
                        model.MeanConcentrationUncertaintyValues = new List<double>();
                    }
                    model.MeanConcentrationUncertaintyValues.Add(record.Value.GetDistributionMean());
                }
            }
        }
    }
}
