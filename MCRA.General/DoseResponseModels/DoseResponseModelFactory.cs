namespace MCRA.General.DoseResponseModels {
    public sealed class DoseResponseModelFactory {
        public static IDoseResponseModelFunction Create(DoseResponseModelType modelType) {
            return modelType switch {
                DoseResponseModelType.Expm1 => new ExponentialModel1(),
                DoseResponseModelType.Expm2 => new ExponentialModel2(),
                DoseResponseModelType.Expm3 => new ExponentialModel3(),
                DoseResponseModelType.Expm4 => new ExponentialModel4(),
                DoseResponseModelType.Expm5 => new ExponentialModel5(),
                DoseResponseModelType.Hillm1 => new HillModel1(),
                DoseResponseModelType.Hillm2 => new HillModel2(),
                DoseResponseModelType.Hillm3 => new HillModel3(),
                DoseResponseModelType.Hillm4 => new HillModel4(),
                DoseResponseModelType.Hillm5 => new HillModel5(),
                DoseResponseModelType.TwoStage => new TwoStageModel(),
                DoseResponseModelType.LogLogist => new LogLogistModel(),
                DoseResponseModelType.Weibull => new WeibullModel(),
                DoseResponseModelType.LogProb => new LogProbModel(),
                DoseResponseModelType.Gamma => new GammaModel(),
                DoseResponseModelType.Logistic => new LogisticModel(),
                DoseResponseModelType.Probit => new ProbitModel(),
                DoseResponseModelType.LVM_Exp_M2 => new ExponentialLatentVariableModel2(),
                DoseResponseModelType.LVM_Exp_M3 => new ExponentialLatentVariableModel3(),
                DoseResponseModelType.LVM_Exp_M4 => new ExponentialLatentVariableModel4(),
                DoseResponseModelType.LVM_Exp_M5 => new ExponentialLatentVariableModel5(),
                DoseResponseModelType.LVM_Hill_M2 => new HillLatentVariableModel2(),
                DoseResponseModelType.LVM_Hill_M3 => new HillLatentVariableModel3(),
                DoseResponseModelType.LVM_Hill_M4 => new HillLatentVariableModel4(),
                DoseResponseModelType.LVM_Hill_M5 => new HillLatentVariableModel5(),
                DoseResponseModelType.Unknown => null,
                _ => throw new NotImplementedException(),
            };
        }

        public static IDoseResponseModelFunction Create(DoseResponseModelType modelType, IDictionary<string, double> parameters) {
            var model = Create(modelType);
            model?.Init(parameters);
            return model;
        }

        public static IDoseResponseModelFunction Create(DoseResponseModelType modelType, IDictionary<string, double> parameters, double bmd, double bmr) {
            var model = Create(modelType);
            model.Init(parameters, bmd, bmr);
            return model;
        }
    }
}
