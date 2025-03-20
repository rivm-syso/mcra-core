using MCRA.General;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AirExposuresSection : ActionSummarySectionBase {

        public List<AirExposuresDataRecord> AirExposuresDataRecords { get; set; }

        public int TotalIndividuals { get; set; }

        public void Summarize(
            ICollection<AirIndividualDayExposure> airIndividualDayExposures
        ) {
            var records = airIndividualDayExposures
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
                .Select(g => new AirExposuresDataRecord {
                    SubstanceName = g.Key.Substance.Name,
                    SubstanceCode = g.Key.Substance.Code,
                    ExposureRoute = g.Key.Route.GetShortDisplayName(),
                    MeanExposure = g.Average(r => r.Amount)
                })
                .ToList();

            AirExposuresDataRecords = records;

            TotalIndividuals = airIndividualDayExposures
                .Select(r => r.SimulatedIndividual.Id)
                .Distinct()
                .Count();
        }

        public void SummarizeUncertainty(
            ICollection<AirIndividualDayExposure> individualAirExposures,
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
