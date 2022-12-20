namespace MCRA.Data.Compiled.Objects {
    public interface IStrongEntity {
        string Code { get; }
        string Name { get; }
        string Description { get; }
    }

    public sealed class StrongEntity: IStrongEntity {

        public StrongEntity() {
        }

        public StrongEntity(string code) {
            Code = code;
            Name = code;
        }

        public StrongEntity(string code, string name) {
            Code = code;
            Name = name;
        }

        public string Code { get; set; }
        public string Name { get; set; }
        public string Description {
            get {
                return Name;
            }
        }

        public bool IsSelected { get; set; }
        public bool IsInSource { get; set; }
    }
}
