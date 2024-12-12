namespace MCRA.General.KineticModelDefinitions.SbmlPbkUtils {

    /// <summary>
    /// Validate PBK model definition for compartments, species
    /// </summary>
    /// <param name="nodes"></param>
    public class PbkModelException(string message)
        : Exception($"Validation of PBK model failed. {message}") {
    }
}
