using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConcentrationModelsTableSection : SummarySection{

        public override bool SaveTemporaryData => true;

        public List<ConcentrationModelRecord> ConcentrationModelRecords { get; set; }

        public void SummarizeUncertain(ICollection<ConcentrationModel> concentrationModels) {
            var modelsLookup = ConcentrationModelRecords.ToDictionary(r => (r.FoodCode, r.CompoundCode));
            foreach (var record in concentrationModels) {
                modelsLookup.TryGetValue((FoodCode: record.Food.Code, CompoundCode: record.Compound.Code), out var model);
                if (model != null) {
                    if (model.MeanConcentrationUncertaintyValues == null) {
                        model.MeanConcentrationUncertaintyValues = new List<double>();
                    }
                    model.MeanConcentrationUncertaintyValues.Add(record.GetDistributionMean());
                }
            }
        }
    }
}
