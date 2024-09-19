using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustExposuresSection : ActionSummarySectionBase {

        public List<DustExposuresDataRecord> DustExposuresDataRecords { get; set; }

        public void Summarize(
            ICollection<IndividualDustExposureRecord> individualDustExposures,
            ICollection<Compound> substances) {
            DustExposuresDataRecords = individualDustExposures
                .GroupBy(r => (r.Substance, r.ExposureRoute))
                .Where(g => substances.Contains(g.Key.Substance))
                .Select(g => new DustExposuresDataRecord {
                    CompoundName = g.Key.Substance.Name,
                    CompoundCode = g.Key.Substance.Code,
                    TotalIndividuals = g.Count(),
                    ExposureRoute = g.Key.ExposureRoute.GetShortDisplayName(),
                    MeanExposure = g.Average(r => r.Exposure)
                })
            .ToList();
        }

        public void SummarizeUncertainty(
            ICollection<IndividualDustExposureRecord> individualDustExposures,
            double lowerBound,
            double upperBound
        ) {
            individualDustExposures
                .GroupBy(r => (r.Substance, r.ExposureRoute))
                    .ForAll(g => {
                        var record = DustExposuresDataRecords
                            .Where(r => r.CompoundCode == g.Key.Substance.Code)
                            .SingleOrDefault();
                        if (record != null) {
                            var meanExposure = g.Average(r => r.Exposure);
                            record.DustUncertaintyValues.Add(meanExposure);
                        }
                    });
        }
    }
}
