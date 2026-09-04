using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.ConsumerProductExposureFractionCalculation {
    public class ConsumerProductExposureFractionResolver {

        public ConsumerProductExposureFractionSGs ExposureFraction { get; protected set; }

        protected List<ConsumerProductExposureFractionRule> AvailableModels { get; set; } = [];

        public ConsumerProductExposureFractionResolver(ConsumerProductExposureFractionSGs exposureFraction) {
            ExposureFraction = exposureFraction;
            init();
        }

        public double GetExposureFraction(double? age, GenderType gender) {
            var candidates = AvailableModels
                .Where(r => r.Gender == gender || r.Gender == GenderType.Undefined)
                .Where(r => !r.Age.HasValue || age.HasValue && age.Value >= r.Age.Value)
                .OrderByDescending(r => r.Gender)
                .ThenByDescending(r => r.Age)
                .ToList();
            var efModel = candidates.FirstOrDefault();
            if (efModel == null) {
                var properties = new List<string>();
                if (age.HasValue) {
                    properties.Add($"age [{age.Value}]");
                }
                if (gender != GenderType.Undefined) {
                    properties.Add($"sex [{gender}]");
                }
                var propertyDescription = string.Join(" and ", properties);
                throw new Exception($"Missing exposure fraction for {ExposureFraction.Product.Name} [{ExposureFraction.Product.Code}] " +
                    $"for {ExposureFraction.Substance.Name} [{ExposureFraction.Substance.Code}], " +
                    $"for {ExposureFraction.Route.GetDisplayName()} " +
                    $"for individual with {propertyDescription}.");
            }
            return efModel.ExposureFraction;
        }

        private void init() {
            // First, add sub-group exposure fractions
            foreach (var sg in ExposureFraction.CPEFSubgroups) {
                if (!AvailableModels.Any(r => r.Age == sg.AgeLower && r.Gender == sg.Sex)) {
                    var efModel = getModel(sg.ExposureFraction, sg.Sex, sg.AgeLower);
                    AvailableModels.Add(efModel);
                }
            }
            // This is the default, non-specific, record. I.e., no individual properties.
            if (ExposureFraction.ExposureFraction.HasValue) {
                var efModel = getModel(ExposureFraction.ExposureFraction.Value, GenderType.Undefined, null);
                AvailableModels.Add(efModel);
            }
        }

        private static ConsumerProductExposureFractionRule getModel(
            double fraction,
            GenderType gender = GenderType.Undefined,
            double? age = null
        ) {
            return new ConsumerProductExposureFractionRule {
                Age = age,
                Gender = gender,
                ExposureFraction = fraction,
            };
        }
    }
}
