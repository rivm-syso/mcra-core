using System.Data;
using System.Globalization;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;

namespace MCRA.Utils.DataFileReading {
    /// <summary>
    /// Data source writer that writes the data sources to csv files
    /// in a specified csv folder.
    /// </summary>
    public class SpreadsheetDataSourceWriter : IDataSourceWriter, IDisposable {
        /// <summary>
        /// Directory to which the data files will be written.
        /// </summary>
        private readonly string _targetFileName;
        protected SpreadsheetDocument _spreadsheetDocument;
        protected uint _sheetIndex = 1;
        /// <summary>
        /// Constructor, must provide temp folder for writing the csv files.
        /// </summary>
        /// <param name="csvDirectory"></param>
        public SpreadsheetDataSourceWriter(string fileName) {
            _targetFileName = fileName;
            if (File.Exists(_targetFileName)) {
                File.Delete(_targetFileName);
            }
        }

        /// <summary>
        /// Closes the writer.
        /// </summary>
        public virtual void Close() {
            _spreadsheetDocument.Save();
            _spreadsheetDocument.Dispose();
            _spreadsheetDocument = null;
        }

        /// <summary>
        /// Opens the writer.
        /// </summary>
        public virtual void Open() {
            _spreadsheetDocument ??= CreateSpreadsheetDocument(_targetFileName);
        }

        /// <summary>
        /// Write the data to a worksheet
        /// </summary>
        /// <param name="table"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="tableDefinition"></param>
        public void Write(
            DataTable table,
            string destinationTableName,
            TableDefinition tableDefinition
        ) {
            Open();
            WriteSpreadsheetTable(_spreadsheetDocument, _sheetIndex++, tableDefinition.TableName, table);
        }

        /// <summary>
        /// Writes the source table to the destination table.
        /// </summary>
        /// <param name="sourceTableReader"></param>
        /// <param name="tableDefinition"></param>
        /// <param name="destinationTableName"></param>
        /// <param name="progressState"></param>
        public void Write(
            IDataReader sourceTableReader,
            TableDefinition tableDefinition,
            string destinationTableName,
            ProgressState progressState = null
        ) {
            try {
                // Create in memory data table based on table definition
                var destinationTable = tableDefinition.CreateDataTable();
                // Set name of destination table to preferred alias
                destinationTableName = tableDefinition.TableName;
                // Get column mappings
                var mappings = tableDefinition.ColumnDefinitions.GetColumnMappings(sourceTableReader.GetColumnNames());

                // Write row-by-row using the column mappings
                while (sourceTableReader.Read()) {
                    var newRow = destinationTable.NewRow();
                    for (int i = 0; i < tableDefinition.ColumnDefinitions.Count; i++) {
                        if (mappings[i] > -1) {
                            newRow[i] = sourceTableReader.IsDBNull(mappings[i])
                                ? DBNull.Value
                                : sourceTableReader.GetValue(mappings[i]);
                        }
                    }
                    destinationTable.Rows.Add(newRow);
                }

                // Write to output
                Write(destinationTable, destinationTableName, tableDefinition);

                sourceTableReader.Close();
            } catch (Exception ex) {
                throw new Exception($"An error occured in table '{destinationTableName}': {ex}");
            }
        }

        private static Stylesheet createStyleSheet() {
            var fonts = new Fonts(
                new Font( // 0 - default
                    new FontSize { Val = 11 }
                ),
                new Font( // 1 - header
                    new FontSize { Val = 11 },
                    new Bold(),
                    new Color() { Rgb = "FFFFFF" }
                )
            );

            var fills = new Fills(
                new Fill(new PatternFill() { PatternType = PatternValues.None }),     // 0 - default
                new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }),  // 1 - default fill of gray 125 (required)
                new Fill(                                                             // 2 - header
                    new PatternFill(
                        new ForegroundColor { Rgb = new HexBinaryValue() { Value = "2F75B5" } }
                    ) { PatternType = PatternValues.Solid }
                )
            );

            var borders = new Borders(
                new Border(   // 0 - default
                    new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new DiagonalBorder()
                ),
                new Border(   // 1 - black border
                    new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new DiagonalBorder()
                )
            );

            var cellFormats = new CellFormats(
                new CellFormat { // 0 - default
                    FontId = 0,
                    FillId = 0,
                    BorderId = 0,
                    ApplyFill = true
                },
                new CellFormat { // 1 - table format header
                    FontId = 1,
                    FillId = 2,
                    BorderId = 1,
                    ApplyFill = true
                },
                new CellFormat { // 2 - date format
                    FontId = 0,
                    FillId = 0,
                    BorderId = 0,
                    ApplyFill = true,
                    NumberFormatId = 55, //d-M-yyyy
                    ApplyNumberFormat = true
                }
            );

