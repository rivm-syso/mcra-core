using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation {

    public sealed class CosmosKineticModelCalculator : DesolvePbkModelCalculator {

        public CosmosKineticModelCalculator(
            KineticModelInstance kineticModelInstance,
            IDictionary<ExposurePathType, double> defaultAbsorptionFactors
        ) : base(kineticModelInstance, defaultAbsorptionFactors) {
            initializePartitionCoefficientCorrelations(kineticModelInstance);
        }

        protected override IDictionary<string, double> drawParameters(IDictionary<string, KineticModelInstanceParameter> parameters, IRandom random, bool isNominal = false, bool useParameterVariability = false) {
            var PCFatOriginal = parameters["PCFat"].Value;
            var result = base.drawParameters(parameters, random, isNominal, useParameterVariability);
            result["ResampledPCFat"] = result["PCFat"];
            result["PCFat"] = PCFatOriginal;
            return result;
        }

        /// <summary>
        /// In the nominal run the correlations for partition coefficients are set:
        /// log_aPoor = log(PCPoor/PCFat) idem for log_aRich, log_aLiver, log_aSkin, log_aSikn_sc
        /// In the uncertainty run this initialisation is skipped.
        /// In the dll based on the correlations, partition coefficients for PCPoor, PCRich, PCLiver, PCSkin and PCSkin_sc are calculated as follows:
        /// PCPoor = exp(log_aPoor)*ResampledPCFat (reverse calculation) etc
        /// </summary>
        /// <param name="kineticModelInstance"></param>
        /// <returns></returns>
        private void initializePartitionCoefficientCorrelations(KineticModelInstance kineticModelInstance) {
            if (!kineticModelInstance.KineticModelInstanceParameters.TryGetValue("log_aPoor", out var parameter)) {
                parameter = new KineticModelInstanceParameter() {
                    Parameter = "log_aPoor",
                    Value = Math.Log(kineticModelInstance.KineticModelInstanceParameters["PCPoor"].Value / kineticModelInstance.KineticModelInstanceParameters["PCFat"].Value),
                };
                kineticModelInstance.KineticModelInstanceParameters["log_aPoor"] = parameter;

                parameter = new KineticModelInstanceParameter() {
                    Parameter = "log_aRich",
                    Value = Math.Log(kineticModelInstance.KineticModelInstanceParameters["PCRich"].Value / kineticModelInstance.KineticModelInstanceParameters["PCFat"].Value),
                };
                kineticModelInstance.KineticModelInstanceParameters["log_aRich"] = parameter;

                parameter = new KineticModelInstanceParameter() {
                    Parameter = "log_aLiver",
                    Value = Math.Log(kineticModelInstance.KineticModelInstanceParameters["PCLiver"].Value / kineticModelInstance.KineticModelInstanceParameters["PCFat"].Value),
                };
                kineticModelInstance.KineticModelInstanceParameters["log_aLiver"] = parameter;

                parameter = new KineticModelInstanceParameter() {
                    Parameter = "log_aSkin",
                    Value = Math.Log(kineticModelInstance.KineticModelInstanceParameters["PCSkin"].Value / kineticModelInstance.KineticModelInstanceParameters["PCFat"].Value),
                };
                kineticModelInstance.KineticModelInstanceParameters["log_aSkin"] = parameter;

                parameter = new KineticModelInstanceParameter() {
                    Parameter = "log_aSkin_sc",
                    Value = Math.Log(kineticModelInstance.KineticModelInstanceParameters["PCSkin_sc"].Value / kineticModelInstance.KineticModelInstanceParameters["PCFat"].Value),
                };
                kineticModelInstance.KineticModelInstanceParameters["log_aSkin_sc"] = parameter;
            }
        }

        protected override double getRelativeCompartmentWeight(KineticModelOutputDefinition parameter, IDictionary<string, double> parameters) {
            if (parameter.Id == "CSkin_sc_u") {
                return parameters["BSA"] * parameters["Height_sc"] * (1 - parameters["fSA_exposed"]);
            } else if (parameter.Id == "CSkin_u") {
                return parameters["BSA"] * parameters["Height_vs"] * (1 - parameters["fSA_exposed"]);
            } else if (parameter.Id == "CPoor") {
                throw new Exception("MCRA dose not implement compartment Muscle tissues");
            } else {
                var factor = 1D;
                foreach (var scalingFactor in parameter.ScalingFactors) {
                    factor *= parameters[scalingFactor];
                }
                foreach (var multiplicationFactor in parameter.MultiplicationFactors) {
                    factor *= multiplicationFactor;
                }
                return factor;
            }
        }
    }
}
