using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public interface IIntakeDistributionSection {
        List<HistogramBin> IntakeDistributionBins { get; }
        int TotalNumberOfIntakes { get; }
        double PercentageZeroIntake { get; }
        //ReferenceDoseRecord Reference { get; }
        UncertainDataPointCollection<double> Percentiles { get; }
    }
}
