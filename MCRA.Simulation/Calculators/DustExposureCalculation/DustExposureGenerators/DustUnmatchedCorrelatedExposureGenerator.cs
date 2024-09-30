using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.DustExposureCalculation {

    public class DustUnmatchedCorrelatedExposureGenerator : DustExposureGenerator {
                     
        /// <summary>
        /// Randomly pair dust and dietary individuals 
        /// (if the properties of the dietary individual match the properties of the dust individual)
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
            
            /*
                if (checkIndividualMatchesDustExposures(individual, dustIndividualDayExposures)) {
                generator.Reset();
                var randomIndividualCode = _dustIndividualCodes.ElementAt(generator.Next(0, _dustIndividualCodes.Count));
                if (exposureSets.TryGetValue(randomIndividualCode, out var exposureSet) && exposureSet != null) {
                    nonDietaryExposures.AddRange(nonDietaryIntakePerCompound(exposureSet, nonDietarySurvey, individual, substances));
                }                    
            } 
            */
            return dustExposures;
        }
    }
}
