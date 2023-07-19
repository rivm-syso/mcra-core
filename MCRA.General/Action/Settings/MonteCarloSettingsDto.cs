namespace MCRA.General.Action.Settings {

    public class MonteCarloSettingsDto {

        public virtual int RandomSeed { get; set; } = 123456;

        public virtual int NumberOfMonteCarloIterations { get; set; } = 100000;

        public virtual bool IsSurveySampling { get; set; }
    }
}
