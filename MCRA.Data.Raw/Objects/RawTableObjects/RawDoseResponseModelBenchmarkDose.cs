﻿using MCRA.General;
using System.Globalization;

namespace MCRA.Data.Raw.Objects.RawTableObjects {

    [RawDataSourceTableID(RawDataSourceTableID.DoseResponseModelBenchmarkDoses)]
    public sealed class RawDoseResponseModelBenchmarkDose : IRawDataTableRecord {
        public string idDoseResponseModel { get; set; }
        public string idSubstance { get; set; }
        public string Covariates { get; set; }
        public double BenchmarkResponse { get; set; }
        public double BenchmarkDose { get; set; }
        public double? BenchmarkDoseLower { get; set; }
        public double? BenchmarkDoseUpper { get; set; }
        public double? Rpf { get; set; }
        public double? RpfLower { get; set; }
        public double? RpfUpper { get; set; }
        public string ModelParameterValues { get; set; }

        public Dictionary<string, double> GetParameterValuesDict() {
            return ModelParameterValues?
                    .Split(',').Select(c => c.Split('='))
                    .Where(r => r.Any())
                    .ToDictionary(r => r[0], r => Convert.ToDouble(r[1], CultureInfo.InvariantCulture));
        }
    }
}
