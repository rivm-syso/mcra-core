using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductExposureSetSummarySection : SummarySection {

        public List<ConsumerProductExposureSetRecord> Records { get; set; }

        public void Summarize(
            ICollection<ConsumerProductExposureAssessment> exposureAssessments
        ) {
            Records = exposureAssessments
                .SelectMany(c => c.ExposureSets, (cpA, cpS) => (cpA, cpS))
                .Select(c => {
                    var allZeros = c.cpS.ExposurePercentiles.All(c => c.Value == 0);
                    var minimum = allZeros
                            ? string.Empty
                            : $"p{c.cpS.ExposurePercentiles.Min(p => p.Percentage)} = {c.cpS.ExposurePercentiles.Min(p => p.Value):0.###E+00}";
                    var maximum = allZeros
                            ? string.Empty
                            : $"p{c.cpS.ExposurePercentiles.Max(p => p.Percentage)} = {c.cpS.ExposurePercentiles.Max(p => p.Value):0.###E+00}";
                    return new ConsumerProductExposureSetRecord() {
                        ExposureSetCode = c.cpS.IdExposureSet,
                        Name = c.cpA.Name,
                        Code = c.cpA.IdAssessment,
                        NumberOfPercentiles = c.cpS.ExposurePercentiles.Count,
                        Minimum = minimum,
                        Maximum = maximum,
                        SubstanceName = c.cpS.Substance.Name,
                        SubstanceCode = c.cpS.Substance.Code,
                        Products = string.Join(", ", c.cpS.Products.Select(p => p.Name)),
                        Routes = string.Join(", ",   c.cpS.ExposureRoutes.Select(r => r.GetShortDisplayName())),
                        PercentageOfZeros = c.cpS.PercentageZeros ?? 0,
                        Unit = c.cpS.ConsumerProductExposureEstimateUnit.GetShortDisplayName(),
                    };
                })
                .ToList();
        }
    }
}
