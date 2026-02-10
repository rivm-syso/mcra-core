namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml {

    /// <summary>
    /// Validate PBK model definition for compartments, species
    /// </summary>
    /// <param name="nodes"></param>
    public class SbmlPbkModelException(string message)
        : Exception($"Validation of PBK model failed. {message}") {
    }
}
