using MCRA.Utils.Statistics;
using MCRA.General;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public abstract class FrequencyModel : IFrequencyModel {

        public List<IndividualFrequency> SpecifiedPredictions { get; set; }
        public List<IndividualFrequency> ConditionalPredictions { get; set; }

        public int MinDegreesOfFreedom { get; set; } = 0;
        public int MaxDegreesOfFreedom { get; set; } = 2;
        public CovariateModelType CovariateModel { get; set; } = CovariateModelType.Constant;
        public FunctionType Function { get; set; } = FunctionType.Polynomial;
        public TestingMethodType TestingMethod { get; set; } = TestingMethodType.Backward;
        public double TestingLevel { get; set; } = 0.05;
        public double FixedDispersion { get; set; }

        public abstract FrequencyModelSummary CalculateParameters(
            ICollection<IndividualFrequency> individualFrequencies,
            List<double> predictionLevels
        );

        public abstract (Distribution, CovariateGroup) GetDistribution(
            ICollection<IndividualFrequency> individualFrequencies,
            CovariateGroup targetCovariateGroup
        );

        /// <summary>
        /// Calculates the (model-assisted) Individual Frequencies (blups). This method is called from the base-class when
        /// 'GetIndividualFrequencies()' is called with the IndividualFrequencyType.ModelAssisted argument.
        /// </summary>
        /// <returns></returns>
        protected abstract ICollection<IndividualFrequency> CalculateModelAssistedFrequencies();

        public abstract ConditionalPredictionResults GetConditionalPredictions();

        public ICollection<IndividualFrequency> GetIndividualFrequencies() {
            return CalculateModelAssistedFrequencies();
        }

        public abstract FrequencyModelSummary GetDefaultModelSummary(ErrorMessages errorMessage);

        /// <summary>
        /// Default value when model is skipped
        /// </summary>
        private double defaultPrediction { get; set; }

        /// <summary>
        /// Default value when model is skipped
        /// </summary>
        private double defaultFrequency { get; set; }

        /// <summary>
        /// Determine whether intake is incidental or not
        /// </summary>
        /// <param name="empiricalFrequency"></param>
        /// <returns></returns>
        public IncidentalIntakeType GetIncidentalIntakeType(double empiricalFrequency) {
            var criticalFraction = 0.001;
            if (empiricalFrequency < criticalFraction) {
                return IncidentalIntakeType.Zero;
            } else if (empiricalFrequency >= 1 - criticalFraction) {
                return IncidentalIntakeType.One;
            } else {
                return IncidentalIntakeType.Incidental;
            }
        }

        /// <summary>
        /// Frequency model: Model estimation is skipped
        /// </summary>
        /// <param name="incidentalIntake"></param>
        /// <param name="individualIntakeFrequencies"></param>
        /// <param name="empiricalFrequency"></param>
        /// <returns></returns>
        public FrequencyModelSummary SkippedModelFrequencyCalculator(
            IncidentalIntakeType incidentalIntake, 
            ICollection<IndividualFrequency> individualIntakeFrequencies,
            double empiricalFrequency
        ) {
            defaultPrediction = empiricalFrequency;
            defaultFrequency = (double)individualIntakeFrequencies.Max(c => c.Nbinomial);
            var errorMessage = ErrorMessages.ModelIsSkipped100Frequencies;
            if (incidentalIntake == IncidentalIntakeType.Zero) {
                defaultFrequency = empiricalFrequency;
                errorMessage = ErrorMessages.ModelIsSkipped0Frequencies;
            }
            return GetDefaultModelSummary(errorMessage);
        }

        /// <summary>
        /// Frequency model: all individuals have equal frequencies. Model estimation is skipped
        /// </summary>
        /// <param name="individualIntakeFrequencies"></param>
        /// <returns></returns>
        public FrequencyModelSummary AllEqualFrequencyCalculator(
            ICollection<IndividualFrequency> individualIntakeFrequencies
        ) {
            defaultPrediction = individualIntakeFrequencies.Average(c => (double)c.Frequency / (double)c.Nbinomial);
            defaultFrequency = (double)individualIntakeFrequencies.Max(c => c.Nbinomial);
            var errorMessage = ErrorMessages.ModelIsSkippedEqualFrequencies;
            if (defaultPrediction == 1) {
                defaultPrediction = 0.9999;
                errorMessage = ErrorMessages.ModelIsSkipped100Frequencies;
            }
            return GetDefaultModelSummary(errorMessage);
        }

        /// <summary>
        /// Default when model is skipped
        /// </summary>
        /// <returns></returns>
        public ModelResult GetDefaultModelResult(ErrorMessages errorMessage) {
            return new ModelResult() {
                Dispersion = 0.00001,
                DispersionSe = 0,
                _2LogLikelihood = double.NaN,
                DegreesOfFreedom = 0,
                Estimates = new List<double>() { double.NaN },
                StandardErrors = new List<double>() { double.NaN },
                ErrorMessage = errorMessage,
            };
        }

        /// <summary>
        /// Default when model is skipped
        /// </summary>
        /// <param name="individualIntakeFrequencies"></param>
        /// <returns></returns>
        protected List<IndividualFrequency> createDefaultConditionalPredictions(
            ICollection<IndividualFrequency> individualIntakeFrequencies
        ) {
            return new List<IndividualFrequency>() {
                new IndividualFrequency() {
                        Cofactor = null,
                        Covariable = double.NaN,
                        Nbinomial = individualIntakeFrequencies.Max(c => c.Nbinomial),
                        Frequency = defaultFrequency,
                        Prediction = defaultPrediction,
                        NumberOfIndividuals = individualIntakeFrequencies.Count,
                    }
                };
        }

        /// <summary>
        /// Default when model is skipped
        /// </summary>
        /// <param name="individualIntakeFrequencies"></param>
        /// <returns></returns>
        public List<IndividualFrequency> GetIndividualPredictions(
            ICollection<IndividualFrequency> individualIntakeFrequencies
        ) {
            var nBinomial = individualIntakeFrequencies.Max(c => c.Nbinomial);
            return individualIntakeFrequencies.Select(c => new IndividualFrequency() {
                    Prediction = defaultPrediction,
                    Cofactor = c.Cofactor,
                    Covariable = c.Covariable,
                    NumberOfIndividuals = 1,
                    Nbinomial = nBinomial,
                    Frequency = defaultFrequency,
                    SimulatedIndividualId = c.SimulatedIndividualId
                })
                .ToList();
        }

        protected bool getCovariateGroup(CovariateGroup targetCovariateGroup, IndividualFrequency f) {
            double? covar = double.IsNaN(f.Covariable) ? null : f.Covariable;
            double? targetCovar = double.IsNaN(targetCovariateGroup.Covariable) ? null : targetCovariateGroup.Covariable;
            return (f.Cofactor == targetCovariateGroup.Cofactor && covar == targetCovar)
                || (f.Cofactor == targetCovariateGroup.Cofactor && covar == null && targetCovar != null)
                || (f.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == targetCovar)
                || (f.Cofactor == null && targetCovariateGroup.Cofactor != null && covar == null && targetCovar != null);
        }
    }
}
