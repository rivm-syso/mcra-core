using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.AirExposureGenerators {

    public class AirUnmatchedExposureGenerator : AirExposureGenerator {

        /// <summary>
        /// Randomly pair dust and reference individuals
        /// </summary>
        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<AirIndividualExposure> airIndividualExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var ix = generator.Next(0, airIndividualExposures.Count);
            var selected = airIndividualExposures.ElementAt(ix);
            var results = new List<IExternalIndividualDayExposure>();
            foreach (var individualDay in individualDays) {
                var result = createExternalIndividualDayExposure(individualDay, selected);
                results.Add(result);
            }
            return results;
        }
    }
}
