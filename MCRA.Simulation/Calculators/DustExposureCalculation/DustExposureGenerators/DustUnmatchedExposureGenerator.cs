using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {

    public class DustUnmatchedExposureGenerator : DustExposureGenerator {

        /// <summary>
        /// No correlation between individuals in different  nondietary surveys
        ///  Randomly pair non-dietary and dietary individuals, no correlation between nondietary individuals
        /// (if the properties of the individual match the covariates of the non-dietary survey)
        /// </summary>
        protected override DustIndividualDayExposure createDustIndividualExposure(
            IIndividualDay individualDay,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var ix = generator.Next(0, dustIndividualDayExposures.Count);
            var selected = dustIndividualDayExposures.ElementAt(ix);
            var result = selected.Clone();
            result.SimulatedIndividualId = individualDay.SimulatedIndividualId;
            result.SimulatedIndividualDayId = individualDay.SimulatedIndividualDayId;
            result.IndividualSamplingWeight = individualDay.IndividualSamplingWeight;
            result.Individual = individualDay.Individual;
            result.Day = individualDay.Day;
            return result;
        }
    }
}
