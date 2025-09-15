namespace MCRA.Data.Compiled.Objects {
    public sealed class HbmSingleValueExposureSurvey {
        private string _name;
        
        public string Id { get; set; }
        public string Name {
            get {
                if (string.IsNullOrEmpty(_name)) {
                    return Id;
                }
                return _name;
            }
            set {
                _name = value;
            }
        }
        public string Description { get; set; }
        public string Country { get; set; }
    }
}
