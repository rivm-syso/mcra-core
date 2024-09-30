using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.MatchIndividualExposures {
    public class MatchIndividualExposure {       
        
        public static List<Individual> GetReferenceIndividuals(
             ActionData data,
             ExposureSource referenceExposureSource) 
            {
            var referenceIndividuals = new List<Individual>();
            if (referenceExposureSource == ExposureSource.DietaryExposures) {
                referenceIndividuals = data.DietaryIndividualDayIntakes
                    .Select(r => r.Individual)
                    .Distinct()
                    .ToList();
            } else if (referenceExposureSource == ExposureSource.OtherNonDietary) {
                throw new NotImplementedException();
                //referenceIndividuals = data.NonDietaryExposures.Values.Cast<Individual>().ToList();

            } else if (referenceExposureSource == ExposureSource.DustExposures) {
                referenceIndividuals = data.IndividualDustExposures
                    .Select(r => r.Individual)
                    .Distinct()
                    .ToList();
            }

            return referenceIndividuals;
        }        
    }
}

