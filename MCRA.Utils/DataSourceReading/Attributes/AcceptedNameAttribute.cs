namespace MCRA.Utils.DataSourceReading.Attributes {

    /// <summary>
    /// Represents an attribute that can be used to map object properties
    /// to csv table columns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class AcceptedNameAttribute : Attribute {

        /// <summary>
        /// Case invariant.
        /// </summary>
        public bool CaseInvariant { get; set; }

        /// <summary>
        /// Maps the annotated property to the column with the specified name.
        /// </summary>
        public string AcceptedName { get; private set; }

        /// <summary>
        /// Field name alias.
        /// </summary>
        /// <param name="fieldName"></param>
        public AcceptedNameAttribute(string fieldName) {
            AcceptedName = fieldName;
        }

        /// <summary>
        /// Checks whether the provided name complies to the rule of the attribute.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Validate(string name) {
            if (CaseInvariant) {
                return string.Equals(AcceptedName, name, StringComparison.OrdinalIgnoreCase);
            } else {
                return string.Equals(AcceptedName, name);
            }
        }
    }
}
