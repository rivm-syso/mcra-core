using System.Xml.Serialization;

namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.DeSolve {

    [Serializable]
    [XmlType("PbkModelSpecification")]
    public class DeSolvePbkModelSpecification : UnitValueDefinition, IPbkModelSpecification {

        /// <summary>
        /// Identifier of the main model (a definition is a specific version of this main model).
        /// </summary>
        public string IdModel { get; set; }

        /// <summary>
        /// The version of this definition.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets/sets the PBK model type.
        /// </summary>
        public PbkModelType Format { get; set; }

        /// <summary>
        /// Gets/sets the file name and extension of the underlying PBK model implementation.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The kinetic model resolution.
        /// </summary>
        public TimeUnit Resolution { get; set; }

        /// <summary>
        /// The evaluation frequency per resolution.
        /// </summary>
        public int EvaluationFrequency { get; set; }

        /// <summary>
        /// The integrator to use, either a function that performs integration, or a list
        /// of class rkMethod. The default is 'lsoda'.
        /// </summary>
        public string IdIntegrator { get; set; }

        /// <summary>
        /// Forcing parameters of the kinetic model.
        /// </summary>
        [XmlArrayItem("ModelSubstance")]
        public List<DeSolvePbkModelSubstanceSpecification> ModelSubstances { get; set; }

        /// <summary>
        /// Forcing parameters of the PBK model.
        /// </summary>
        [XmlArrayItem("Forcing")]
        public List<DeSolvePbkModelForcingSpecification> Forcings { get; set; }

        /// <summary>
        /// Input parameters of the PBK model.
        /// </summary>
        [XmlArrayItem("Parameter")]
        public List<DeSolvePbkModelParameterSpecification> Parameters { get; set; }

        /// <summary>
        /// State parameters of the PBK model.
        /// </summary>
        [XmlArrayItem("State")]
        public List<DeSolvePbkModelStateSpecification> States { get; set; }

        /// <summary>
        /// Output parameters of the PBK model.
        /// </summary>
        [XmlArrayItem("Output")]
        public List<DeSolvePbkModelOutputSpecification> Outputs { get; set; }

        /// <summary>
        /// Retrieves the list of substances defined within the PBK model.
        /// </summary>
        public List<IPbkModelSubstanceSpecification> GetModelSubstances() {
            return ModelSubstances
                .Cast<IPbkModelSubstanceSpecification>()
                .ToList();
        }

        /// <summary>
        /// Retrieves the list of outputs defined in the PBK model.
        /// </summary>
        public List<IPbkModelOutputSpecification> GetOutputs() {
            return Outputs
                .Cast<IPbkModelOutputSpecification>()
                .ToList();
        }

        /// <summary>
        /// Returns the biological matrices for which this PBK model provides outputs.
        /// </summary>
        /// <returns></returns>
        public List<BiologicalMatrix> GetOutputMatrices() {
            return Outputs
                .Select(r => r.BiologicalMatrix)
                .Where(r => r != BiologicalMatrix.Undefined)
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// Retrieves the list of states defined within the PBK model.
        /// </summary>
        public List<DeSolvePbkModelStateSpecification> GetStates() {
            var result = States
                .OrderBy(r => r.Order)
                .ToList();
            return result;
        }

        /// <summary>
        /// Retrieves the list of parameters defined within the PBK model.
        /// </summary>
        public List<IPbkModelParameterSpecification> GetParameters() {
            return Parameters.Cast<IPbkModelParameterSpecification>().ToList();
        }

        /// <summary>
        /// Returns true if this is a lifetime model, i.e. it has an (external) initial age
        /// parameter and an (internal/variable) age parameter.
        /// </summary>
        public bool IsLifetimeModel() {
            return Parameters.Any(p => p.Type == PbkModelParameterType.Age && p.IsInternalParameter)
                && Parameters.Any(p => p.Type == PbkModelParameterType.AgeInit && !p.IsInternalParameter);
        }

        /// <summary>
        /// Gets a parameter definition by its type.
        /// </summary>
        public IPbkModelParameterSpecification GetParameterDefinitionByType(PbkModelParameterType paramType) {
            return Parameters.FirstOrDefault(p => p.Type == paramType);
        }
    }
}
