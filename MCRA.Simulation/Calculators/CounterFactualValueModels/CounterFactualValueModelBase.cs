using MCRA.Data.Compiled.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.CounterFactualValueModels {

    public class CounterFactualValueModelParametrisation : ICounterFactualValueModelParametrisation {
        public double Factor { get; set; }
    }

    public abstract class CounterFactualValueModelBase<T> : ICounterFactualValueModel
        where T : CounterFactualValueModelParametrisation {


        protected List<T> ModelParametrisations { get; set; } = [];

        public ExposureResponseFunction ExposureResponseFunction { get; protected set; }

        public CounterFactualValueModelBase(
            ExposureResponseFunction erf
        ) {
            ExposureResponseFunction = erf;
        }

        public void CalculateParameters() {
            var parametrisation = getParametrisation(
                ExposureResponseFunction.CounterfactualValue,
                ExposureResponseFunction.CFVUncertaintyUpper
            );
            ModelParametrisations.Add(parametrisation);
        }

        public virtual void ResampleModelParameters(IRandom random) {
            // Default no action / no uncertainty
        }

        /// <summary>
        /// Returns the conversion factor for the specified age and sex.
        /// </summary>
        public double GetCounterFactualValue(
        ) {
            var candidates = ModelParametrisations
                .ToList();
            var parametrisation = candidates.FirstOrDefault();
            return parametrisation.Factor;
        }


        /// <summary>
        /// Returns the model parameterisations (and the currently drawn factors).
        /// </summary>
        public List<ICounterFactualValueModelParametrisation> GetParametrisations() {
            var result = ModelParametrisations
                .Cast<ICounterFactualValueModelParametrisation>()
                .ToList();
            return result;
        }

        /// <summary>
        /// Get the parametrisation 
        /// </summary>
        protected abstract T getParametrisation(
            double factor,
            double? upper
        );
    }
}
