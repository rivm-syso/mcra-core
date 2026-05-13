using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.General.Annotations;
using MCRA.General.ModuleDefinitions.Settings;
using MCRA.General.UnitDefinitions.Defaults;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Objects.SubstanceConcentrations;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Actions.ConsumerProductConcentrations {

    [ActionType(ActionType.ConsumerProductConcentrations)]
    public class ConsumerProductConcentrationsActionCalculator(ProjectDto project) : ActionCalculatorBase<IConsumerProductConcentrationsActionResult>(project) {
        private ConsumerProductConcentrationsModuleConfig ModuleConfig => (ConsumerProductConcentrationsModuleConfig)_moduleSettings;

        protected override void verify() {
            _actionDataLinkRequirements[ScopingType.ConsumerProductConcentrations][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.ConsumerProductConcentrations][ScopingType.ConsumerProducts].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (ModuleConfig.ResampleCPConcentrations) {
                result.Add(UncertaintySource.ConsumerProductConcentrations);
            }
            return result;
        }
        protected override void loadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressState) {
            var targetUnitPerMass = new ConsumerProductConcentrationUnitSpec(SystemUnits.DefaultConsumerProductConcentrationUnit);
            var targetUnitPerUnit = new ConsumerProductConcentrationUnitSpec(SystemUnits.DefaultConsumerProductConcentrationUnitPerUnit);

            var collections = new List<ConsumerProductSubstanceConcentrationsCollection>();
            var adjustedConsumerProductConcentrations = subsetManager.AllConsumerProductConcentrations
                .GroupBy(c => (c.Product))
                .Select(g => {
                    // Here we require that the units of one product belong to one type, either per mass or per unit
                    var targetUnitSpec = g.First().Unit.IsPerUnit() ? targetUnitPerUnit : targetUnitPerMass;
                    var alignedConcentrations = g.Select(c => {
                        var alignmentFactor = c.Unit.GetConcentrationAlignmentFactor(targetUnitSpec.Unit, c.Substance.MolecularMass);
                        var conc = c.Concentration * alignmentFactor;
                        return new AlignedSubstanceConcentration {
                            Substance = c.Substance,
                            Concentration = conc,
                            SamplingWeight = c.SamplingWeight,
                        };
                    }).ToList();

                    var collection = new ConsumerProductSubstanceConcentrationsCollection(alignedConcentrations, targetUnitSpec) {
                        Product = g.Key
                    };
                    return collection;
                })
                .OrderBy(c => c.Product.Code)
                .ToList();

            data.AllConsumerProductConcentrationsCollections = adjustedConsumerProductConcentrations;
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            var localProgress = progressReport.NewProgressState(100);

            if (ModuleConfig.ResampleCPConcentrations) {
                if (factorialSet.Contains(UncertaintySource.ConsumerProductConcentrations)) {
                    localProgress.Update("Resampling consumer product concentrations");

                    var resampledCollections = new List<ConsumerProductSubstanceConcentrationsCollection>();
                    foreach (var collection in data.AllConsumerProductConcentrationsCollections) {
                        var resampledCollection = collection
                            .GroupBy(c => c.Substance)
                            .SelectMany(c => c.Resample(uncertaintySourceGenerators[UncertaintySource.ConsumerProductConcentrations])
                                .Select(r => new AlignedSubstanceConcentration() {
                                    Substance = r.Substance,
                                    Concentration = r.Concentration,
                                    SamplingWeight = (r as AlignedSubstanceConcentration).SamplingWeight,
                                })
                            )
                            .ToList();

                        var newCollection = new ConsumerProductSubstanceConcentrationsCollection(resampledCollection, collection.Unit) {
                            Product = collection.Product
                        };

                        resampledCollections.Add(newCollection);
                    }

                    data.AllConsumerProductConcentrationsCollections = resampledCollections;
                }
            }
            localProgress.Update(100);
        }

        protected override void summarizeActionResult(IConsumerProductConcentrationsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            localProgress.Update("Summarizing consumer product concentration distributions", 0);
            var summarizer = new ConsumerProductConcentrationsSummarizer(ModuleConfig);
            summarizer.Summarize(_actionSettings, actionResult, data, header, order);
            localProgress.Update(100);
        }
    }
}
