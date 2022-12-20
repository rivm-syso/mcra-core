using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ProcessingType: IStrongEntity {

        private string _name;
        private string _description;

        public ProcessingType() {
        }

        public ProcessingType(string code) : this() {
            Code = code;
        }

        public string Code { get; set; }

        public string Name {
            get {
                if (!string.IsNullOrEmpty(_name)) {
                    return _name;
                }
                return Description;
            }
            set {
                _name = value;
            }
        }

        public string Description {
            get {
                if (!string.IsNullOrEmpty(_description)) {
                    return _description;
                }
                return Code;
            }
            set {
                _description = value;
            }
        }

        public bool IsBulkingBlending { get; set; }

        public string DistributionTypeString { get; set; }

        public ProcessingDistributionType DistributionType {
            get {
                return ProcessingDistributionTypeConverter.FromString(DistributionTypeString, ProcessingDistributionType.LogisticNormal);
            }
            set {
                DistributionTypeString = value.ToString();
            }
        }
    }
}
