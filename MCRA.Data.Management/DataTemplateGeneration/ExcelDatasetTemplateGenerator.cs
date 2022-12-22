using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using MCRA.General;
using MCRA.General.ModuleDefinitions;
using MCRA.General.TableDefinitions;

namespace MCRA.Data.Management.DataTemplateGeneration {

    /// <summary>
    /// Generator class for creating a template Excel file for tables
    /// of specific table groups.
    /// </summary>
    public class ExcelDatasetTemplateGenerator : IDatasetTemplateGenerator {

        private readonly string _targetFile;

        /// <summary>
        /// Creates a new <see cref="ExcelDatasetTemplateGenerator"/> instance.
        /// </summary>
        /// <param name="targetFileName"></param>
        public ExcelDatasetTemplateGenerator(string targetFileName) {
            _targetFile = targetFileName;
        }

        /// <summary>
        /// Method to generate the template for the specified data source.
        /// </summary>
        /// <param name="sourceTableGroup">Source table group of the tables to create</param>
        /// <param name="dataFormatId">Optional data format id for a subset of the table group</param>
        public void Create(SourceTableGroup sourceTableGroup, string dataFormatId = null) {
            using (var document = SpreadsheetDocument.Create(_targetFile, SpreadsheetDocumentType.Workbook)) {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());

                var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();
                var stylesheet = createStylesheet();
                stylesPart.Stylesheet = stylesheet;
                stylesPart.Stylesheet.Save();

                createReadMe(sourceTableGroup, workbookPart, sheets, 1);
                createTables(sourceTableGroup, workbookPart, sheets, dataFormatId);
                workbookPart.Workbook.Save();
            }
        }

        private void createTables(
            SourceTableGroup sourceTableGroup,
            WorkbookPart workbookPart,
            Sheets sheets,
            string dataFormatId = null
        ) {
            uint sheetIndex = 2;
            var tableIds = McraTableDefinitions.Instance.GetTableGroupRawTables(sourceTableGroup, dataFormatId);
            foreach (var tableId in tableIds) {
                var table = McraTableDefinitions.Instance.GetTableDefinition(tableId);
                var headers = table.ColumnDefinitions
                    .Select(r => r.Id)
                    .ToList();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet();

                var columns = new Columns(
                    headers
                        .Select((h, hix) => new Column {
                            Min = Convert.ToUInt32(hix + 1),
                            Max = Convert.ToUInt32(hix + 1),
                            CustomWidth = false,
                            Width = Math.Max(4D, (h.Length + 4) * 1D),
                            BestFit = true
                        })
                        .ToArray()
                );
                worksheetPart.Worksheet.AppendChild(columns);

                var sheetData = new SheetData();
                worksheetPart.Worksheet.Append(sheetData);

                var sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = sheetIndex++, Name = table.Id };
                sheets.Append(sheet);

                var headerRow = new Row();

                foreach (var header in headers) {
                    var cell = new Cell {
                        DataType = CellValues.String,
                        CellValue = new CellValue(header),
                        StyleIndex = 2
                    };
                    headerRow.AppendChild(cell);
                }

                sheetData.AppendChild(headerRow);
            }
        }

        private void createReadMe(
            SourceTableGroup sourceTableGroup,
            WorkbookPart workbookPart,
            Sheets sheets,
            uint sheetId
        ) {
            var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet();

            // Setting up columns
            var columns = new Columns(
                new Column {
                    Min = 1,
                    Max = 1,
                    Width = 140,
                    CustomWidth = true
                }
            );
            worksheetPart.Worksheet.AppendChild(columns);

            var sheetData = new SheetData();
            worksheetPart.Worksheet.Append(sheetData);

            var sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = sheetId, Name = "Read me" };
            sheets.Append(sheet);

            var headerRow = new Row();

            var tableGroup = McraTableDefinitions.Instance.DataGroupDefinitions[sourceTableGroup];
            var module = McraModuleDefinitions.Instance.ModuleDefinitionsByTableGroup[sourceTableGroup];
            var moduleClass = McraModuleDefinitions.Instance.GetActionClass(module.ActionType).ToString();

            string readmeText;
            readmeText = getReadmeText();

            //var readmeText = File.ReadAllText("./Resources/TextTemplates/ExcelDataSourceTemplate_ReadMe.md");
            readmeText = readmeText.Replace("[TableGroupId]", tableGroup.Id.ToLower());
            readmeText = readmeText.Replace("[TableGroup]", tableGroup.Name.ToLower());
            readmeText = readmeText.Replace("[ModuleId]", module.Id.ToLower());
            readmeText = readmeText.Replace("[ModuleClassId]", moduleClass.ToLower());

            var cell = new Cell {
                DataType = CellValues.String,
                CellValue = new CellValue(readmeText),
                StyleIndex = 1
            };
            headerRow.AppendChild(cell);

            sheetData.AppendChild(headerRow);
        }

        private static string getReadmeText() {
            string readmeText;
            var assembly = typeof(ExcelDatasetTemplateGenerator).Assembly;
            var resourceName = $"{assembly.GetName().Name}.Resources.TextTemplates.ExcelDataSourceTemplate_ReadMe.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    readmeText = reader.ReadToEnd();
                }
            }
            return readmeText;
        }

        private Stylesheet createStylesheet() {
            var fonts = new Fonts(
                new Font( // Index 0 - default
                    new FontSize() { Val = 11 }
                ),
                new Font( // Index 1 - header
                    new FontSize() { Val = 11 },
                    new Bold(),
                    new Color() { Rgb = "FFFFFF" }
                )
            );

            var fills = new Fills(
                new Fill(new PatternFill() { PatternType = PatternValues.None }),       // Index 0 - default
                new Fill(new PatternFill() { PatternType = PatternValues.Gray125 }),    // Index 1 - The default fill of gray 125 (required)
                new Fill(new PatternFill(                                                // Index 2 - header
                        new ForegroundColor { Rgb = new HexBinaryValue() { Value = "2F75B5" } }
                    ) { PatternType = PatternValues.Solid })
            );

            var borders = new Borders(
                new Border(), // index 0 default
                new Border( // index 1 black border
                    new LeftBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new RightBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new TopBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new BottomBorder(new Color() { Auto = true }) { Style = BorderStyleValues.Thin },
                    new DiagonalBorder())
            );

            var cellFormats = new CellFormats(
                new CellFormat(), // default
                new CellFormat {  // readme
                    FontId = 0,
                    FillId = 0,
                    BorderId = 1,
                    ApplyBorder = true,
                    Alignment = new Alignment() {
                        WrapText = true
                    }
                },
                new CellFormat {  // header
                    FontId = 1,
                    FillId = 2,
                    BorderId = 1,
                    ApplyFill = true
                }
            );

            var styleSheet = new Stylesheet(fonts, fills, borders, cellFormats);
            return styleSheet;
        }
    }
}
