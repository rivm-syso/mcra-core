using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.AirExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.AirExposureGenerators {

    public class AirUnmatchedCorrelatedExposureGenerator : AirExposureGenerator {

        /// <summary>
        /// Randomly pair air and dietary individuals
        /// (if the properties of the dietary individual match the properties of the air individual)
        /// </summary>
        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<AirIndividualDayExposure> airIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            throw new NotImplementedException();
        }
    }
}
