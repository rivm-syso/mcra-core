using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmConcentrationModelsSection : ActionSummarySectionBase {
        public List<HbmConcentrationModelRecord> Records { get; set; }

        /// <summary>
        /// Summarize only 1) number of residues > 0 and 2) fraction of censored > 0
        /// </summary>
        /// <param name="concentrationModels"></param>
        public void Summarize(
            IDictionary<(HumanMonitoringSamplingMethod Method, Compound Substance), ConcentrationModel> concentrationModels
        ) {
            Records = new List<HbmConcentrationModelRecord>();
            foreach (var concentrationModel in concentrationModels) {
                if (concentrationModel.Value.Residues.NumberOfResidues > 0 && concentrationModel.Value.Residues.FractionCensoredValues > 0) {
                    if (concentrationModel.Value.ModelType == ConcentrationModelType.CensoredLogNormal) {
                        var model = concentrationModel.Value as CMCensoredLogNormal;
                        var mean = Math.Exp(model.Mu + Math.Pow(model.Sigma, 2) / 2);
                        var record = new HbmConcentrationModelRecord() {
                            SamplingMethodCode = concentrationModel.Key.Method.Code,
                            SamplingMethodName = concentrationModel.Key.Method.Name,
                            SubstanceName = concentrationModel.Key.Substance.Name,
                            SubstanceCode = concentrationModel.Key.Substance.Code,
                            Mu = model.Mu,
                            Sigma = model.Sigma,
                            Mean = mean,
                            StandardDeviation = mean * Math.Sqrt(Math.Exp(Math.Pow(model.Sigma, 2)) - 1),
                            TotalMeasurementsCount = model.Residues.NumberOfResidues,
                            Model = model.ModelType,
                            FractionCensored = model.FractionCensored,
                        };
                        Records.Add(record);
                    } else if (concentrationModel.Value.ModelType == ConcentrationModelType.Empirical) {
                        var model = concentrationModel.Value as CMEmpirical;
                        var record = new HbmConcentrationModelRecord() {
                            SamplingMethodCode = concentrationModel.Key.Method.Code,
                            SamplingMethodName = concentrationModel.Key.Method.Name,
                            SubstanceName = concentrationModel.Key.Substance.Name,
                            SubstanceCode = concentrationModel.Key.Substance.Code,
                            Mu = double.NaN,
                            Sigma = double.NaN,
                            Mean = double.NaN,
                            StandardDeviation = double.NaN,
                            TotalMeasurementsCount = model.Residues.NumberOfResidues,
                            Model = model.ModelType,
                            FractionCensored = model.FractionCensored,
                        };
                        Records.Add(record);
                    }
                }
            }
        }
    }
}
