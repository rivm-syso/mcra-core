﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using NCalc;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public static class ExposureResponseFunctionExtensions {

        public static double Compute(this ExposureResponseFunction erf, double x) {
            if (x <= erf.Baseline) {
                return 1D;
            }

            Expression erfSpecification;
            if (erf.HasErfSubGroups()) {
                var i = 0;
                while (i < erf.ErfSubgroups.Count && erf.ErfSubgroups.ElementAt(i).ExposureUpper < x) {
                    i++;
                }
                erfSpecification = erf.ErfSubgroups.ElementAt(i).ExposureResponseSpecification;
            } else {
                erfSpecification = erf.ExposureResponseSpecification;
            }

            if (erf.ExposureResponseType == ExposureResponseType.Function) {
                erfSpecification.Parameters["x"] = x;
                return (double)erfSpecification.Evaluate();
            } else if (erf.ExposureResponseType == ExposureResponseType.PerDoubling) {
                var doubFac = (double)erfSpecification.Evaluate();
                return Math.Pow(doubFac, Math.Log2(x / erf.Baseline));
            } else if (erf.ExposureResponseType == ExposureResponseType.PerUnit) {
                var a = (double)erfSpecification.Evaluate();
                var b = 1 - a * erf.Baseline;
                return a * x + b;
            } else if (erf.ExposureResponseType == ExposureResponseType.Constant) {
                return (double)erfSpecification.Evaluate();
            } else {
                throw new NotImplementedException();
            }
        }
    }
}

