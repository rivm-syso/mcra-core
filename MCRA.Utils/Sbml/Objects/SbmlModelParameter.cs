namespace MCRA.Utils.Sbml.Objects {
    public class SbmlModelParameter : SbmlModelElement {
        public string Units { get; set; }
        public double DefaultValue { get; set; }
        public bool IsConstant { get; set; }
    }
}
