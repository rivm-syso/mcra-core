﻿using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.TargetExposurePercentiles)]
    public sealed class RawTargetExposurePercentile : IRawDataTableRecord {
        public string idTargetExposureModel { get; set; }
        public double Percentage { get; set; }
        public double Exposure { get; set; }
    }
}