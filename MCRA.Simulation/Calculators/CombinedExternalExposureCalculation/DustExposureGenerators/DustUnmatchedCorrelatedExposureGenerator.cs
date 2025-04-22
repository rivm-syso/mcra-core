using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.DustExposureCalculation;
using MCRA.Simulation.Calculators.ExternalExposureCalculation;

namespace MCRA.Simulation.Calculators.CombinedExternalExposureCalculation.DustExposureGenerators {

    public class DustUnmatchedCorrelatedExposureGenerator : DustExposureGenerator {

        /// <summary>
        /// Randomly pair dust and dietary individuals
        /// (if the properties of the dietary individual match the properties of the dust individual)
        /// </summary>
        protected override List<IExternalIndividualDayExposure> generate(
            ICollection<IIndividualDay> individualDays,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            throw new NotImplementedException();
        }
    }
}
