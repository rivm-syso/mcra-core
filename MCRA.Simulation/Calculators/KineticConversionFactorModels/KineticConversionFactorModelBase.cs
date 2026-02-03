using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticConversionFactorModels {

    public interface IKineticConversionFactorModelParametrisation {
        double? Age { get; set; }
        GenderType Gender { get; set; }
        double Factor { get; set; }
    }

    public interface IKineticConversionFactorDataModel : IKineticConversionFactorModel {
        List<IKineticConversionFactorModelParametrisation> GetParametrisations();
        public KineticConversionFactor ConversionRule { get; }
    }

    public class KineticConversionFactorModelParametrisation : IKineticConversionFactorModelParametrisation {
        public double? Age { get; set; }
        public GenderType Gender { get; set; }
        public double Factor { get; set; }
    }

    public abstract class KineticConversionFactorModelBase<T> : IKineticConversionFactorDataModel
        where T : KineticConversionFactorModelParametrisation {

        public bool UseSubgroups { get; set; }

        protected List<T> ModelParametrisations { get; set; } = [];

        public KineticConversionFactor ConversionRule { get; protected set; }

        public Compound SubstanceTo => ConversionRule.SubstanceTo;

        public Compound SubstanceFrom => ConversionRule.SubstanceFrom;

        public ExposureUnitTriple UnitTo => ConversionRule.DoseUnitTo;

        public ExposureUnitTriple UnitFrom => ConversionRule.DoseUnitFrom;

        public ExposureTarget TargetFrom => ConversionRule.TargetFrom;

        public ExposureTarget TargetTo => ConversionRule.TargetTo;

        public KineticConversionFactorModelBase(
            KineticConversionFactor conversion,
            bool useSubgroups
        ) {
            ConversionRule = conversion;
            UseSubgroups = useSubgroups;
        }

        public void CalculateParameters() {
            // First, check whether to use subgroups and if subgroups are available
            // and use individual properties as keys for lookup
            if (UseSubgroups && ConversionRule.KCFSubgroups.Any()) {
                foreach (var sg in ConversionRule.KCFSubgroups) {
                    checkSubGroupUncertaintyValue(sg);
                    var parametrisation = getSubgroupParametrisation(
                        sg.AgeLower,
                        sg.Gender,
                        sg.ConversionFactor,
                        sg.UncertaintyUpper
                    );
                    if (!ModelParametrisations.Any(r => r.Age == sg.AgeLower && r.Gender == sg.Gender)) {
                        ModelParametrisations.Add(parametrisation);
                    }
                }
            }

            //This is the default, no individual properties are needed.
            if (!ModelParametrisations.Any(r => r.Age == null && r.Gender == GenderType.Undefined)) {
                var parametrisation = getSubgroupParametrisation(
                    null,
                    GenderType.Undefined,
                    ConversionRule.ConversionFactor,
                    ConversionRule.UncertaintyUpper
                );
                ModelParametrisations.Add(parametrisation);
            }
        }

        public virtual void ResampleModelParameters(IRandom random) {
            // Default no action / no uncertainty
        }

        /// <summary>
        /// Returns the conversion factor for the specified age and sex.
        /// </summary>
        public double GetConversionFactor(
            double? age,
            GenderType gender
        ) {
            var candidates = ModelParametrisations
                .Where(r => r.Gender == gender || r.Gender == GenderType.Undefined)
                .Where(r => !r.Age.HasValue || age.HasValue && age.Value >= r.Age.Value)
                .OrderByDescending(r => r.Gender)
                .ThenByDescending(r => r.Age)
                .ToList();
            var parametrisation = candidates.FirstOrDefault();
            return parametrisation.Factor;
        }

        /// <summary>
        /// Returns the model parameterisations (and the currently drawn factors).
        /// </summary>
        /// <returns></returns>
        public List<IKineticConversionFactorModelParametrisation> GetParametrisations() {
            var result = ModelParametrisations
                .Cast<IKineticConversionFactorModelParametrisation>()
                .ToList();
            return result;
        }

        /// <summary>
        /// Get the parametrisation for the specified sub-group based on the provided
        /// factor and upper.
        /// </summary>
        protected abstract T getSubgroupParametrisation(
            double? age,
            GenderType gender,
            double factor,
            double? upper
        );

        protected virtual void checkSubGroupUncertaintyValue(KineticConversionFactorSG sg) {
            // No checks
        }

        public bool HasCovariate(KineticConversionFactorCovariateType covariateType) {
            if (!UseSubgroups) {
                return false;
            } else {
                switch (covariateType) {
                    case KineticConversionFactorCovariateType.Age:
                        return ConversionRule.KCFSubgroups.Any(c => c.AgeLower != null);
                    case KineticConversionFactorCovariateType.Sex:
                        return ConversionRule.KCFSubgroups.Any(c => c.Gender != GenderType.Undefined);
                    default:
                        return false;
                }
            }
        }
    }
}
