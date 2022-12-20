using System;

namespace MCRA.Utils.DataSourceReading.Attributes {

    /// <summary>
    /// Represents an attribute that can be used to specify whether properties
    /// are required or not when mapping the object to csv table columns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class RequiredFieldAttribute : Attribute {
        public RequiredFieldAttribute() { }
    }
}
