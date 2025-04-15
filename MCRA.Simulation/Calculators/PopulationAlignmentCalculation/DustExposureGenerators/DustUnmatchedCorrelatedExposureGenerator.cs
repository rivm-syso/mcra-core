using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Calculators.DustExposureCalculation;

namespace MCRA.Simulation.Calculators.PopulationAlignmentCalculation.DustExposureGenerators {

    public class DustUnmatchedCorrelatedExposureGenerator : DustExposureGenerator {

        /// <summary>
        /// Randomly pair dust and dietary individuals
        /// (if the properties of the dietary individual match the properties of the dust individual)
        /// </summary>
        protected override DustIndividualDayExposure createDustIndividualExposure(
            IIndividualDay individualDay,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            throw new NotImplementedException();
        }
    }
}
