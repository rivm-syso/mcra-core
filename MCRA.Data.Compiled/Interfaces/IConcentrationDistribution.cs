using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Data.Compiled.Interfaces {
    public interface IConcentrationDistribution {
        string IdDistribution { get; }
        Compound Substance { get; }
        double Mean { get; }
        double? CvVariability { get; }
        double? OccurrencePercentage { get; }
    }
}
