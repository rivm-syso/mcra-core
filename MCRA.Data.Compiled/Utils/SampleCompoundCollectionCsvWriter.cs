using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using System.Text;

namespace MCRA.Data.Compiled.Utils {
    public sealed class SampleCompoundCollectionCsvWriter {

        public bool PrintLocation { get; set; } = true;

        public SampleCompoundCollectionCsvWriter() {
        }

        public string WriteCsv(List<SampleCompoundCollection> sampleCompoundCollections, ICollection<Compound> compounds, string fileName, bool printImputedNonDetects, bool printImputedMissingValues) {
            using (var stream = new FileStream(fileName, FileMode.Create)) {
                using (var streamWriter = new StreamWriter(stream, Encoding.Default)) {
                    printCsvHeaders(streamWriter, compounds);
                    foreach (var sampleCompoundCollection in sampleCompoundCollections) {
                        printCsvRecords(streamWriter, sampleCompoundCollection, compounds, printImputedNonDetects, printImputedMissingValues);
                    }
                }
            }
            return fileName;
        }

        private void printCsvHeaders(StreamWriter streamWriter, ICollection<Compound> compounds) {
            var records = new List<string>();
            records.Add("Sample");
            records.Add("Food name");
            records.Add("Food code");
            if (PrintLocation) {
                records.Add("Origin");
            }
            records.AddRange(compounds.Select(c => c.Code));
            streamWriter.WriteLine(string.Join(",", records));
        }

        private void printCsvRecords(StreamWriter streamWriter, SampleCompoundCollection sampleCompoundCollection, ICollection<Compound> compounds, bool printImputedNonDetects, bool printImputedMissingValues) {
            foreach (var record in sampleCompoundCollection.SampleCompoundRecords) {
                var records = new List<string>();
                records.Add(record.FoodSample?.Code ?? string.Empty);
                records.Add(sampleCompoundCollection.Food.Name);
                records.Add(sampleCompoundCollection.Food.Code);
                if (PrintLocation) {
                    records.Add(record.FoodSample?.Location ?? string.Empty);
                }
                var concentrationRecords = compounds.Select(c => {
                    if (record.SampleCompounds[c].IsCensoredValue && !printImputedNonDetects) {
                        return "ND";
                    } else if (record.SampleCompounds[c].IsMissingValue && !printImputedMissingValues) {
                        return "MV";
                    } else {
                        return $"{record.SampleCompounds[c].Residue:G3}";
                    }
                }).ToList();
                records.AddRange(concentrationRecords);
                streamWriter.WriteLine(string.Join(",", records));
            }
        }
    }
}
