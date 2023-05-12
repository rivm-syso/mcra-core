namespace MCRA.General.Action.Settings.Dto {

    public class MixtureSelectionSettingsDto {

        public virtual ExposureApproachType ExposureApproachType { get; set; } = ExposureApproachType.RiskBased;

        public virtual int K { get; set; } = 4;

        public virtual double SW { get; set; } = 0.2D;

        public virtual int NumberOfIterations { get; set; } = 1000;

        public virtual int RandomSeed { get; set; } = 0;

        public virtual double Epsilon { get; set; } = 1e-3;

        public virtual double RatioCutOff { get; set; } = 1;

        public virtual double TotalExposureCutOff { get; set; }

        public virtual bool AutomaticallyDeterminationOfClusters { get; set; } = true;

        public virtual int NumberOfClusters { get; set; } = 4;

        public virtual ClusterMethodType ClusterMethodType { get; set; } = ClusterMethodType.NoClustering;

        public virtual NetworkAnalysisType NetworkAnalysisType { get; set; } = NetworkAnalysisType.NoNetworkAnalysis;

        // MCR Analysis

        public virtual bool IsMcrAnalysis { get; set; }

        public virtual ExposureApproachType McrExposureApproachType { get; set; } = ExposureApproachType.RiskBased;
        public virtual bool IsLogTransform { get; set; }

    }
}
