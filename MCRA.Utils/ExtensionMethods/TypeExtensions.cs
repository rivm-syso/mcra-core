namespace MCRA.Utils.ExtensionMethods {
    public static class TypeExtensions {
        public static bool IsNumeric(this Type varType) {
            return varType == typeof(double)
                || varType == typeof(decimal)
                || varType == typeof(float)
                || varType == typeof(int)
                || varType == typeof(double?)
                || varType == typeof(decimal?)
                || varType == typeof(float?)
                || varType == typeof(int?);
        }
    }
}
