using MCRA.Utils.DataFileReading;
using System.Xml.Serialization;

namespace MCRA.Data.Raw.Converters {
    public sealed class EntityCodeConversionsCollection {

        private static HashSet<string> _originalColumnNameAliases =
            new(new [] { "Original", "Alias", "Old", "OldCode", "From" }, StringComparer.OrdinalIgnoreCase);

        private static HashSet<string> _targetColumnNameAliases =
            new(new [] { "Target", "Code", "New", "NewCode", "To" }, StringComparer.OrdinalIgnoreCase);

        private static char[] _quotes = { '\"', ' ' };

        public string IdEntity { get; set; }

        [XmlArrayItem("ConversionTuple")]
        public List<EntityCodeConversionTuple> ConversionTuples { get; set; }

        public string RecodingsFileName { get; set; }

        public void LoadFromFile(string basePath) {
            if (!string.IsNullOrEmpty(RecodingsFileName)) {
                var tuples = new List<EntityCodeConversionTuple>();
                var recodingsFileName = RecodingsFileName.Trim();
                var filename = Path.IsPathRooted(recodingsFileName)
                    ? recodingsFileName
                    : Path.Combine(basePath, recodingsFileName);
                using (var stream = new FileStream(filename, FileMode.Open)) {
                    var reader = new CsvDataReader(stream);
                    var headers = reader.Header.Select(r => r.Trim()).ToList();
                    var originalColumnIndex = headers.TakeWhile(r => !_originalColumnNameAliases.Contains(r)).Count();
                    var targetColumnIndex = headers.TakeWhile(r => !_targetColumnNameAliases.Contains(r)).Count();
                    if (originalColumnIndex < headers.Count && targetColumnIndex < headers.Count) {
                        while (reader.NextResult()) {
                            if (!reader.IsDBNull(originalColumnIndex) && !reader.IsDBNull(targetColumnIndex)) {
                                var oldCode = reader.GetString(originalColumnIndex).Trim(_quotes).Replace("\\\"", "\"");
                                var newCode = reader.GetString(targetColumnIndex).Trim(_quotes).Replace("\\\"", "\"");
                                if (!string.IsNullOrEmpty(oldCode) && !string.IsNullOrEmpty(newCode)) {
                                    tuples.Add(new EntityCodeConversionTuple(oldCode, newCode));
                                }
                            }
                        }
                    }
                }
                var duplicateConversions = tuples.GroupBy(r => r.OriginalCode);
                if (duplicateConversions.Any(r => r.Count() > 1)) {
                    var duplicateCodes = duplicateConversions
                        .Where(r => r.Count() > 1)
                        .Select(r => r.Key).ToList();
                    throw new Exception($"Duplicate conversions found in file {recodingsFileName}: {string.Join(",", duplicateCodes)}.");
                }
                ConversionTuples = tuples;
            }
        }
    }
}
