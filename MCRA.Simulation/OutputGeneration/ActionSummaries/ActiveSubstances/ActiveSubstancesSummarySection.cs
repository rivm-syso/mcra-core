using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using System.Text;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ActiveSubstancesSummarySection : SummarySection {

        public List<string> SubstanceNames;
        public List<string> SubstanceCodes;

        public List<ActiveSubstanceModelRecord> Records { get; set; }

        private ActiveSubstancesTableSection _activeSubstancesTableSection;

        public ActiveSubstancesTableSection ActiveSubstancesTableSection {
            get {
                _activeSubstancesTableSection = new ActiveSubstancesTableSection() {
                    SectionId = SectionId.MungeToGuid("AssessmentGroupMembershipsTableSection"),
                    Record = Records.FirstOrDefault()
                };
                return _activeSubstancesTableSection;
            }
        }

        public void Summarize(IDictionary<Compound, double> membershipProbabilities, ICollection<Compound> substances, Effect effect) {
            var substancesOrdered = substances.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList();
            SubstanceCodes = substancesOrdered.Select(r => r.Code).ToList();
            SubstanceNames = substancesOrdered.Select(r => r.Name).ToList();
            Records = new List<ActiveSubstanceModelRecord>();
            var memberships = substancesOrdered
                .ToDictionary(
                    r => r,
                    r => membershipProbabilities.TryGetValue(r, out var membership) ? membership : double.NaN
                );
            var probableMemberships = memberships.Where(r => !double.IsNaN(r.Value) && r.Value > 0 && r.Value < 1).ToList();
            var certainMemberships = memberships.Where(r => r.Value == 1).ToList();
            var mean = memberships.Any() ? memberships.Select(r => r.Value).Where(r => !double.IsNaN(r)).Average() : double.NaN;
            var median = memberships.Any() ? memberships.Select(r => r.Value).Where(r => !double.IsNaN(r)).Median() : double.NaN;
            var record = new ActiveSubstanceModelRecord() {
                EffectCode = effect?.Code,
                EffectName = effect?.Name,
                IsProbabilistic = memberships.Any(r => r.Value > 0 && r.Value < 1),
                MembershipScoresCount = memberships.Count(r => !double.IsNaN(r.Value)),
                MissingMembershipScoresCount = memberships.Count(r => double.IsNaN(r.Value)),
                ProbableMembershipsCount = probableMemberships.Count,
                CertainMembershipsCount = certainMemberships.Count,
                MembershipScoresMean = mean,
                MembershipScoresMedian = median,
                MembershipProbabilities = memberships
                    .Select(r => new ActiveSubstanceRecord() {
                        SubstanceCode = r.Key.Code,
                        SubstanceName = r.Key.Name,
                        Probability = r.Value
                    })
                    .OrderByDescending(r => r.Probability)
                    .ThenBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };
            Records.Add(record);
        }

        public void Summarize(ICollection<ActiveSubstanceModel> activeSubstanceModels, ICollection<Compound> substances) {
            var substancesOrdered = substances.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList();
            SubstanceCodes = substancesOrdered.Select(r => r.Code).ToList();
            SubstanceNames = substancesOrdered.Select(r => r.Name).ToList();
            Records = new List<ActiveSubstanceModelRecord>();
            foreach (var model in activeSubstanceModels) {
                var memberships = substancesOrdered
                    .ToDictionary(
                        r => r,
                        r => model.MembershipProbabilities.TryGetValue(r, out var membership) ? membership : double.NaN
                    );
                var probableMemberships = memberships.Where(r => !double.IsNaN(r.Value) && r.Value > 0 && r.Value < 1).ToList();
                var certainMemberships = memberships.Where(r => r.Value == 1).ToList();
                var mean = memberships.Any() ? memberships.Where(r => !double.IsNaN(r.Value)).Select(r => r.Value).Average() : double.NaN;
                var median = memberships.Any() ? memberships.Where(r => !double.IsNaN(r.Value)).Select(r => r.Value).Median() : double.NaN;
                var record = new ActiveSubstanceModelRecord() {
                    Code = model.Code,
                    Name = model.Name,
                    Description = model.Description,
                    Reference = model.Reference,
                    EffectCode = model.Effect?.Code,
                    EffectName = model.Effect?.Name,
                    IsProbabilistic = memberships.Any(r => r.Value > 0 && r.Value < 1),
                    MembershipScoresCount = memberships.Count(r => !double.IsNaN(r.Value)),
                    MissingMembershipScoresCount = memberships.Count(r => double.IsNaN(r.Value)),
                    ProbableMembershipsCount = probableMemberships.Count,
                    CertainMembershipsCount = certainMemberships.Count,
                    MembershipScoresMean = mean,
                    MembershipScoresMedian = median,
                    MembershipProbabilities = memberships
                        .Select(r => new ActiveSubstanceRecord() {
                            SubstanceCode = r.Key.Code,
                            SubstanceName = r.Key.Name,
                            Probability = r.Value
                        })
                        .OrderByDescending(r => r.Probability)
                        .ThenBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                        .ToList()
                };
                Records.Add(record);
            }
        }

        public string WriteCsv(string filename) {
            var membershipLookups = Records
                .Select(r => r.MembershipProbabilities.ToDictionary(p => p.SubstanceCode, p => p.Probability))
                .ToList();
            using (var stream = new FileStream(filename, FileMode.Create)) {
                using (var streamWriter = new StreamWriter(stream, Encoding.Default)) {
                    var headers = new List<string>() { "IdSubstance", "SubstanceName" };
                    headers.AddRange(Records.Select(r => r.Code).ToList());
                    streamWriter.WriteLine(string.Join(",", headers));
                    for (int i = 0; i < SubstanceCodes.Count; i++) {
                        var records = new List<string>() { $"\"{SubstanceCodes[i]}\"", $"\"{SubstanceNames[i]}\"" };
                        records.AddRange(membershipLookups
                            .Select(r => r.TryGetValue(SubstanceCodes[i], out var prob) && !double.IsNaN(prob) ? prob.ToString() : string.Empty));
                        streamWriter.WriteLine(string.Join(",", records));
                    }
                }
            }
            return filename;
        }
    }
}
