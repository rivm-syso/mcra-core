using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using System.Text;

namespace MCRA.Simulation.Test.Helpers {
    public sealed class SampleCompoundCollectionCsvWriter {

        public bool PrintLocation { get; set; } = true;

        public SampleCompoundCollectionCsvWriter() {
        }

        public string WriteCsv(
            ICollection<SampleCompoundCollection> sampleCompoundCollections,
            ICollection<Compound> compounds,
            string fileName,
            bool printImputedNonDetects,
            bool printImputedMissingValues
        ) {
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
            var records = new List<string> {
                "Sample",
                "Food name",
                "Food code"
            };
            if (PrintLocation) {
                records.Add("Origin");
            }
            records.AddRange(compounds.Select(c => c.Code));
            streamWriter.WriteLine(string.Join(",", records));
        }

        private void printCsvRecords(StreamWriter streamWriter, SampleCompoundCollection sampleCompoundCollection, ICollection<Compound> compounds, bool printImputedNonDetects, bool printImputedMissingValues) {
            foreach (var record in sampleCompoundCollection.SampleCompoundRecords) {
                var records = new List<string> {
                    record.FoodSample?.Code ?? string.Empty,
                    sampleCompoundCollection.Food.Name,
                    sampleCompoundCollection.Food.Code
                };
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
