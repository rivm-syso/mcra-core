using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion {
    public abstract class ExposureBiomarkerConversionModelBase {
        public bool UseSubgroups { get; set; }
        public ExposureBiomarkerConversion ConversionRule { get; protected set; }
        protected List<IKineticConversionFactorModelParametrisation> ModelParametrisations { get; set; } = new();
        public ExposureBiomarkerConversionModelBase(
            ExposureBiomarkerConversion conversion,
            bool useSubgroups
        ) {
            ConversionRule = conversion; 
            UseSubgroups = useSubgroups;
        }

        public abstract void CalculateParameters();
        public abstract double Draw(IRandom random, double? age, GenderType gender);
        protected double drawForParametrisation(
            IRandom random,
            double? age,
            GenderType gender,
            Func<IKineticConversionFactorModelParametrisation, IRandom, double> drawFunction
        ) {
            var candidates = ModelParametrisations
                .Where(r => r.Gender == gender || r.Gender == GenderType.Undefined)
                .Where(r => !r.Age.HasValue || (age.HasValue && age.Value >= r.Age.Value))
                .OrderByDescending(r => r.Gender)
                .ThenByDescending(r => r.Age)
                .ToList();
            var parametrisation = candidates.FirstOrDefault();
            return drawFunction(parametrisation, random);
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
