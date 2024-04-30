using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Action;
using MCRA.Simulation.OutputGeneration;
using MCRA.General.ModuleDefinitions.Settings;

namespace MCRA.Simulation.Actions.RelativePotencyFactors {
    public enum RelativePotencyFactorsSections {         
        //No sub-sections
    }
    public class RelativePotencyFactorsSummarizer : ActionModuleResultsSummarizer<RelativePotencyFactorsModuleConfig, RelativePotencyFactorsActionResult> {

        public RelativePotencyFactorsSummarizer(RelativePotencyFactorsModuleConfig config) : base(config) {
        }

        public override void Summarize(ActionModuleConfig sectionConfig, RelativePotencyFactorsActionResult actionResult, ActionData data, SectionHeader header, int order) {
            var outputSettings = new ModuleOutputSectionsManager<RelativePotencyFactorsSections>(sectionConfig, ActionType);
            if (!outputSettings.ShouldSummarizeModuleOutput()) {
                return;
            }
            var subHeader = summarizeRelativePotencyFactors(
                data.CorrectedRelativePotencyFactors,
                data.ActiveSubstances,
                _configuration.UncertaintyLowerBound,
                _configuration.UncertaintyUpperBound,
                header,
                order
            );
            subHeader.Units = collectUnits();
            if (data.RawRelativePotencyFactors?.Any() ?? false) {
                createCompoundRpfDataSection(
                    data.CorrectedRelativePotencyFactors,
                    data.RawRelativePotencyFactors,
                    data.ActiveSubstances,
                    subHeader,
                    0);
            }
        }

        public void SummarizeUncertain(ProjectDto project, RelativePotencyFactorsActionResult actionResult, ActionData data, SectionHeader header) {
            if (_configuration.ReSampleRPFs) {
                summarizeRelativePotencyFactorsUncertain(data, header);
            }
        }

        private List<ActionSummaryUnitRecord> collectUnits(){
            var result = new List<ActionSummaryUnitRecord> {
                new("LowerBound", $"p{_configuration.UncertaintyLowerBound}"),
                new("UpperBound", $"p{_configuration.UncertaintyUpperBound}")
            };
            return result;
        }

        private SectionHeader summarizeRelativePotencyFactors(
                 IDictionary<Compound, double> correctedRelativePotencyFactors,
                 ICollection<Compound> activeSubstances,
                 double uncertaintyLowerLimit,
                 double uncertaintyUpperLimit,
                 SectionHeader header,
                 int order
            ) {
            var section = new RelativePotencyFactorsSummarySection();
            var subHeader = header.AddSubSectionHeaderFor(section, ActionType.GetDisplayName(), order);
            section.Records = activeSubstances.Select(r => {
                var record = new RelativePotencyFactorsSummaryRecord() {
                    CompoundCode = r.Code,
                    CompoundName = r.Name,
                    RelativePotencyFactorUncertaintyValues = new List<double>(),
                    UncertaintyLowerBound = uncertaintyLowerLimit,
                    UncertaintyUpperBound = uncertaintyUpperLimit
                };
                if (correctedRelativePotencyFactors.ContainsKey(r)) {
                    record.RelativePotencyFactor = correctedRelativePotencyFactors[r];
                }
                return record;
            }).ToList();
            subHeader.SaveSummarySection(section);
            return subHeader;
        }

        private static void summarizeRelativePotencyFactorsUncertain(ActionData data, SectionHeader header) {
            var subHeader = header.GetSubSectionHeader<RelativePotencyFactorsSummarySection>();
            if (subHeader != null) {
                var section = subHeader.GetSummarySection() as RelativePotencyFactorsSummarySection;
                var modelsLookup = data.CorrectedRelativePotencyFactors.ToDictionary(r => r.Key.Code, r => r.Value);
                foreach (var record in section.Records) {
                    if (modelsLookup.ContainsKey(record.CompoundCode)) {
                        record.RelativePotencyFactorUncertaintyValues.Add(modelsLookup[record.CompoundCode]);
                    }
                }
                subHeader.SaveSummarySection(section);
            }
        }

        private static SectionHeader createCompoundRpfDataSection(
                IDictionary<Compound, double> correctedRelativePotencyFactors,
                IDictionary<Compound, RelativePotencyFactor> rawRelativePotencyFactors,
                ICollection<Compound> activeSubstances,
                SectionHeader header,
                int order
            ) {
            var section = new CompoundRPFDataSection();
            var subHeader = header.AddSubSectionHeaderFor(section, "Available data", order);
            section.Records = activeSubstances.Select(r => {
                var record = new CompoundRPFDataRecord() {
                    CompoundCode = r.Code,
                    CompoundName = r.Name,
                };
                if (rawRelativePotencyFactors?.ContainsKey(r) ?? false) {
                    record.RPF = rawRelativePotencyFactors[r].RPF;
                }
                if (correctedRelativePotencyFactors.ContainsKey(r)) {
                    record.RescaledRPF = correctedRelativePotencyFactors[r];
                }
                return record;
            }).ToList();
            subHeader.SaveSummarySection(section);
            return subHeader;
        }
    }
}
