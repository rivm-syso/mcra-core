namespace MCRA.General.Action.Settings {

    public class FocalFood {
        public virtual string CodeFood { get; set; }

        public virtual string CodeSubstance { get; set; }

        public override string ToString() {
            return base.ToString()[1..].ToLower();
        }
    }
}
