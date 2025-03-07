using MCRA.General;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DustExposuresSection : ActionSummarySectionBase {

        public List<DustExposuresDataRecord> DustExposuresDataRecords { get; set; }

        public int TotalIndividuals { get; set; }

        public void Summarize(
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures
        ) {
            var records = dustIndividualDayExposures
                .AsParallel()
                .SelectMany(
                    r => r.ExposuresPerPath
                        .SelectMany(
                            eprs => eprs.Value,
                            (epsr, ipc) => (
                                r.SimulatedIndividual.Id,
                                r.SimulatedIndividual.SamplingWeight,
                                Route: epsr.Key.Route,
                                Substance: ipc.Compound,
                                ipc.Amount
                            )
                        )
                )
                .GroupBy(r => (r.Route, r.Substance))
                .Select(g => new DustExposuresDataRecord {
                    SubstanceName = g.Key.Substance.Name,
                    SubstanceCode = g.Key.Substance.Code,
                    ExposureRoute = g.Key.Route.GetShortDisplayName(),
                    MeanExposure = g.Average(r => r.Amount)
                })
                .ToList();

            DustExposuresDataRecords = records;

            TotalIndividuals = dustIndividualDayExposures
                .Select(r => r.SimulatedIndividual.Id)
                .Distinct()
                .Count();
        }

        public void SummarizeUncertainty(
            ICollection<DustIndividualDayExposure> individualDustExposures,
            double lowerBound,
            double upperBound
        ) {
            /*
            individualDustExposures
                .GroupBy(r => (r.Substance, r.ExposureRoute))
                    .ForAll(g => {
                        var record = DustExposuresDataRecords
                            .Where(r => r.SubstanceCode == g.Key.Substance.Code)
                            .SingleOrDefault();
                        if (record != null) {
                            var meanExposure = g.Average(r => r.Exposure);
                            record.DustUncertaintyValues.Add(meanExposure);
                        }
                    });
            */
        }
    }
}
