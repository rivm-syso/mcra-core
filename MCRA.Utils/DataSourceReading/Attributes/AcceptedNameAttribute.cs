using System;

namespace MCRA.Utils.DataSourceReading.Attributes {

    /// <summary>
    /// Represents an attribute that can be used to map object properties
    /// to csv table columns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class AcceptedNameAttribute : Attribute {

        /// <summary>
        /// Maps the annotated property to the column with the specified name.
        /// </summary>
        public string AcceptedName { get; private set; }

        /// <summary>
        /// Field name alias.
        /// </summary>
        /// <param name="columnName"></param>
        public AcceptedNameAttribute(string columnName) {
            AcceptedName = columnName;
        }
    }
}
