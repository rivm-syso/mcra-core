using System.Collections.Generic;

namespace MCRA.General.Action.Settings.Dto {

    public class IntakeModelSettingsDto {

        public virtual IntakeModelType IntakeModelType { get; set; } = IntakeModelType.OIM;

        public virtual TransformType TransformType { get; set; }

        public virtual int GridPrecision { get; set; } = 20;

        public virtual int NumberOfIterations { get; set; } = 5;

        public virtual bool SplineFit { get; set; }

        public virtual bool CovariateModelling { get; set; }

        public virtual bool FirstModelThenAdd { get; set; }

        public virtual double Dispersion { get; set; } = 0.0001D;

        public virtual double VarianceRatio { get; set; } = 1D;

        public virtual List<IntakeModelPerCategoryDto> IntakeModelsPerCategory { get; set; } = new List<IntakeModelPerCategoryDto>();
    }
}
