namespace MCRA.Simulation.Calculators.IntakeModelling {
    public class UsualIntakeResult {
        public List<ISUFUsualIntake> UsualIntakes { get; set; }
        public List<IsufModelDiagnostics> Diagnostics { get; set; }
        public List<double> ConsumersOnly { get; set; }
    }
}
