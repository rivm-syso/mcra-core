﻿using MCRA.General;

namespace MCRA.Data.Raw.Objects.RawObjects {
    [RawDataSourceTableID(RawDataSourceTableID.FacetDescriptors)]
    public class RawFacetDescriptor : IRawDataTableRecord {
        public string idFacetDescriptor { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}