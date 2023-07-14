using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MCRA.Utils.ExtensionMethods {
    public static class DataTableExtensions {

        /// <summary>
        /// Writes the datatable to the specified csv file.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="fileName"></param>
        /// <param name="delimiter"></param>
        /// <param name="useQuotes"></param>
        /// <param name="append"></param>
        /// <param name="orderBy"></param>
        public static void ToCsv(
            this DataTable table,
            string fileName,
            string delimiter = ",",
            bool useQuotes = true,
            bool append = false,
            IEnumerable<string> orderBy = null
        ) {
            var sb = new StringBuilder();
            var tableItems = new DataRow[table.Rows.Count];

            if (!append) {
                var columnNames = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName);
                sb.AppendLine(string.Join(delimiter, columnNames));
            }

            //if ordering is requested, create an ordered view of the datatable and
            //populate the tableItems from that
            if (orderBy?.Any() ?? false) {
                var view = table.DefaultView;
                view.Sort = string.Join(", ", orderBy);
                var rowIndex = 0;
                foreach (DataRowView rowView in view) {
                    tableItems[rowIndex++] = rowView.Row;
                }
            } else {
                for (var i = 0; i < table.Rows.Count; i++) {
                    tableItems[i] = table.Rows[i];
                }
            }

            foreach (DataRow row in tableItems) {
                var fields = row.ItemArray.Select(field => field.toCsvFieldString(useQuotes));
                sb.AppendLine(string.Join(delimiter, fields));
            }
            if (append) {
                File.AppendAllText(fileName, sb.ToString());
            } else {
                File.WriteAllText(fileName, sb.ToString());
            }
        }


        private static string toCsvFieldString(this object field, bool useQuotes = true) {
            string s;
            if (field.GetType() == typeof(double) || field.GetType() == typeof(float)) {
                s = ((double)field).ToString(CultureInfo.InvariantCulture);
            } else if (field.GetType() == typeof(double?)) {
                s = field != null ? ((double)field).ToString(CultureInfo.InvariantCulture) : string.Empty;
            } else if (field.GetType() == typeof(int)) {
                s = field.ToString();
            } else if (field.GetType() == typeof(int?)) {
                s = field != null ? ((int)field).ToString() : string.Empty;
            } else if (field.GetType() == typeof(bool)) {
                s = field.ToString();
            } else if (field.GetType() == typeof(bool?)) {
                s = field != null ? ((bool)field).ToString() : string.Empty;
            } else if (field.GetType() == typeof(byte)) {
                s = field.ToString();
            } else if (field.GetType() == typeof(byte?)) {
                s = field != null ? ((byte)field).ToString() : string.Empty;
            } else if (field.GetType() == typeof(DateTime)) {
                s = ((DateTime)field).ToString("yyyy-MM-dd HH:mm:ss");
            } else {
                //Replace all whitespace with single space, this includes
                //line endings, tabs and multiple consecutive spaces
                var data = Regex.Replace(field.ToString(), @"\s+", " ");
                if (useQuotes) {
                    s = string.Concat("\"", data.Replace("\"", "\"\""), "\"");
                } else {
                    s = data.Replace("\"", "\"\"");
                }
            }
            return s;
        }

        /// <summary>
        /// Creates a data table from the list of generics that can be used for bulkcopying to
        /// the target database.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="records"></param>
        /// <param name="tableName"></param>
        /// <param name="enumsAsString">If true, enum fields are stored as strings in the data tables.</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(
            this IEnumerable<T> records, 
            string tableName = null, 
            bool enumsAsString = true
        ) {
            var properties = typeof(T).GetProperties();
            var dataTable = new DataTable(tableName);
            foreach (var p in properties) {
                if (enumsAsString && p.PropertyType.IsEnum) {
                    dataTable.Columns.Add(p.Name, typeof(string));
                } else {
                    dataTable.Columns.Add(p.Name, Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType);
                }
            }
            foreach (var record in records) {
                DataRow row = dataTable.NewRow();
                for (int i = 0; i < dataTable.Columns.Count; i++) {
                    var value = properties[i].GetValue(record, null);
                    if (enumsAsString && properties[i].PropertyType.IsEnum) {
                        row[i] = value.ToString();
                    } else if (value != null) {
                        row[i] = value;
                    } else {
                        row[i] = DBNull.Value;
                    }
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

        /// <summary>
        /// Writes the data table to a nicely formatted string.
        /// </summary>
        /// <param name="dataTable"></param>
        /// <returns></returns>
        public static string ConvertDataTableToString(this DataTable dataTable) {
            var output = new StringBuilder();

            var columnsWidths = new int[dataTable.Columns.Count];

            // Get column widths
            foreach (DataRow row in dataTable.Rows) {
                for (int i = 0; i < dataTable.Columns.Count; i++) {
                    var length = row[i].ToString().Length;
                    if (columnsWidths[i] < length) {
                        columnsWidths[i] = length;
                    }
                }
            }

            // Get Column Titles
            for (int i = 0; i < dataTable.Columns.Count; i++) {
                var length = dataTable.Columns[i].ColumnName.Length;
                if (columnsWidths[i] < length) {
                    columnsWidths[i] = length;
                }
            }

            // Write Column titles
            for (int i = 0; i < dataTable.Columns.Count; i++) {
                var text = dataTable.Columns[i].ColumnName;
                output.Append("|" + PadCenter(text, columnsWidths[i] + 2));
            }
            output.Append("|\n" + new string('=', output.Length) + "\n");

            // Write Rows
            foreach (DataRow row in dataTable.Rows) {
                for (int i = 0; i < dataTable.Columns.Count; i++) {
                    //Replace all whitespace with single space, this includes
                    //line endings, tabs and multiple consecutive spaces
                    var text = Regex.Replace(row[i].ToString(), @"\s+", " ");
                    output.Append("|" + PadCenter(text, columnsWidths[i] + 2));
                }
                output.Append("|\n");
            }
            return output.ToString();
        }

        private static string PadCenter(string text, int maxLength) {
            int diff = maxLength - text.Length;
            return new string(' ', diff / 2) + text + new string(' ', (int)(diff / 2.0 + 0.5));
        }
    }
}
