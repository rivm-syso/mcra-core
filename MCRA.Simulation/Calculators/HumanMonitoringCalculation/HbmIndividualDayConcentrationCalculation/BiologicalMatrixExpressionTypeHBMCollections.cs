using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation {
    public sealed class BiologicalMatrixExpressionTypeHBMCollections {

        public BiologicalMatrix BiologicalMatrix { get; set; }
        public ExpressionType ExpressionType { get; set; }

        public ICollection<HbmIndividualDayConcentration> HbmIndividualDayConcentrationCollections { get; set; }
    }
}
