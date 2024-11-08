namespace MCRA.Utils.DataFileReading {
    /// <summary>
    /// Foreign key reference.
    /// </summary>
    public sealed class ForeignKeyReference {

        /// <summary>
        /// Name of referred table.
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// Name of referred field in the referred table.
        /// </summary>
        public string Field { get; set; }

    }
}
