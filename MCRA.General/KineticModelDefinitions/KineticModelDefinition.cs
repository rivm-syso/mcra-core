using System;
using System.Collections.Generic;
using System.Linq;
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
        /// Gets/sets the dll name.
        /// </summary>
        public string DllName { get; set; }

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
        public ICollection<ExposureRouteType> GetExposureRoutes() {
            return Forcings.OrderBy(c => c.Order).Select(c => c.Id).ToList();
        }
    }
}
