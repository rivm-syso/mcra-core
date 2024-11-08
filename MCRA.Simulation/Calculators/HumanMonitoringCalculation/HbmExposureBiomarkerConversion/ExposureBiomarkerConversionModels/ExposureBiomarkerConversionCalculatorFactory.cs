﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion.ExposureBiomarkerConversionModels {
    public class ExposureBiomarkerConversionCalculatorFactory {

        public static IExposureBiomarkerConversionModel Create(
            ExposureBiomarkerConversion conversion,
            bool useSubgroups
        ) {

            IExposureBiomarkerConversionModel model = null;
            if (!conversion.VariabilityUpper.HasValue) {
                model = new ExposureBiomarkerConversionConstantModel(conversion, useSubgroups);
                model.CalculateParameters();
                return model;
            }
            switch (conversion.Distribution) {
                case BiomarkerConversionDistribution.Unspecified:
                    model = new ExposureBiomarkerConversionConstantModel(conversion, useSubgroups);
                    break;
                case BiomarkerConversionDistribution.LogNormal:
                    model = new ExposureBiomarkerConversionLogNormalModel(conversion, useSubgroups);
                    break;
                case BiomarkerConversionDistribution.Uniform:
                    model = new ExposureBiomarkerConversionUniformModel(conversion, useSubgroups);
                    break;
                case BiomarkerConversionDistribution.Beta:
                    model = new ExposureBiomarkerConversionBetaModel(conversion, useSubgroups);
                    break;
            }
            model.CalculateParameters();
            return model;
        }
    }
}
