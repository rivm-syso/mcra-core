using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes result for ISUF model
    /// </summary>
    public sealed class ISUFModelResultsSection : SummarySection {

        public List<double> DiscreteFrequencies { get; set; }
        public List<IsufModelDiagnostics> ISUFDiagnostics { get; set; }
        //TODO: move these properties to a subsection "ISUFDescriptionSection" oid
        public double VarianceBetweenUnit { get; set; }
        public double VarianceWithinUnit { get; set; }
        public double PMA4 { get; set; }
        public double MA4 { get; set; }
        public int NumberOfKnots { get; set; }
        public double AndersonDarling { get; set; }
        public double ErrorRate { get; set; }
        public double Power { get; set; }

        public void SummarizeModel(ISUFModel isufModel) {
            this.DiscreteFrequencies = isufModel.FrequencyResult.DiscreteFrequencies.ToList();
            this.VarianceBetweenUnit = isufModel.TransformationResult.VarianceBetweenUnit;
            this.VarianceWithinUnit = isufModel.TransformationResult.VarianceWithinUnit;
            this.MA4 = isufModel.TransformationResult.MA4;
            this.PMA4 = isufModel.TransformationResult.PMA4;
            this.NumberOfKnots = isufModel.TransformationResult.NumberOfKnots;
            this.AndersonDarling = isufModel.TransformationResult.AndersonDarlingResults.ADStatistic;
            this.ErrorRate = isufModel.TransformationResult.AndersonDarlingResults.ErrorRate;
            //TODO dit heeft niet veel zijn maar laat het nog maar even staan
            //this.ISUFDiagnosticsSection.SummarizeDiagnostics(isufModel.ISUFDiagnostics);
            this.ISUFDiagnostics = isufModel.UsualIntakeResult.Diagnostics.ToList();
            this.Power = isufModel.TransformationResult.Power;
        }
    }
}
