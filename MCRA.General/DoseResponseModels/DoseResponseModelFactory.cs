namespace MCRA.General.DoseResponseModels {
    public sealed class DoseResponseModelFactory {
        public static IDoseResponseModelFunction Create(DoseResponseModelType modelType) {
            switch (modelType) {
                case DoseResponseModelType.Expm1:
                    return new ExponentialModel1();
                case DoseResponseModelType.Expm2:
                    return new ExponentialModel2();
                case DoseResponseModelType.Expm3:
                    return new ExponentialModel3();
                case DoseResponseModelType.Expm4:
                    return new ExponentialModel4();
                case DoseResponseModelType.Expm5:
                    return new ExponentialModel5();
                case DoseResponseModelType.Hillm1:
                    return new HillModel1();
                case DoseResponseModelType.Hillm2:
                    return new HillModel2();
                case DoseResponseModelType.Hillm3:
                    return new HillModel3();
                case DoseResponseModelType.Hillm4:
                    return new HillModel4();
                case DoseResponseModelType.Hillm5:
                    return new HillModel5();
                case DoseResponseModelType.TwoStage:
                    return new TwoStageModel();
                case DoseResponseModelType.LogLogist:
                    return new LogLogistModel();
                case DoseResponseModelType.Weibull:
                    return new WeibullModel();
                case DoseResponseModelType.LogProb:
                    return new LogProbModel();
                case DoseResponseModelType.Gamma:
                    return new GammaModel();
                case DoseResponseModelType.Logistic:
                    return new LogisticModel();
                case DoseResponseModelType.Probit:
                    return new ProbitModel();
                case DoseResponseModelType.LVM_Exp_M2:
                    return new ExponentialLatentVariableModel2();
                case DoseResponseModelType.LVM_Exp_M3:
                    return new ExponentialLatentVariableModel3();
                case DoseResponseModelType.LVM_Exp_M4:
                    return new ExponentialLatentVariableModel4();
                case DoseResponseModelType.LVM_Exp_M5:
                    return new ExponentialLatentVariableModel5();
                case DoseResponseModelType.LVM_Hill_M2:
                    return new HillLatentVariableModel2();
                case DoseResponseModelType.LVM_Hill_M3:
                    return new HillLatentVariableModel3();
                case DoseResponseModelType.LVM_Hill_M4:
                    return new HillLatentVariableModel4();
                case DoseResponseModelType.LVM_Hill_M5:
                    return new HillLatentVariableModel5();
                case DoseResponseModelType.Unknown:
                    return null;
                default:
                    throw new NotImplementedException();
            }
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
