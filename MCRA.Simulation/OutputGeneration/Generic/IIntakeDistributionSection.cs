using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;

namespace MCRA.Simulation.OutputGeneration {
    public interface IIntakeDistributionSection {
        List<HistogramBin> IntakeDistributionBins { get; }
        double PercentageZeroIntake { get; }
        UncertainDataPointCollection<double> Percentiles { get; }
    }
}
