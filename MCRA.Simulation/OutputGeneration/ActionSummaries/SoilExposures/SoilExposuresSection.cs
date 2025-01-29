using MCRA.General;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SoilExposuresSection : ActionSummarySectionBase {

        public List<SoilExposuresDataRecord> SoilExposuresDataRecords { get; set; }

        public int TotalIndividuals { get; set; }

        public void Summarize(
            ICollection<SoilIndividualDayExposure> dustIndividualDayExposures
        ) {
            var records = dustIndividualDayExposures
                .AsParallel()
                .SelectMany(
                    r => r.ExposurePerSubstanceRoute
                        .SelectMany(
                            eprs => eprs.Value,
                            (epsr, ipc) => (
                                r.SimulatedIndividualId,
                                r.IndividualSamplingWeight,
                                Route: epsr.Key,
                                Substance: ipc.Compound,
                                ipc.Amount
                            )
                        )
                )
                .GroupBy(r => (r.Route, r.Substance))
                .Select(g => new SoilExposuresDataRecord {
                    SubstanceName = g.Key.Substance.Name,
                    SubstanceCode = g.Key.Substance.Code,
                    ExposureRoute = g.Key.Route.GetShortDisplayName(),
                    MeanExposure = g.Average(r => r.Amount)
                })
                .ToList();

            SoilExposuresDataRecords = records;

            TotalIndividuals = dustIndividualDayExposures
                .Select(r => r.SimulatedIndividualId)
                .Distinct()
                .Count();
        }

        public void SummarizeUncertainty(
            ICollection<SoilIndividualDayExposure> individualSoilExposures,
            double lowerBound,
            double upperBound
        ) {
            /*
            individualSoilExposures
                .GroupBy(r => (r.Substance, r.ExposureRoute))
                    .ForAll(g => {
                        var record = SoilExposuresDataRecords
                            .Where(r => r.SubstanceCode == g.Key.Substance.Code)
                            .SingleOrDefault();
                        if (record != null) {
                            var meanExposure = g.Average(r => r.Exposure);
                            record.SoilUncertaintyValues.Add(meanExposure);
                        }
                    });
            */
        }
    }
}
