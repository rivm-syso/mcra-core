using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.SoilExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.SoilExposureGenerators {

    public class SoilUnmatchedCorrelatedExposureGenerator : SoilExposureGenerator {

        /// <summary>
        /// Randomly pair soil and dietary individuals
        /// (if the properties of the dietary individual match the properties of the soil individual)
        /// </summary>
        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<SoilIndividualDayExposure> soilIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            throw new NotImplementedException();
        }
    }
}
