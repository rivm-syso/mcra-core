using System.Collections.Generic;
using System.Linq;
using MCRA.Utils.Collections;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DoseResponseMixtureSet {
        public string Mixture { get; set; }
        public string ModelName { get; set; }
        public SerializableDictionary<string, double> RPFDict { get; set; }
        public List<DoseResponseMixtureRecord> DoseResponseMixtureRecords { get; set; }

        public List<DoseResponseRecord> CumulativeExposure() {
            var doseResponseRecords = new List<DoseResponseRecord>();
            foreach (var record in DoseResponseMixtureRecords) {
                var result = new DoseResponseRecord() {
                    Response = record.Response,
                    Dose = (record.MixtureDose.All(r => RPFDict.ContainsKey(r.SubstanceCode))) ? record.MixtureDose.Sum(c => c.Dose * RPFDict[c.SubstanceCode]) : double.NaN,
                };
                doseResponseRecords.Add(result);
            }
            return doseResponseRecords;
        }
    }
}
