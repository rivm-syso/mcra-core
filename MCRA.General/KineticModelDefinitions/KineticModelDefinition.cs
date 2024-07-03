using System.Xml.Serialization;

namespace MCRA.General {

    [XmlRoot("KineticModelDefinition")]
    [Serializable]
    public class KineticModelDefinition : UnitValueDefinition {

        /// <summary>
        /// Identifier of the main model (a definition is a specific version
        /// of this main model).
        /// </summary>
        public string IdModel { get; set; }

        /// <summary>
        /// The version of this definition.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets/sets the PBK model type.
        /// </summary>
        public KineticModelType Format { get; set; }

        /// <summary>
        /// Gets/sets the file name and extension of the underlying kinetic model engine.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The id of the parameter associated with the body weight.
        /// </summary>
        public string IdBodyWeightParameter { get; set; }

        /// <summary>
        /// The id of the parameter associated with the body surface area.
        /// </summary>
        public string IdBodySurfaceAreaParameter { get; set; }

        /// <summary>
        /// The id of the parameter associated with the age.
        /// </summary>
        public string IdAgeParameter { get; set; }

        /// <summary>
        /// The id of the parameter associated with the gender.
        /// </summary>
        public string IdGenderParameter { get; set; }

        /// <summary>
        /// The kinetic model resolution.
        /// </summary>
        public string Resolution { get; set; }

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
        [XmlArrayItem("KineticModelSubstance")]
        public List<KineticModelSubstanceDefinition> KineticModelSubstances { get; set; }

        /// <summary>
        /// Forcing parameters of the kinetic model.
        /// </summary>
        [XmlArrayItem("Forcing")]
        public List<KineticModelInputDefinition> Forcings { get; set; }

        /// <summary>
        /// Input parameters of the kinetic model.
        /// </summary>
        [XmlArrayItem("Parameter")]
        public List<KineticModelParameterDefinition> Parameters { get; set; }

        /// <summary>
        /// State parameters of the kinetic model.
        /// </summary>
        [XmlArrayItem("State")]
        public List<KineticModelStateVariableDefinition> States { get; set; }

        /// <summary>
        /// Output parameters of the kinetic model.
        /// </summary>
        [XmlArrayItem("Output")]
        public List<KineticModelOutputDefinition> Outputs { get; set; }

        /// <summary>
        /// Returns the exposure routes of the kinetic model definition (ordered by
        /// the order of the forcings).
        /// </summary>
        /// <returns></returns>
        public ICollection<ExposurePathType> GetExposureRoutes() {
            return Forcings.OrderBy(c => c.Order).Select(c => c.Route).ToList();
        }

        /// <summary>
        /// Returns the input definition for the exposure path type.
        /// </summary>
        /// <param name="exposurePathType"></param>
        /// <returns></returns>
        public KineticModelInputDefinition GetInputByPathType(ExposurePathType exposurePathType) {
            var route = exposurePathType.GetExposureRoute();
            var input = Forcings.FirstOrDefault(r => r.Route == exposurePathType);
            if (input == null) {
                // Fall back to oral if dietary path is missing
                input = Forcings.FirstOrDefault(r => r.Route == ExposurePathType.Oral);
            }
            return input;
        }

        /// <summary>
        /// The time scale considered by the model.
        /// </summary>
        public TimeUnit TimeScale {
            get {
                return TimeUnitConverter.FromString(Resolution);
            }
        }
    }
}
