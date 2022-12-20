namespace MCRA.Utils.DataSourceReading.ValueConversion {
    public interface IValueConverter {

        /// <summary>
        /// Converts the string value to the type of the value converter.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        object Convert(string value);
    }
}
