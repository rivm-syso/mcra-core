namespace MCRA.Data.Compiled.Objects {
    public sealed class OccupationalTask {
        private string _name;
        public string Code { get; set; }
        public string Name {
            get => string.IsNullOrEmpty(_name) ? Code : _name;
            set => _name = value;
        }
        public string Description { get; set; }
    }
}
