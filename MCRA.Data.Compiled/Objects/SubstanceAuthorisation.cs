namespace MCRA.Data.Compiled.Objects {

    /// <summary>
    /// Substance authorisation
    /// </summary>
    public sealed class SubstanceAuthorisation {

        public string Reference { get; set; }

        public Food Food { get; set; }
        public Compound Substance { get; set; }
    }
}
