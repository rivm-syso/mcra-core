﻿using MCRA.General;
using MCRA.General.ModuleDefinitions.Interfaces;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Logistic normal normal model without correlation for chronic exposure assessment.
    /// </summary>
    public class LNN0Model : UncorrelatedIntakeModel<LogisticNormalFrequencyModel, NormalAmountsModel> {

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="frequencyModelSettings"></param>
        /// <param name="amountModelSettings"></param>
        /// <param name="predictionLevels"></param>
        public LNN0Model(
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
        public override IntakeModelType IntakeModelType => IntakeModelType.LNN0;
    }
}
