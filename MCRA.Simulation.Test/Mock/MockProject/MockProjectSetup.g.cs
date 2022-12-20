using MCRA.General.Action.Settings.Dto;
using Moq;

namespace MCRA.Simulation.Test.Mock.MockProject {
    public partial class MockProject {
        private void initializeSubSettingsMocks() {
            //setup all subsettings moqs
            var AssessmentMock = new Mock<AssessmentSettingsDto>().SetupAllProperties();
            setSettings(AssessmentMock, Project.AssessmentSettings);
            _settingsMoq.Setup(m => m.AssessmentSettings).Returns(AssessmentMock.Object);
            _moqsDict.Add(typeof(AssessmentSettingsDto), AssessmentMock);

            var FoodSurveyMock = new Mock<FoodSurveySettingsDto>().SetupAllProperties();
            setSettings(FoodSurveyMock, Project.FoodSurveySettings);
            _settingsMoq.Setup(m => m.FoodSurveySettings).Returns(FoodSurveyMock.Object);
            _moqsDict.Add(typeof(FoodSurveySettingsDto), FoodSurveyMock);

            var EffectMock = new Mock<EffectSettingsDto>().SetupAllProperties();
            setSettings(EffectMock, Project.EffectSettings);
            _settingsMoq.Setup(m => m.EffectSettings).Returns(EffectMock.Object);
            _moqsDict.Add(typeof(EffectSettingsDto), EffectMock);

            var ConversionMock = new Mock<ConversionSettingsDto>().SetupAllProperties();
            setSettings(ConversionMock, Project.ConversionSettings);
            _settingsMoq.Setup(m => m.ConversionSettings).Returns(ConversionMock.Object);
            _moqsDict.Add(typeof(ConversionSettingsDto), ConversionMock);

            var AgriculturalUseMock = new Mock<AgriculturalUseSettingsDto>().SetupAllProperties();
            setSettings(AgriculturalUseMock, Project.AgriculturalUseSettings);
            _settingsMoq.Setup(m => m.AgriculturalUseSettings).Returns(AgriculturalUseMock.Object);
            _moqsDict.Add(typeof(AgriculturalUseSettingsDto), AgriculturalUseMock);

            var SubsetMock = new Mock<SubsetSettingsDto>().SetupAllProperties();
            setSettings(SubsetMock, Project.SubsetSettings);
            _settingsMoq.Setup(m => m.SubsetSettings).Returns(SubsetMock.Object);
            _moqsDict.Add(typeof(SubsetSettingsDto), SubsetMock);

            var AmountModelMock = new Mock<AmountModelSettingsDto>().SetupAllProperties();
            setSettings(AmountModelMock, Project.AmountModelSettings);
            _settingsMoq.Setup(m => m.AmountModelSettings).Returns(AmountModelMock.Object);
            _moqsDict.Add(typeof(AmountModelSettingsDto), AmountModelMock);

            var ConcentrationModelMock = new Mock<ConcentrationModelSettingsDto>().SetupAllProperties();
            setSettings(ConcentrationModelMock, Project.ConcentrationModelSettings);
            _settingsMoq.Setup(m => m.ConcentrationModelSettings).Returns(ConcentrationModelMock.Object);
            _moqsDict.Add(typeof(ConcentrationModelSettingsDto), ConcentrationModelMock);

            var CovariatesSelectionMock = new Mock<CovariatesSelectionSettingsDto>().SetupAllProperties();
            setSettings(CovariatesSelectionMock, Project.CovariatesSelectionSettings);
            _settingsMoq.Setup(m => m.CovariatesSelectionSettings).Returns(CovariatesSelectionMock.Object);
            _moqsDict.Add(typeof(CovariatesSelectionSettingsDto), CovariatesSelectionMock);

            var DietaryIntakeCalculationMock = new Mock<DietaryIntakeCalculationSettingsDto>().SetupAllProperties();
            setSettings(DietaryIntakeCalculationMock, Project.DietaryIntakeCalculationSettings);
            _settingsMoq.Setup(m => m.DietaryIntakeCalculationSettings).Returns(DietaryIntakeCalculationMock.Object);
            _moqsDict.Add(typeof(DietaryIntakeCalculationSettingsDto), DietaryIntakeCalculationMock);

            var EffectModelMock = new Mock<EffectModelSettingsDto>().SetupAllProperties();
            setSettings(EffectModelMock, Project.EffectModelSettings);
            _settingsMoq.Setup(m => m.EffectModelSettings).Returns(EffectModelMock.Object);
            _moqsDict.Add(typeof(EffectModelSettingsDto), EffectModelMock);

            var FrequencyModelMock = new Mock<FrequencyModelSettingsDto>().SetupAllProperties();
            setSettings(FrequencyModelMock, Project.FrequencyModelSettings);
            _settingsMoq.Setup(m => m.FrequencyModelSettings).Returns(FrequencyModelMock.Object);
            _moqsDict.Add(typeof(FrequencyModelSettingsDto), FrequencyModelMock);

            var HumanMonitoringMock = new Mock<HumanMonitoringSettingsDto>().SetupAllProperties();
            setSettings(HumanMonitoringMock, Project.HumanMonitoringSettings);
            _settingsMoq.Setup(m => m.HumanMonitoringSettings).Returns(HumanMonitoringMock.Object);
            _moqsDict.Add(typeof(HumanMonitoringSettingsDto), HumanMonitoringMock);

            var IntakeModelMock = new Mock<IntakeModelSettingsDto>().SetupAllProperties();
            setSettings(IntakeModelMock, Project.IntakeModelSettings);
            _settingsMoq.Setup(m => m.IntakeModelSettings).Returns(IntakeModelMock.Object);
            _moqsDict.Add(typeof(IntakeModelSettingsDto), IntakeModelMock);

            var KineticModelMock = new Mock<KineticModelSettingsDto>().SetupAllProperties();
            setSettings(KineticModelMock, Project.KineticModelSettings);
            _settingsMoq.Setup(m => m.KineticModelSettings).Returns(KineticModelMock.Object);
            _moqsDict.Add(typeof(KineticModelSettingsDto), KineticModelMock);

            var MixtureSelectionMock = new Mock<MixtureSelectionSettingsDto>().SetupAllProperties();
            setSettings(MixtureSelectionMock, Project.MixtureSelectionSettings);
            _settingsMoq.Setup(m => m.MixtureSelectionSettings).Returns(MixtureSelectionMock.Object);
            _moqsDict.Add(typeof(MixtureSelectionSettingsDto), MixtureSelectionMock);

            var NonDietaryMock = new Mock<NonDietarySettingsDto>().SetupAllProperties();
            setSettings(NonDietaryMock, Project.NonDietarySettings);
            _settingsMoq.Setup(m => m.NonDietarySettings).Returns(NonDietaryMock.Object);
            _moqsDict.Add(typeof(NonDietarySettingsDto), NonDietaryMock);

            var ScenarioAnalysisMock = new Mock<ScenarioAnalysisSettingsDto>().SetupAllProperties();
            setSettings(ScenarioAnalysisMock, Project.ScenarioAnalysisSettings);
            _settingsMoq.Setup(m => m.ScenarioAnalysisSettings).Returns(ScenarioAnalysisMock.Object);
            _moqsDict.Add(typeof(ScenarioAnalysisSettingsDto), ScenarioAnalysisMock);

            var ScreeningMock = new Mock<ScreeningSettingsDto>().SetupAllProperties();
            setSettings(ScreeningMock, Project.ScreeningSettings);
            _settingsMoq.Setup(m => m.ScreeningSettings).Returns(ScreeningMock.Object);
            _moqsDict.Add(typeof(ScreeningSettingsDto), ScreeningMock);

            var UnitVariabilityMock = new Mock<UnitVariabilitySettingsDto>().SetupAllProperties();
            setSettings(UnitVariabilityMock, Project.UnitVariabilitySettings);
            _settingsMoq.Setup(m => m.UnitVariabilitySettings).Returns(UnitVariabilityMock.Object);
            _moqsDict.Add(typeof(UnitVariabilitySettingsDto), UnitVariabilityMock);

            var UncertaintyAnalysisMock = new Mock<UncertaintyAnalysisSettingsDto>().SetupAllProperties();
            setSettings(UncertaintyAnalysisMock, Project.UncertaintyAnalysisSettings);
            _settingsMoq.Setup(m => m.UncertaintyAnalysisSettings).Returns(UncertaintyAnalysisMock.Object);
            _moqsDict.Add(typeof(UncertaintyAnalysisSettingsDto), UncertaintyAnalysisMock);

            var OutputDetailMock = new Mock<OutputDetailSettingsDto>().SetupAllProperties();
            setSettings(OutputDetailMock, Project.OutputDetailSettings);
            _settingsMoq.Setup(m => m.OutputDetailSettings).Returns(OutputDetailMock.Object);
            _moqsDict.Add(typeof(OutputDetailSettingsDto), OutputDetailMock);

            var MonteCarloMock = new Mock<MonteCarloSettingsDto>().SetupAllProperties();
            setSettings(MonteCarloMock, Project.MonteCarloSettings);
            _settingsMoq.Setup(m => m.MonteCarloSettings).Returns(MonteCarloMock.Object);
            _moqsDict.Add(typeof(MonteCarloSettingsDto), MonteCarloMock);

            _settingsMoq.Setup(m => m.ActionType).Returns(Project.ActionType);

            _settingsMoq.Setup(m => m.StandardActionCode).Returns(Project.StandardActionCode);

            _settingsMoq.Setup(m => m.ShortOutputTemplate).Returns(Project.ShortOutputTemplate);

            _settingsMoq.Setup(m => m.CalculationActionTypes).Returns(Project.CalculationActionTypes);

            _settingsMoq.Setup(m => m.ScopeKeysFilters).Returns(Project.ScopeKeysFilters);

            _settingsMoq.Setup(m => m.LoopScopingTypes).Returns(Project.LoopScopingTypes);

            _settingsMoq.Setup(m => m.SelectedCompounds).Returns(Project.SelectedCompounds);

            _settingsMoq.Setup(m => m.FocalFoods).Returns(Project.FocalFoods);

            _settingsMoq.Setup(m => m.LocationSubsetDefinition).Returns(Project.LocationSubsetDefinition);

            _settingsMoq.Setup(m => m.PeriodSubsetDefinition).Returns(Project.PeriodSubsetDefinition);

            _settingsMoq.Setup(m => m.SamplesSubsetDefinitions).Returns(Project.SamplesSubsetDefinitions);

            _settingsMoq.Setup(m => m.IndividualsSubsetDefinitions).Returns(Project.IndividualsSubsetDefinitions);

            _settingsMoq.Setup(m => m.FoodAsEatenSubset).Returns(Project.FoodAsEatenSubset);

            _settingsMoq.Setup(m => m.ModelledFoodSubset).Returns(Project.ModelledFoodSubset);

            _settingsMoq.Setup(m => m.FocalFoodAsEatenSubset).Returns(Project.FocalFoodAsEatenSubset);

            _settingsMoq.Setup(m => m.FocalFoodAsMeasuredSubset).Returns(Project.FocalFoodAsMeasuredSubset);

            _settingsMoq.Setup(m => m.SelectedScenarioAnalysisFoods).Returns(Project.SelectedScenarioAnalysisFoods);

        }
    }
}
