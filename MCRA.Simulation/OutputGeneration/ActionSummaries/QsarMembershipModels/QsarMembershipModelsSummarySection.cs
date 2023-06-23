using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class QsarMembershipModelsSummarySection : SummarySection {

        public List<QsarMembershipModelRecord> Records { get; set; }

        public void Summarize(ICollection<QsarMembershipModel> qsarMembershipModels, HashSet<Compound> compounds) {
            Records = qsarMembershipModels
                .Select(r => {
                    var scores = r.MembershipScores
                        .Where(e => compounds.Contains(e.Key))
                        .Select(e => e.Value)
                        .ToList();
                    var positivesCount = scores.Count(s => s > 0);
                    return new QsarMembershipModelRecord() {
                        Code = r.Code,
                        Name = r.Name,
                        Description = r.Description,
                        EffectCode = r.Effect.Code,
                        EffectName = r.Effect.Name,
                        Accuracy = r.Accuracy ?? double.NaN,
                        Sensitivity = r.Sensitivity ?? double.NaN,
                        Specificity = r.Specificity ?? double.NaN,
                        MembershipScoresCount = scores.Count,
                        FractionPositives = (scores.Count > 0) ? (double)positivesCount / scores.Count : double.NaN
                    };
                })
                .ToList();
        }
    }
}
