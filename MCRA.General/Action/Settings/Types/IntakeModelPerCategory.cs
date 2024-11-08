using System.Xml.Serialization;

namespace MCRA.General.Action.Settings {
    public class IntakeModelPerCategory {

        public virtual IntakeModelType ModelType { get; set; } = IntakeModelType.LNN0;

        public virtual TransformType TransformType { get; set; }
        [XmlArrayItem("FoodCode")]
        public virtual List<string> FoodsAsMeasured { get; set; } = [];

    }
}
