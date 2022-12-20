using MCRA.Data.Compiled.Objects;
using MCRA.Data.Management;
using MCRA.Data.Management.CompiledDataManagers.DataReadingSummary;
using MCRA.Data.Management.RawDataObjectConverters;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.Action.Settings.Dto;
using MCRA.General.Annotations;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.HazardCharacterisationCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Actions.RelativePotencyFactors {

    [ActionType(ActionType.RelativePotencyFactors)]
    public class RelativePotencyFactorsActionCalculator : ActionCalculatorBase<RelativePotencyFactorsActionResult> {

        public RelativePotencyFactorsActionCalculator(ProjectDto project) : base(project) {
        }

        protected override void verify() {
            _actionInputRequirements[ActionType.AOPNetworks].IsRequired = _project.EffectSettings.IncludeAopNetworks;
            _actionInputRequirements[ActionType.AOPNetworks].IsVisible = _project.EffectSettings.IncludeAopNetworks;
            _actionDataLinkRequirements[ScopingType.RelativePotencyFactors][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.RelativePotencyFactorsUncertain][ScopingType.Effects].AlertTypeMissingData = AlertType.Notification;
            _actionDataLinkRequirements[ScopingType.RelativePotencyFactorsUncertain][ScopingType.Compounds].AlertTypeMissingData = AlertType.Notification;
        }

        public override ICollection<UncertaintySource> GetRandomSources() {
            var result = new List<UncertaintySource>();
            if (_project.UncertaintyAnalysisSettings.ReSampleRPFs) {
                result.Add(UncertaintySource.RPFs);
            }
            return result;
        }

        protected override ActionSettingsSummary summarizeSettings() {
            var summarizer = new RelativePotencyFactorsSettingsSummarizer();
            return summarizer.Summarize(_project);
        }

        protected override void loadData(
            ActionData data,
            SubsetManager subsetManager,
            CompositeProgressState progressState
        ) {
            if (data.SelectedEffect != null && subsetManager.AllRelativePotencyFactors.ContainsKey(data.SelectedEffect.Code)) {
                data.RawRelativePotencyFactors = subsetManager.AllRelativePotencyFactors[data.SelectedEffect.Code].ToDictionary(r => r.Compound);
            }
            var correctedRpfs = loadRelativePotencyFactors(data.ActiveSubstances, data.ReferenceCompound, data.RawRelativePotencyFactors);
            data.CorrectedRelativePotencyFactors = correctedRpfs;
            checkRpfs(data);
        }

        private static void checkRpfs(ActionData data) {
            if (data.ActiveSubstances.Any(r => !data.CorrectedRelativePotencyFactors.TryGetValue(r, out var rpf) || double.IsNaN(rpf))) {
                var missingCount = data.CorrectedRelativePotencyFactors.Where(r => double.IsNaN(r.Value)).ToList();
                throw new Exception($"Missing relative potency factors for {missingCount.Count} substances");
            }
        }

        protected override RelativePotencyFactorsActionResult run(ActionData data, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var result = new RelativePotencyFactorsActionResult();
            var correctedRpfs = computeRelativePotencyFactors(
                data.ActiveSubstances,
                data.ReferenceCompound,
                data.HazardCharacterisations
            );
            result.CorrectedRelativePotencyFactors = correctedRpfs;
            localProgress.Update(100);
            return result;
        }

        protected override void updateSimulationData(ActionData data, RelativePotencyFactorsActionResult result) {
            if (result.CorrectedRelativePotencyFactors != null) {
                data.CorrectedRelativePotencyFactors = result.CorrectedRelativePotencyFactors;
            }
            if (_project.ActionType != ActionType.RelativePotencyFactors) {
                checkRpfs(data);
            }
        }

        protected override void summarizeActionResult(RelativePotencyFactorsActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new RelativePotencyFactorsSummarizer();
            summarizer.Summarize(_project, actionResult, data, header, order);
            localProgress.Update(100);
        }

        protected override void loadDataUncertain(
            ActionData data,
            UncertaintyFactorialSet factorialSet,
            Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators,
            CompositeProgressState progressReport
        ) {
            if (factorialSet.Contains(UncertaintySource.RPFs) && data.RawRelativePotencyFactors != null) {
                var compounds = data.ActiveSubstances;
                var reference = compounds.First(r => r.Code == _project.EffectSettings.CodeReferenceCompound);
                var rawRelativePotencyFactors = data.RawRelativePotencyFactors;
                var correctedRpfs = resampleRelativePotencyFactors(
                    compounds,
                    reference,
                    rawRelativePotencyFactors,
                    uncertaintySourceGenerators[UncertaintySource.RPFs]
                );
                data.CorrectedRelativePotencyFactors = correctedRpfs;
            }
        }

        protected override RelativePotencyFactorsActionResult runUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport) {
            var result = new RelativePotencyFactorsActionResult();
            var correctedRpfs = computeRelativePotencyFactors(data.ActiveSubstances, data.ReferenceCompound, data.HazardCharacterisations);
            result.CorrectedRelativePotencyFactors = correctedRpfs;
            return result;
        }

        protected override void summarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, RelativePotencyFactorsActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport) {
            var localProgress = progressReport.NewProgressState(100);
            var summarizer = new RelativePotencyFactorsSummarizer();
            summarizer.SummarizeUncertain(_project, actionResult, data, header);
            localProgress.Update(100);
        }

        protected override void updateSimulationDataUncertain(ActionData data, RelativePotencyFactorsActionResult result) {
            updateSimulationData(data, result);
        }

        private static Dictionary<Compound, double> loadRelativePotencyFactors(
            ICollection<Compound> substances,
            Compound reference, IDictionary<Compound, RelativePotencyFactor> rawRelativePotencyFactors
        ) {
            var referenceRpf = reference != null && rawRelativePotencyFactors.ContainsKey(reference) ? (double)rawRelativePotencyFactors[reference].RPF : double.NaN;
            var referenceRpfCorrectionFactor = !double.IsNaN(referenceRpf) ? 1D / referenceRpf : 1D;
            var result = new Dictionary<Compound, double>();
            foreach (var substance in substances) {
                if (substance == reference) {
                    result[substance] = 1D;
                } else if (!double.IsNaN(referenceRpf) && rawRelativePotencyFactors.ContainsKey(substance)) {
                    var rawRpf = (double)rawRelativePotencyFactors[substance].RPF;
                    result[substance] = referenceRpfCorrectionFactor * rawRpf;
                } else {
                    result[substance] = double.NaN;
                }
            }
            return result;
        }

        private static Dictionary<Compound, double> computeRelativePotencyFactors(
            ICollection<Compound> substances,
            Compound referenceSubstance,
            IDictionary<Compound, IHazardCharacterisationModel> hazardCharacterisations
        ) {
            var result = new Dictionary<Compound, double>();

            if (referenceSubstance != null && !substances.Contains(referenceSubstance)) {
                throw new Exception("Reference substance is not part of the active substances.");
            } else if (referenceSubstance == null) {
                //For MixtureSelection purposes, it is irrelevant which substance is the reference substance
                referenceSubstance = substances.First();
            }
            var referenceHazardDose = hazardCharacterisations != null && hazardCharacterisations.ContainsKey(referenceSubstance)
                ? hazardCharacterisations[referenceSubstance].Value
                : double.NaN;

            foreach (var substance in substances) {
                if (!double.IsNaN(referenceHazardDose) && hazardCharacterisations.ContainsKey(substance)) {
                    var hazardDose = hazardCharacterisations[substance].Value;
                    var correctedRpf = referenceHazardDose / hazardDose;
                    result[substance] = correctedRpf;
                } else {
                    result[substance] = double.NaN;
                }
            }
            return result;
        }

        private static Dictionary<Compound, double> resampleRelativePotencyFactors(
            ICollection<Compound> compounds,
            Compound reference,
            IDictionary<Compound, RelativePotencyFactor> rawRelativePotencyFactors,
            IRandom generator
        ) {
            var referenceRawRpf = rawRelativePotencyFactors.ContainsKey(reference)
                ? rawRelativePotencyFactors[reference] : null;
            double referenceRpfCorrectionFactor = double.NaN;

            var capturingGenerator = new CapturingGenerator(generator);
            capturingGenerator.StartCapturing();

            var result = new Dictionary<Compound, double>();
            if (referenceRawRpf != null) {
                double sampledReferenceRawRpf;
                if (referenceRawRpf.RelativePotencyFactorsUncertains.Any()) {
                    var ix = generator.Next(0, referenceRawRpf.RelativePotencyFactorsUncertains.Count);
                    sampledReferenceRawRpf = (double)referenceRawRpf.RelativePotencyFactorsUncertains.ElementAt(ix).RPF;
                } else {
                    generator.Next();
                    sampledReferenceRawRpf = (double)referenceRawRpf.RPF;
                }
                referenceRpfCorrectionFactor = 1D / sampledReferenceRawRpf;
                result[reference] = 1D;
            }

            foreach (var compound in compounds) {
                capturingGenerator.Repeat();
                if (compound != reference) {
                    var rawRpf = rawRelativePotencyFactors.ContainsKey(compound) ? rawRelativePotencyFactors[compound] : null;
                    if (rawRpf != null && rawRpf.RelativePotencyFactorsUncertains.Any()) {
                        var ix = generator.Next(0, rawRpf.RelativePotencyFactorsUncertains.Count);
                        var sampledRawRpf = (double)rawRpf.RelativePotencyFactorsUncertains.ElementAt(ix).RPF;
                        result[compound] = referenceRpfCorrectionFactor * sampledRawRpf;
                    } else {
                        result[compound] = double.NaN;
                    }
                }
            }

            return result;
        }

        protected override void writeOutputData(IRawDataWriter rawDataWriter, ActionData data, RelativePotencyFactorsActionResult result) {
            var rawDataConverter = new RawRelativePotencyFactorDataConverter();
            var rawData = rawDataConverter.ToRaw(data.SelectedEffect, data.CorrectedRelativePotencyFactors);
            rawDataWriter.Set(rawData);
        }
    }
}
