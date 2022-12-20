using System;

namespace MCRA.Utils.DataSourceReading.Attributes {

    /// <summary>
    /// Represents an attribute that can be used to ignore properties
    /// when mapping the object to csv table columns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class IgnoreFieldAttribute : Attribute {
    }
}
