using MCRA.General;

namespace MCRA.Simulation.Action {

    public sealed class ActionInputRequirement {
        public ActionType ActionType { get; set; }
        public bool IsRequired { get; set; }
        public bool IsVisible { get; set; }

        public override string ToString() {
            return $"{ActionType}: Req {(IsRequired ? "Y" : "N")} Vis {(IsVisible ? "Y" : "N")}";
        }
    }
}
