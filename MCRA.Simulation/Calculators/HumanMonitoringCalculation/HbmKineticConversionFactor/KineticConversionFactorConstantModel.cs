﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmKineticConversionFactor {
    public sealed class KineticConversionFactorConstantModel : KineticConversionFactorModelBase {

        internal class ConstantModelParametrisation : KineticConversionFactorModelParametrisationBase {
            public double Factor { get; set; }
        }

        public KineticConversionFactorConstantModel(KineticConversionFactor conversion) : base(conversion) {
        }

        public override void CalculateParameters() {
            //First, check whether subgroups are available and use individual properties as keys for lookup
            if (ConversionRule.KCFSubgroups.Any()) {
                foreach (var sg in ConversionRule.KCFSubgroups) {
                    ModelParametrisations.Add(
                        new ConstantModelParametrisation() {
                            Age = sg.AgeLower,
                            Gender = sg.Gender,
                            Factor = sg.ConversionFactor
                        }
                    );
                }
            }
            //This is the default, no individual properties are needed.
            if (!ModelParametrisations.Any(r => r.Age == null && r.Gender == GenderType.Undefined)) {
                ModelParametrisations.Add(
                    new ConstantModelParametrisation() {
                        Age = null,
                        Gender = GenderType.Undefined,
                        Factor = ConversionRule.ConversionFactor
                    }
                );
            }
        }

        public override double Draw(IRandom random, double? age, GenderType gender) {
            Func<IKineticConversionFactorModelParametrisation, IRandom, double> drawFunction =
                (param, random) => {
                    var constParams = param as ConstantModelParametrisation;
                    return constParams.Factor;
                };
            return drawForParametrisation(random, age, gender, drawFunction);
        }
    }
}