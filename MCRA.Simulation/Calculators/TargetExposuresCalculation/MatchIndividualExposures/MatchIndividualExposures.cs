using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.MatchIndividualExposures {
    public class MatchIndividualExposure {

        public static List<Individual> GetReferenceIndividuals(
             ActionData data,
             ExposureSource referenceExposureSource)
            {
            var referenceIndividuals = new List<Individual>();
            if (referenceExposureSource == ExposureSource.Diet) {
                referenceIndividuals = data.DietaryIndividualDayIntakes
                    .Select(r => r.Individual)
                    .Distinct()
                    .ToList();
            } else if (referenceExposureSource == ExposureSource.OtherNonDiet) {
                throw new NotImplementedException();
            } else if (referenceExposureSource == ExposureSource.Dust) {
                referenceIndividuals = data.IndividualDustExposures
                    .Select(r => r.Individual)
                    .Distinct()
                    .ToList();
            }

            return referenceIndividuals;
        }
    }
}

