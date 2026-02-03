using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration.Generic;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class ConcentrationPercentilesSection : SummarySection {
        public override bool SaveTemporaryData => true;
        public List<ConcentrationPercentileRecord> Records { get; set; } = [];
        public void Summarize(
            ICollection<SimpleSubstanceConcentration> concentrations,
            ICollection<Compound> substances,
            double uncertaintyLowerBound,
            double uncertaintyUpperBound,
            List<double> percentages
        ) {
            foreach (var substance in substances) {
                if (concentrations.Where(c => c.Substance.Code == substance.Code).Any(c => c.Concentration > 0)) {
                    var percentiles = concentrations
                        .Where(c => c.Substance.Code == substance.Code)
                        .Select(c => c.Concentration)
                        .Percentiles(percentages);

                    var zip = percentages.Zip(percentiles, (x, v) => new { X = x, V = v })
                        .ToList();

                    var records = zip.Select(p => new ConcentrationPercentileRecord {
                        UncertaintyLowerLimit = uncertaintyLowerBound,
                        UncertaintyUpperLimit = uncertaintyUpperBound,
                        XValue = p.X / 100,
                        Value = p.V,
                        Values = [],
                        SubstanceName = substance.Name,
                        SubstanceCode = substance.Code,
                    })
                    .ToList();
                    Records.AddRange(records);
                }
            }
        }

        public void SummarizeUncertainty(
            ICollection<SimpleSubstanceConcentration> concentrations,
            ICollection<Compound> substances,
            List<double> percentages
        ) {
            foreach (var substance in substances) {
                if (concentrations.Where(c => c.Substance.Code == substance.Code).Any(c => c.Concentration > 0)) {
                    var percentiles = concentrations
                        .Where(c => c.Substance.Code == substance.Code)
                        .Select(c => c.Concentration)
                        .Percentiles(percentages);
                    var records = Records
                        .Where(r => r.SubstanceCode == substance.Code);
                    var zip = records.Zip(percentiles, (r, v) => new { Record = r, Value = v })
                        .ToList();
                    foreach (var item in zip) {
                        item.Record.Values.Add(item.Value);
                    }
                }
            }
        }
    }
}
