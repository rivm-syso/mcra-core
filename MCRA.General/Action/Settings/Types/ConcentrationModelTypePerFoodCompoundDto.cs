﻿namespace MCRA.General.Action.Settings {
    public class ConcentrationModelTypePerFoodCompoundDto {

        public virtual string CodeFood { get; set; }

        public virtual string CodeCompound { get; set; }

        public virtual ConcentrationModelType ConcentrationModelType { get; set; }
    }
}