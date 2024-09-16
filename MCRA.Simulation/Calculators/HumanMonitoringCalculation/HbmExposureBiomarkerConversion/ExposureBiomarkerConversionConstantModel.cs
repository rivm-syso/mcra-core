using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public class ExposureBiomarkerConversionConstantModel : IExposureBiomarkerConversionModel {
        public bool UseSubgroups { get; set; }
        public ExposureBiomarkerConversion ConversionRule { get; protected set; }
        protected List<IKineticConversionFactorModelParametrisation> ModelParametrisations { get; set; } = [];

        public ExposureBiomarkerConversionConstantModel(
            ExposureBiomarkerConversion conversion,
            bool useSubgroups
        ) {
            ConversionRule = conversion;
            UseSubgroups = useSubgroups;
        }

        public double Draw(IRandom random, double? age, GenderType gender) {
            var candidates = ModelParametrisations
                .Where(r => r.Gender == gender || r.Gender == GenderType.Undefined)
                .Where(r => !r.Age.HasValue || (age.HasValue && age.Value >= r.Age.Value))
                .OrderByDescending(r => r.Gender)
                .ThenByDescending(r => r.Age)
                .ToList();
            var parametrisation = candidates.FirstOrDefault();
            return drawFunction(parametrisation, random);
        }

        protected virtual double drawFunction(IKineticConversionFactorModelParametrisation param, IRandom random) {
            return param.Factor;
        }

        public void CalculateParameters() {
            // subclasses use parameter checks on upper values
            // check whether the instance is the base constant model class
            // so we can skip these checks
            var isConstantModel = GetType() == typeof(ExposureBiomarkerConversionConstantModel);

            // First, check whether to use subgroups and if subgroups are available and use individual properties as
            // keys for lookup
            if (UseSubgroups) {
                foreach (var sg in ConversionRule.EBCSubgroups) {
                    if (!isConstantModel) {
                        checkSubGroupUncertaintyValue(sg);
                    }
                    if (isConstantModel || !ModelParametrisations.Any(r => r.Age == sg.AgeLower && r.Gender == sg.Gender)) {
                        var parametrisation = getParametrisation(sg.ConversionFactor, sg.VariabilityUpper.Value, sg.Gender, sg.AgeLower);
                        ModelParametrisations.Add(parametrisation);
                    }
                }
            }
            // This is the default, no individual properties are needed.
            if (!ModelParametrisations.Any(r => r.Age == null && r.Gender == GenderType.Undefined)) {
                if (!isConstantModel && !ConversionRule.VariabilityUpper.HasValue) {
                    throw new Exception($"Missing uncertainty upper value for exposure biomarker conversion factor {ConversionRule.IdExposureBiomarkerConversion}");
                }
                var parametrisation = getParametrisation(ConversionRule.ConversionFactor, ConversionRule.VariabilityUpper ?? 0D);
                ModelParametrisations.Add(parametrisation);
            }
        }

        protected virtual IKineticConversionFactorModelParametrisation getParametrisation(
            double conversionFactor,
            double variabilityFactor,
            GenderType gender = GenderType.Undefined,
            double? age = null
        ) {
            return new KineticConversionFactorModelParametrisation {
                Age = age,
                Gender = gender,
                Factor = conversionFactor
            };
        }

        protected static void checkSubGroupUncertaintyValue(ExposureBiomarkerConversionSG sg) {
            if (!sg.VariabilityUpper.HasValue) {
                var sgStrings = new List<string>();
                if (sg.AgeLower.HasValue) {
                    sgStrings.Add(sg.AgeLower.Value.ToString());
                }
                if (sg.Gender != GenderType.Undefined) {
                    sgStrings.Add(sg.Gender.ToString());
                }
                var sgString = string.Join(",", sgStrings);
                throw new Exception($"Missing variability upper value for subgroup [{sgString}] of exposure biomarker conversion factor [{sg.IdExposureBiomarkerConversion}].");
            }
        }
    }
}
