namespace MCRA.Utils.DataSourceReading.Attributes {

    /// <summary>
    /// Represents an attribute that can be used to map object properties
    /// to table columns identified by a specific index.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ColumnIndexAttribute : Attribute {

        /// <summary>
        /// Maps the annotated property to the column with the specified index.
        /// </summary>
        public int ColumnIndex { get; private set; } = -1;

        /// <summary>
        /// Maps the annotated property to the column with the specified index.
        /// </summary>
        public ColumnIndexAttribute(int columnIndex) {
            ColumnIndex = columnIndex;
        }
    }
}
