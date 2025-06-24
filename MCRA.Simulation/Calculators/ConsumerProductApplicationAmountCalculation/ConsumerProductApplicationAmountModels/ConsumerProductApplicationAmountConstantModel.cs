using System.ComponentModel;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion.ExposureBiomarkerConversionModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.ConsumerProductApplicationAmountCalculation {
    public class ConsumerProductApplicationAmountConstantModel(
        ConsumerProductApplicationAmountSGs applicationAmount
        ) : IConsumerProductApplicationAmountModel {

        public ConsumerProductApplicationAmountSGs ApplicationAmount { get; protected set; } = applicationAmount;

        protected List<ConsumerProductApplicationAmountModelParametrisation> ModelParametrisations { get; set; } = [];

        public double Draw(IRandom random, double? age, GenderType gender) {
            var candidates = ModelParametrisations
                .Where(r => r.Gender == gender || r.Gender == GenderType.Undefined)
                .Where(r => !r.Age.HasValue || age.HasValue && age.Value >= r.Age.Value)
                .OrderByDescending(r => r.Gender)
                .ThenByDescending(r => r.Age)
                .ToList();
            var parametrisation = candidates.FirstOrDefault();
            if (parametrisation == null) {
                var properties = new List<string>();
                if (age.HasValue) {
                    properties.Add($"age [{age.Value}]");
                }
                if (gender != GenderType.Undefined) {
                    properties.Add($"sex [{gender}]");
                }
                var propertyDescription = string.Join(" and ", properties);
                throw new Exception($"Missing application amount for {ApplicationAmount.Product.Name} [{ApplicationAmount.Product.Code}] for individual with {propertyDescription}.");
            }
            return drawFunction(parametrisation, random);
        }

        protected virtual double drawFunction(
            ConsumerProductApplicationAmountModelParametrisation param,
            IRandom random
        ) {
            return param.Amount;
        }

        public void CalculateParameters() {
            var isConstantModel = GetType() == typeof(ConsumerProductApplicationAmountConstantModel);

            // First, add/initialise sub-group parametrisations
            foreach (var sg in ApplicationAmount.CPAASubgroups) {
                if (!ModelParametrisations.Any(r => r.Age == sg.AgeLower && r.Gender == sg.Sex)) {
                    if (!isConstantModel && !sg.CvVariability.HasValue) {
                        throw new Exception($"Missing amount or CV for combination of individual properties and {ApplicationAmount.Product.Code}");
                    }
                    var parametrisation = getParametrisation(sg.Amount, sg.CvVariability ?? double.NaN, sg.Sex, sg.AgeLower);
                    ModelParametrisations.Add(parametrisation);
                }
            }

            // This is the default, non-specific, record. I.e., no individual properties.
            if (ApplicationAmount.Amount.HasValue) {
                if (!isConstantModel && !ApplicationAmount.CvVariability.HasValue) {
                    throw new Exception($"Missing amount or CV for combination of individual properties and {ApplicationAmount.Product.Code}");
                }
                var parametrisation = getParametrisation(ApplicationAmount.Amount.Value, ApplicationAmount.CvVariability ?? double.NaN);
                ModelParametrisations.Add(parametrisation);
            }
        }

        protected virtual ConsumerProductApplicationAmountModelParametrisation getParametrisation(
            double amount,
            double cv,
            GenderType gender = GenderType.Undefined,
            double? age = null
        ) {
            return new ConsumerProductApplicationAmountModelParametrisation {
                Age = age,
                Gender = gender,
                Amount = amount,
            };
        }
    }
}
