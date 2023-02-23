using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.KarrerKineticModelCalculation {

    public sealed class KarrerReImplementedKineticModelCalculator : PbpkModelCalculator {

        public KarrerReImplementedKineticModelCalculator(
            KineticModelInstance kineticModelInstance,
            IDictionary<ExposureRouteType, double> defaultAbsorptionFactors
        ) : base(kineticModelInstance, defaultAbsorptionFactors) {
        }

        protected override IDictionary<string, double> drawParameters(IDictionary<string, KineticModelInstanceParameter> parameters, IRandom random, bool IsNominal = false, bool useParameterVariability = false) {
            var result = base.drawParameters(parameters, random, IsNominal, _kineticModelInstance.UseParameterVariability);
            return result;
        }

        /// <summary>
        /// In the Karrer PBKP model the oral dose is applied in the first 3 minutes (3/60 = 0.05 hour) so for 1 dose a day during 3 minutes it is X /  0.05
        /// Dermal doses are applied for 24 hours, so for 1 dose a day during 24 hours it is X / 24
        /// MCRA supplies for dietary daily doses
        /// Duration (in hours) 3 minutes = 0.05 hour: X / (3/60) = X / 0.05
        /// Duration (in hours) 24 hours: X / (1440/60) = X / 24
        /// For multiple dosings: X / NumberOfDosings /duration. Note duration is 24/NumberOfDosings
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="doses"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        protected override List<double> getUnitDoses(IDictionary<string, KineticModelInstanceParameter> parameters, List<double> doses, ExposureRouteType route) {
            var result = new List<double>();
            switch (route) {
                case ExposureRouteType.Dietary:
                    doses.ForEach(c => result.Add(c / _kineticModelInstance.NumberOfDosesPerDay));
                    break;
                case ExposureRouteType.Oral:
                    //  is also dermal for Karrer model, based on PCPs;
                    doses.ForEach(c => result.Add(c / _kineticModelInstance.NumberOfDosesPerDay));
                    break;
                case ExposureRouteType.Dermal:
                    // is dermal for Karrer model, based on Thermal Paper;
                    doses.ForEach(c => result.Add(c / _kineticModelInstance.NumberOfDosesPerDayNonDietaryDermal));
                    break;
                case ExposureRouteType.Inhalation:
                    doses.ForEach(c => result.Add(c / _kineticModelInstance.NumberOfDosesPerDayNonDietaryInhalation));
                    break;
                default:
                    throw new Exception("Route not recognized");
            }
            return result;
        }

        protected override double getAge(IDictionary<string, double> parameters, Individual individual, string ageProperty) {
            var property = individual?.IndividualPropertyValues
                .Where(c => c.IndividualProperty.Code.Equals(ageProperty, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? null;
            if (property != null) {
                return (double)property.DoubleValue;
            }
            var age = individual?.Covariable ?? 60;
            return double.IsNaN(age) ? 60 : age;
        }

        protected override double getGender(IDictionary<string, double> parameters, Individual individual, string genderProperty) {
            var property = individual?.IndividualPropertyValues
                .Where(c => c.IndividualProperty.Code.Equals(genderProperty, StringComparison.OrdinalIgnoreCase)).FirstOrDefault() ?? null;
            if (property != null) {
                return property.TextValue.Equals(GenderType.Male.GetDisplayName(), StringComparison.OrdinalIgnoreCase) ? 1d : 0d;
            }
            var gender = individual?.Cofactor ?? GenderType.Male.GetDisplayName();
            gender = !string.IsNullOrEmpty(gender) ? gender : GenderType.Male.GetDisplayName();
            return gender == GenderType.Male.GetDisplayName() ? 1d : 0d;
        }

        /// <summary>
        /// In Karrer model not state variables are update but local parameters, 
        /// therefore doses for time points that are not events need to set back to zero, so implement all events for every timepoint.
        /// </summary>
        /// <param name="eventsDictionary"></param>
        /// <returns></returns>
        protected override List<int> calculateEvents(IDictionary<ExposureRouteType, List<int>> eventsDictionary) {
            var endEvaluationPeriod = _kineticModelInstance.NumberOfDays * getTimeUnitMultiplier(_kineticModelInstance.ResolutionType) - 1;
            return Enumerable.Range(0, endEvaluationPeriod).ToList();
        }

        protected override double getRelativeCompartmentWeight(KineticModelOutputDefinition outputParameter, IDictionary<string, double> parameters) {
            return 1;
        }
    }
}
