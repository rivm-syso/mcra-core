namespace MCRA.General {

    public static class IndividualPropertyTypeExtensions {

        /// <summary>
        /// Returns the property type.
        /// </summary>
        /// <param name="individualPropertytype"></param>
        /// <returns></returns>
        public static PropertyType GetPropertyType(this IndividualPropertyType individualPropertytype) {
            switch (individualPropertytype) {
                case IndividualPropertyType.Categorical:
                case IndividualPropertyType.Gender:
                case IndividualPropertyType.Month:
                case IndividualPropertyType.Boolean:
                case IndividualPropertyType.Location:
                case IndividualPropertyType.Isced:
                case IndividualPropertyType.JobTask:
                    return PropertyType.Cofactor;
                case IndividualPropertyType.Integer:
                case IndividualPropertyType.Nonnegative:
                case IndividualPropertyType.NonnegativeInteger:
                case IndividualPropertyType.Numeric:
                    return PropertyType.Covariable;
                case IndividualPropertyType.DateTime:
                    return PropertyType.DateTime;
                default:
                    throw new Exception($"Failed to extract property type!");
            }
        }
    }
}
