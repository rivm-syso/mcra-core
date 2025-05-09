﻿using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ProcessingType : StrongEntity {

        private string _description;

        private readonly string _unspecified = "F28.A07XD";
        public ProcessingType() {
        }

        public ProcessingType(string code) {
            Code = code;
        }

        public override string Description {
            get => !string.IsNullOrEmpty(_description) ? _description : Code;
            set => _description = value;
        }

        public bool IsBulkingBlending { get; set; }
        public ProcessingDistributionType DistributionType { get; set; } = ProcessingDistributionType.LogisticNormal;

        public bool IsUnspecified => Code.Equals(_unspecified, StringComparison.OrdinalIgnoreCase);
    }
}
