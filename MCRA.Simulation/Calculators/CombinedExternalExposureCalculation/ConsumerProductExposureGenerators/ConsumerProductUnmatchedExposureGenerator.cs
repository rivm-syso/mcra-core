using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.ConsumerProductExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.ConsumerProductExposureGenerators {

    public class ConsumerProductUnmatchedExposureGenerator : ConsumerProductExposureGenerator {

        /// <summary>
        /// Randomly pair consumer product and reference individuals
        /// </summary>
        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<ConsumerProductIndividualExposure> cpIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var ix = generator.Next(0, cpIndividualDayExposures.Count);
            var selected = cpIndividualDayExposures.ElementAt(ix);
            var results = new List<IExternalIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = createExternalIndividualDayExposure(individualDay, selected);
                results.Add(result);
            }
            return results;
        }
    }
}
