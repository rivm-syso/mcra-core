using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ComponentCalculation.ExposureMatrixCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils;

namespace MCRA.Simulation.Actions.ExposureMixtures {
    public sealed class ExposureMixturesActionResult : IActionResult {
        public List<ComponentRecord> ComponentRecords { get; set; }
        public GeneralMatrix UMatrix { get; set; }
        public IndividualMatrix IndividualComponentMatrix { get; set; }
        public List<Compound> Substances { get; set; }
        public int NumberOfDays { get; set; }
        public int NumberOfSelectedDays { get; set; }
        public double TotalExposureCutOffPercentile { get; set; }
        public List<double> RMSE { get; set; }
        public ExposureMatrix ExposureMatrix { get; set; }
        public double[,] GlassoSelect { get; set; }
        public Dictionary<Compound, string> SubstanceSamplingMethods { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
