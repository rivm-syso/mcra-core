using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using MCRA.Data.Management;
using MCRA.General.Action.ActionSettingsManagement;
using MCRA.Data.Management.CompiledDataManagers;
using MCRA.Data.Management.RawDataWriters;
using MCRA.General;
using MCRA.General.ModuleDefinitions;
using MCRA.Simulation.Actions.ActionComparison;
using MCRA.Simulation.Calculators.PercentilesUncertaintyFactorialCalculation;
using MCRA.Simulation.OutputGeneration;
using System.Collections.Generic;
using MCRA.Simulation.Action.UncertaintyFactorial;

namespace MCRA.Simulation.Action {

    public interface IActionCalculator {

        // Action info

        ModuleDefinition ModuleDefinition { get; }

        ActionType ActionType { get; }

        SourceTableGroup TableGroup { get; }

        bool CanCompute { get; }

        List<ActionInputRequirement> InputActionTypes { get; }

        bool ResultsComputed { get; set;  }

        bool ResultsSummarized { get; set; }

        bool SimulationDataUpdated { get; set; }

        bool ShouldCompute { get; }

        // General functions

        List<int> GetRawDataSources();

        void Verify();

        IActionSettingsManager GetSettingsManager();

        Dictionary<ScopingType, DataReadingReport> GetDataReadingReport(ICompiledLinkManager linkManager);

        bool CheckDataDependentSettings(ICompiledLinkManager linkManager);

        void LoadData(ActionData data, SubsetManager subsetManager, CompositeProgressState progressReport);

        void LoadDefaultData(ActionData data);

        ActionSettingsSummary SummarizeSettings();

        IActionResult Run(ActionData data, CompositeProgressState progressReport);

        void SummarizeActionResult(IActionResult actionResult, ActionData data, SectionHeader header, int order, CompositeProgressState progressReport);

        void WriteOutputData(IRawDataWriter rawDataWriter, ActionData data, IActionResult result);

        void UpdateSimulationData(ActionData data, IActionResult result);

        // Uncertainty loop

        ICollection<UncertaintySource> GetRandomSources();

        void LoadDataUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, CompositeProgressState progressReport);

        IActionResult RunUncertain(ActionData data, UncertaintyFactorialSet factorialSet, Dictionary<UncertaintySource, IRandom> uncertaintySourceGenerators, SectionHeader header, CompositeProgressState progressReport);

        void SummarizeActionResultUncertain(UncertaintyFactorialSet factorialSet, IActionResult actionResult, ActionData data, SectionHeader header, CompositeProgressState progressReport);

        void WriteOutputDataUncertain(IRawDataWriter rawDataWriter, ActionData data, IActionResult result, int idBootstrap);

        void UpdateSimulationDataUncertain(ActionData data, IActionResult result);

        // Uncertainty factorial

        void SummarizeUncertaintyFactorial(UncertaintyFactorialDesign uncertaintyFactorial, List<UncertaintyFactorialResultRecord> factorialResult, SectionHeader header);

        // Action comparison

        IActionComparisonData LoadActionComparisonData(ICompiledDataManager compiledDataManager, string idResultSet, string nameResultSet);

        void SummarizeComparison(ICollection<IActionComparisonData> comparisonData, SectionHeader header);

    }
}
