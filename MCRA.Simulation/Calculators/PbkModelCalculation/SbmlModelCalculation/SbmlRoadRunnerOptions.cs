namespace MCRA.Simulation.Calculators.PbkModelCalculation.SbmlModelCalculation {
    /// <summary>Configuration for SBML RoadRunner simulations.</summary>
    public sealed class SbmlRoadRunnerOptions {
        public const string SectionName = "SbmlRoadRunner";
        /// <summary>
        /// Worker count as a positive integer, or an automatic policy: "Maximum" or "Balanced".
        /// Balanced uses 60% of the available processors.
        /// </summary>
        public string WorkerCount { get; set; } = "1";
        public RoadRunnerWorkerProxyType ProxyType { get; set; } = RoadRunnerWorkerProxyType.InProcess;
        public bool ShowConsoleWindow { get; set; }
        public bool ExtensiveConsoleLogging { get; set; }

        /// <summary>Gets the process count selected for this machine and configuration.</summary>
        public int GetEffectiveWorkerCount() {
            var processorCount = Math.Max(1, Environment.ProcessorCount);
            if (string.Equals(WorkerCount, "Maximum", StringComparison.OrdinalIgnoreCase)) {
                return processorCount;
            }
            if (string.Equals(WorkerCount, "Balanced", StringComparison.OrdinalIgnoreCase)) {
                return Math.Max(1, (int)Math.Ceiling(processorCount * 0.6));
            }
            if (int.TryParse(WorkerCount, out var workerCount) && workerCount >= 1) {
                return workerCount;
            }
            throw new InvalidOperationException(
                $"SbmlRoadRunner:WorkerCount must be 'Maximum', 'Balanced', or a positive integer; received '{WorkerCount}'.");
        }
    }
}
