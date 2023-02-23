namespace MCRA.Utils.ExtensionMethods {
    public static class MatrixExtensionMethods {

        /// <summary>
        /// Returns true if the two-dimensional array (matrix) of type T
        /// equals the other two-dimensional array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="matrix"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool MatrixEquals<T>(this T[,] matrix, T[,] other) {
            var equals = matrix.Rank == other.Rank
                && Enumerable.Range(0, matrix.Rank).All(dimension => matrix.GetLength(dimension) == other.GetLength(dimension))
                && matrix.Cast<T>().SequenceEqual(other.Cast<T>());
            return equals;
        }
    }
}
