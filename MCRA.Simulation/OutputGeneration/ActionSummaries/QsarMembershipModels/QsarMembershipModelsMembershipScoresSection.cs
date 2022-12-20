using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class QsarMembershipModelsMembershipScoresSection : SummarySection {

        public List<QsarMembershipModelMembershipScoresRecord> Records { get; set; }

        public void Summarize(ICollection<QsarMembershipModel> qsarMembershipModels, HashSet<Compound> compounds) {
            Records = qsarMembershipModels
                .Select(r => new QsarMembershipModelMembershipScoresRecord() {
                    Code = r.Code,
                    Name = r.Name,
                    MembershipScores = r.MembershipScores
                        .Where(e => compounds.Contains(e.Key))
                        .Select(e => new QsarMembershipModelSubstanceRecord() {
                            SubstanceCode = e.Key.Code,
                            SubstanceName = e.Key.Name,
                            MembershipScore = e.Value
                        })
                        .ToList(),
                })
                .ToList();
        }
    }
}
