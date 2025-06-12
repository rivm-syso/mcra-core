using MCRA.General;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductExposuresSection : ActionSummarySectionBase {

        public List<ConsumerProductExposuresDataRecord> ConsumerProductExposuresDataRecords { get; set; }

        public int TotalIndividuals { get; set; }

        public void Summarize(
            ICollection<ConsumerProductIndividualIntake> cpIndividualDayExposures
        ) {
            var records = cpIndividualDayExposures
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
                .Select(g => new ConsumerProductExposuresDataRecord {
                    SubstanceName = g.Key.Substance.Name,
                    SubstanceCode = g.Key.Substance.Code,
                    ExposureRoute = g.Key.Route.GetShortDisplayName(),
                    MeanExposure = g.Average(r => r.Amount)
                })
                .ToList();

            ConsumerProductExposuresDataRecords = records;

            TotalIndividuals = cpIndividualDayExposures
                .Select(r => r.SimulatedIndividual.Id)
                .Distinct()
                .Count();
        }

        public void SummarizeUncertainty(
            ICollection<ConsumerProductIndividualIntake> individualConsumerProductExposures,
            double lowerBound,
            double upperBound
        ) {
            // TODO
        }
    }
}
