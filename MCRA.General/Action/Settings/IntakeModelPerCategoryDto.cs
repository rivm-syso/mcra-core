using MCRA.General;
using System.Collections.Generic;

namespace MCRA.General.Action.Settings.Dto {
    public class IntakeModelPerCategoryDto {

        public virtual IntakeModelType ModelType { get; set; } = IntakeModelType.LNN0;

        public virtual TransformType TransformType { get; set; }

        public virtual List<IntakeModelPerCategory_FoodAsMeasuredDto> FoodsAsMeasured { get; set; } = new List<IntakeModelPerCategory_FoodAsMeasuredDto>();

    }
}
