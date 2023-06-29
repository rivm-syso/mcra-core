using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ApprovalBySubstanceSummarySection : SummarySection {

        public int UnspecifiedApprovals { get; set; }

        public List<ApprovalBySubstanceSummaryRecord> Records { get; set; }

        public void Summarize(
            IDictionary<Compound, SubstanceApproval> substanceApprovals,
            ICollection<Compound> substances
        ) {
            UnspecifiedApprovals = substances?.Except(substanceApprovals.Keys).Count() ?? 0;
            Records = substanceApprovals.Values
                .Select(r => new ApprovalBySubstanceSummaryRecord() {
                        SubstanceCode = r.Substance.Code,
                        SubstanceName = r.Substance.Name,
                        IsApproved = r.IsApproved,
                    }
                )
                .OrderBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
