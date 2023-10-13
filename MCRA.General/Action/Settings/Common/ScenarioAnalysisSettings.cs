namespace MCRA.General.Action.Settings {

    public class ScenarioAnalysisSettings {

        public virtual bool UseScenario { get; set; }

        public virtual bool IsMrlSettingScenario { get; set; }

        public virtual double ProposedMrl { get; set; }

        public virtual string CodeCompound { get; set; }

        public virtual double UseFrequency { get; set; }

        public virtual ConcentrationModelType ConcentrationModelType { get; set; }

    }
}
