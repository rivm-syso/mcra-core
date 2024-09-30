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
        /// <param name="individual"></param>
        /// <param name="dustIndividualDayExposures"></param>
        /// <param name="substances"></param>
        /// <param name="generator"></param>
        /// <returns></returns>
        protected override List<DustIndividualDayExposure> createDustIndividualExposure(
            IIndividualDay individual,
            ICollection<DustIndividualDayExposure> dustIndividualDayExposures,
            ICollection<Compound> substances,
            IRandom generator
        ) {
            var dustExposures = new List<DustIndividualDayExposure>();
            var individualSet = dustIndividualDayExposures
                .Select(r => r.SimulatedIndividualId)
                .Distinct()
                .ToList();
            var ix = generator.Next(0, individualSet.Count);
            var selectedDustExposures = dustIndividualDayExposures
                .Where(r => r.SimulatedIndividualId == individualSet.ElementAt(ix));
            foreach (var selectedDustExposure in selectedDustExposures) { 
                selectedDustExposure.Individual = individual.Individual;
                dustExposures.Add(selectedDustExposure);
            }            
            return dustExposures;
        }
    }
}
