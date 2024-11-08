﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SubGroupComponentSummaryRecord {
        [Description("Subgroup.")]
        [DisplayName("Subgroup")]
        public int ClusterId { get; set; }

        [Description("Component.")]
        [DisplayName("Component")]
        public int ComponentNumber { get; set; }

        [Description("Relative exposure of each component to subgroup.")]
        [DisplayName("Relative exposure subgroup(%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set; }

        [Description("Relative exposure of each component to population.")]
        [DisplayName("Relative exposure population (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentageAll { get; set; }

        [Description("Number of individuals.")]
        [DisplayName("Number of individuals")]
        public int NumberOfIndividuals { get; set; }
    }
}
