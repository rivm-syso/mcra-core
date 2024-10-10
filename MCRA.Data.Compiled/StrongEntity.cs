using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Compiled {
    public abstract class StrongEntity {
        private string _name;

        public virtual string Code { get; set; }

        public string Name {
            get => string.IsNullOrEmpty(_name) ? Code : _name;
            set => _name = value;
        }

        public virtual string Description { get; set; }

        public override string ToString() => $"[{GetHashCode():X8}] {Code}";

        public override int GetHashCode() => Code?.GetChecksum() ?? base.GetHashCode();
    }

    public sealed class ScopeEntity : StrongEntity {
        public ScopeEntity() {
        }

        public ScopeEntity(string code) {
            Code = code;
            Name = code;
        }

        public ScopeEntity(string code, string name) {
            Code = code;
            Name = name;
        }

        public bool IsSelected { get; set; }
        public bool IsInSource { get; set; }
    }
}