            var styleSheet = new Stylesheet(fonts, fills, borders, cellFormats);
            return styleSheet;
        }


        #region IDisposable Members

        /// <summary>
        /// Dispose implementation.
        /// </summary>
        public void Dispose() {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        private void dispose(bool disposing) {
            if (disposing == true) {
                Close();
            }
        }

        /// <summary>
        /// Create a new spreadsheet document with the default styles
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static SpreadsheetDocument CreateSpreadsheetDocument(string fileName) {
            var spreadSheetDoc = SpreadsheetDocument.Create(fileName, SpreadsheetDocumentType.Workbook);
            var workbookPart = spreadSheetDoc.AddWorkbookPart();
            spreadSheetDoc.WorkbookPart.Workbook = new Workbook {
                Sheets = new Sheets()
            };

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
            var stylesheet = createStyleSheet();
            stylesPart.Stylesheet = stylesheet;
            stylesPart.Stylesheet.Save();

            return spreadSheetDoc;
        }

        /// <summary>
        /// Writes the content of the table to a spreadsheet with the name sheetName
        /// This will either create a new sheet or append the data to an existing sheet with this name
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="sheetId"></param>
        /// <param name="sheetName"></param>
        /// <param name="table"></param>
        /// <exception cref="Exception"></exception>
        public static void WriteSpreadsheetTable(SpreadsheetDocument workbook, uint sheetId, string sheetName, DataTable table) {
            try {
                // Get column names from data table
                var colNames = new List<string>();
                foreach (DataColumn column in table.Columns) {
                    colNames.Add(column.ColumnName);
                }

                // Try to find existing worksheet with the name sheetName
                var sheet = workbook.WorkbookPart.Workbook
                    .GetFirstChild<Sheets>()
                    .Elements<Sheet>()
                    .FirstOrDefault(s => s.Name == sheetName);

                SheetData sheetData = null;
                if (sheet != null) {
                    //Sheet exists, get sheetData part to append new rows
                    string relationshipId = sheet.Id.Value;
                    var sheetPart = (WorksheetPart)workbook.WorkbookPart.GetPartById(relationshipId);
                    sheetData = sheetPart.Worksheet.GetFirstChild<SheetData>();
                } else {
                    // Create sheet part and worksheet
                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    sheetPart.Worksheet = new Worksheet();

                    // Create sheet
                    sheet = new Sheet() {
                        Id = workbook.WorkbookPart.GetIdOfPart(sheetPart),
                        SheetId = sheetId,
                        Name = sheetName
                    };
                    var sheets = workbook.WorkbookPart.Workbook.GetFirstChild<Sheets>();
                    sheets.Append(sheet);

                    // Create columes
                    var columns = new Columns(
                        colNames
                            .Select((h, hix) => new Column {
                                Min = Convert.ToUInt32(hix + 1),
                                Max = Convert.ToUInt32(hix + 1),
                                CustomWidth = false,
                                Width = Math.Max(4D, (h.Length + 4) * 1D),
                                BestFit = true
                            })
                            .ToArray()
                    );
                    sheetPart.Worksheet.AppendChild(columns);

                    // Create sheet data
                    sheetData = new SheetData();
                    sheetPart.Worksheet.Append(sheetData);

                    // Create header row
                    var headerRow = new Row();
                    foreach (var colName in colNames) {
                        var cell = new Cell {
                            DataType = CellValues.String,
                            CellValue = new CellValue(colName),
                            StyleIndex = 1
                        };
                        headerRow.AppendChild(cell);
                    }
                    sheetData.AppendChild(headerRow);
                }

                // Fill or append table data to sheet
                foreach (DataRow dsrow in table.Rows) {
                    var row = new Row();
                    foreach (var col in colNames) {
                        if (dsrow.IsNull(col)) {
                            row.AppendChild(new Cell());
                            continue;
                        }

                        var colDef = table.Columns[col];
                        var xlDataType = CellValues.String;
                        var styleIndex = 0U;
                        CellValue xlDataValue = null;
                        if (colDef.DataType.IsSubclassOf(typeof(Enum))) {
                            xlDataValue = new CellValue(string.Format(CultureInfo.InvariantCulture, "{0}", ((Enum)dsrow[col]).GetDisplayName()));
                        } else if (colDef.DataType.IsNumeric()) {
                            xlDataType = CellValues.Number;
                            xlDataValue = new CellValue(Convert.ToDouble(dsrow[col]));
                        } else if (colDef.DataType == typeof(bool)) {
                            xlDataType = CellValues.Number;
                            xlDataValue = new CellValue(Convert.ToBoolean(dsrow[col]) ? 1 : 0);
                        } else if (colDef.DataType == typeof(DateTime)) {
                            xlDataType = CellValues.Number;
                            xlDataValue = new CellValue(Convert.ToDateTime(dsrow[col]).ToOADate());
                            styleIndex = 2U;
                        } else {
                            xlDataValue = new CellValue(dsrow[col].ToString());
                        }

                        var cell = new Cell {
                            DataType = xlDataType,
                            CellValue = xlDataValue,
                            StyleIndex = new UInt32Value(styleIndex)
                        };
                        row.AppendChild(cell);
                    }
                    sheetData.AppendChild(row);
                }
            } catch (Exception ex) {
                throw new Exception($"An error occured in table '{table.TableName}': {ex}");
            }
        }
        #endregion
    }
}
