using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.ConcentrationModelCalculation.ConcentrationModels;
using MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringAnalysis;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmConcentrationModelsSection : ActionSummaryBase {
        public List<HbmConcentrationModelRecord> Records { get; set; }

        /// <summary>
        /// Summarize only 1) number of residues > 0 and 2) fraction of censored > 0
        /// </summary>
        /// <param name="concentrationModels"></param>
        public void Summarize(
            IDictionary<(HumanMonitoringSamplingMethod, Compound), ConcentrationModel> concentrationModels
        ) {
            Records = new List<HbmConcentrationModelRecord>();
            foreach (var concentrationModel in concentrationModels) {
                if (concentrationModel.Value.Residues.NumberOfResidues > 0 && concentrationModel.Value.Residues.FractionCensoredValues > 0) {
                    if (concentrationModel.Value.ModelType == ConcentrationModelType.CensoredLogNormal) {
                        var model = (concentrationModel.Value as CMCensoredLogNormal);
                        var mean = Math.Exp(model.Mu + Math.Pow(model.Sigma, 2) / 2);
                        var record = new HbmConcentrationModelRecord() {
                            SamplingMethodCode = concentrationModel.Key.Item1.Code,
                            SamplingMethodName = concentrationModel.Key.Item1.Name,
                            SubstanceName = concentrationModel.Key.Item2.Name,
                            SubstanceCode = concentrationModel.Key.Item2.Code,
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
                        var model = (concentrationModel.Value as CMEmpirical);
                        var record = new HbmConcentrationModelRecord() {
                            SamplingMethodCode = concentrationModel.Key.Item1.Code,
                            SamplingMethodName = concentrationModel.Key.Item1.Name,
                            SubstanceName = concentrationModel.Key.Item2.Name,
                            SubstanceCode = concentrationModel.Key.Item2.Code,
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
