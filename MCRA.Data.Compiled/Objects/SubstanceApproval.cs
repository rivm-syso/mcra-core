namespace MCRA.Data.Compiled.Objects {

    /// <summary>
    /// Substance approval, in accordance with Regulation (EC) No 1107/2009.
    /// </summary>
    public sealed class SubstanceApproval{

        public Compound Substance { get; set; }

        /// <summary>
        /// Specifies whether the substance is approved or not. Substances not included are by default not approved.
        /// </summary>
        public bool IsApproved { get; set; }
    }
}
