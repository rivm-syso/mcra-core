using MCRA.General;
using MCRA.Simulation.Calculators.IntakeModelling.IntakeModels;
using System.Collections.Generic;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Beta Binomial Normal model for chronic exposure assessment.
    /// </summary>
    public class BBNModel : UncorrelatedIntakeModel<BetaBinomialFrequencyModel, NormalAmountsModel> {

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="frequencyModelSettings"></param>
        /// <param name="amountModelSettings"></param>
        /// <param name="predictionLevels"></param>
        public BBNModel(
            IIntakeModelCalculationSettings frequencyModelSettings,
            IIntakeModelCalculationSettings amountModelSettings,
            List<double> predictionLevels = null
        ) : base(
            frequencyModelSettings,
            amountModelSettings,
            predictionLevels
        ) { }

        /// <summary>
        /// The exposure model type.
        /// </summary>
        public override IntakeModelType IntakeModelType {
            get {
                return IntakeModelType.BBN;
            }
        }
    }
}
