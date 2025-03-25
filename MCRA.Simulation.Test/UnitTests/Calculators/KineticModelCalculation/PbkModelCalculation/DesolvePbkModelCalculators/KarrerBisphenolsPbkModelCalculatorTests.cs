using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators.KarrerKineticModelCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation.PbkModelCalculation.DesolvePbkModelCalculators {

    [TestClass]
    public class KarrerBisphenolsPbkModelCalculatorTests : DesolvePbkModelCalculatorBaseTests {
        protected override KineticModelInstance getDefaultInstance(params Compound[] substance) {
            var instance = createFakeModelInstance(substance.Single());
            return instance;
        }

        protected override PbkModelCalculatorBase createCalculator(
            KineticModelInstance instance,
            PbkSimulationSettings simulationSettings
        ) {
            var calculator = new KarrerReImplementedKineticModelCalculator(instance, simulationSettings);
            return calculator;
        }

        protected override TargetUnit getDefaultInternalTarget() {
            return TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Urine);
        }

        protected override TargetUnit getDefaultExternalTarget() {
            return TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
        }

        [TestMethod]
        [DataRow(ExposureRoute.Oral)]
        public override void TestForwardAcute(ExposureRoute route) {
            testForwardAcute(route);
        }

        [TestMethod]
        [DataRow(ExposureRoute.Oral)]
        public override void TestForwardChronic(ExposureRoute route) {
            testForwardChronic(route);
        }

        private static KineticModelInstance createFakeModelInstance(Compound substance) {
            var idModelDefinition = "EuroMix_Bisphenols_PBPK_model_V2";
            var idModelInstance = $"{idModelDefinition}-{substance.Code}";
            var kineticModelParameters = new List<KineticModelInstanceParameter>();

            void addParam(string name, double value) {
                kineticModelParameters.Add(new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = name,
                    Value = value
                });
            }

            addParam("BW", 60);
            addParam("age", 35);
            addParam("gender", 1);

            addParam("QCC", 5.9);
            addParam("QgonadC", 0.000178);
            addParam("QliverC", 0.23);
            addParam("QfatC", 0.044);
            addParam("QbrainC", 0.11);
            addParam("QskinC", 0.044);
            addParam("QmuscleC", 0.19);

            addParam("QCC_adult_f", 5.9);
            addParam("QgonadC_adult_f", 0.00045);
            addParam("QliverC_adult_f", 0.23);
            addParam("QfatC_adult_f", 0.24);
            addParam("QbrainC_adult_f", 0.105041);
            addParam("QskinC_adult_f", 0.043767);
            addParam("QmuscleC_adult_f", 0.142429);

            addParam("QCC_adult_m", 5.9);
            addParam("QgonadC_adult_m", 0.00045);
            addParam("QliverC_adult_m", 0.23);
            addParam("QfatC_adult_m", 0.044);
            addParam("QbrainC_adult_m", 0.11);
            addParam("QskinC_adult_m", 0.044);
            addParam("QmuscleC_adult_m", 0.19);

            addParam("QCC_adolescent_f", 5.9);
            addParam("QgonadC_adolescent_f", 0.00045);
            addParam("QliverC_adolescent_f", 0.23);
            addParam("QfatC_adolescent_f", 0.24);
            addParam("QbrainC_adolescent_f", 0.105041);
            addParam("QskinC_adolescent_f", 0.043767);
            addParam("QmuscleC_adolescent_f", 0.142429);

            addParam("QCC_adolescent_m", 5.9);
            addParam("QgonadC_adolescent_m", 0.00045);
            addParam("QliverC_adolescent_m", 0.23);
            addParam("QfatC_adolescent_m", 0.24);
            addParam("QbrainC_adolescent_m", 0.105041);
            addParam("QskinC_adolescent_m", 0.043767);
            addParam("QmuscleC_adolescent_m", 0.142429);

            addParam("QCC_child_f", 5.9);
            addParam("QgonadC_child_f", 0.00045);
            addParam("QliverC_child_f", 0.23);
            addParam("QfatC_child_f", 0.24);
            addParam("QbrainC_child_f", 0.105041);
            addParam("QskinC_child_f", 0.043767);
            addParam("QmuscleC_child_f", 0.142429);

            addParam("QCC_child_m", 5.9);
            addParam("QgonadC_child_m", 0.00045);
            addParam("QliverC_child_m", 0.23);
            addParam("QfatC_child_m", 0.24);
            addParam("QbrainC_child_m", 0.105041);
            addParam("QskinC_child_m", 0.043767);
            addParam("QmuscleC_child_m", 0.142429);

            addParam("VplasmaC", 0.044);
            addParam("VfatC", 0.20);
            addParam("VliverC", 0.025);
            addParam("VbrainC", 0.020);
            addParam("VskinC", 0.045);
            addParam("VgonadC", 0.00048);
            addParam("VmuscleC", 0.54);
            addParam("VrichC", 0.054);
            addParam("VbodygC", 0.044);
            addParam("VbodysC", 0.044);

            addParam("VplasmaC_adult_f", 0.044);
            addParam("VfatC_adult_f", 0.20);
            addParam("VliverC_adult_f", 0.025);
            addParam("VbrainC_adult_f", 0.020);
            addParam("VskinC_adult_f", 0.045);
            addParam("VgonadC_adult_f", 0.00048);
            addParam("VmuscleC_adult_f", 0.54);
            addParam("VrichC_adult_f", 0.054);
            addParam("VbodygC_adult_f", 0.044);
            addParam("VbodysC_adult_f", 0.044);

            addParam("VplasmaC_adult_m", 0.044);
            addParam("VfatC_adult_m", 0.20);
            addParam("VliverC_adult_m", 0.025);
            addParam("VbrainC_adult_m", 0.020);
            addParam("VskinC_adult_m", 0.045);
            addParam("VgonadC_adult_m", 0.00048);
            addParam("VmuscleC_adult_m", 0.54);
            addParam("VrichC_adult_m", 0.054);
            addParam("VbodygC_adult_m", 0.044);
            addParam("VbodysC_adult_m", 0.044);

            addParam("VplasmaC_adolescent_f", 0.044);
            addParam("VfatC_adolescent_f", 0.20);
            addParam("VliverC_adolescent_f", 0.025);
            addParam("VbrainC_adolescent_f", 0.020);
            addParam("VskinC_adolescent_f", 0.045);
            addParam("VgonadC_adolescent_f", 0.00048);
            addParam("VmuscleC_adolescent_f", 0.54);
            addParam("VrichC_adolescent_f", 0.054);
            addParam("VbodygC_adolescent_f", 0.044);
            addParam("VbodysC_adolescent_f", 0.044);

            addParam("VplasmaC_adolescent_m", 0.044);
            addParam("VfatC_adolescent_m", 0.20);
            addParam("VliverC_adolescent_m", 0.025);
            addParam("VbrainC_adolescent_m", 0.020);
            addParam("VskinC_adolescent_m", 0.045);
            addParam("VgonadC_adolescent_m", 0.00048);
            addParam("VmuscleC_adolescent_m", 0.54);
            addParam("VrichC_adolescent_m", 0.054);
            addParam("VbodygC_adolescent_m", 0.044);
            addParam("VbodysC_adolescent_m", 0.044);

            addParam("VplasmaC_child_f", 0.044);
            addParam("VfatC_child_f", 0.20);
            addParam("VliverC_child_f", 0.025);
            addParam("VbrainC_child_f", 0.020);
            addParam("VskinC_child_f", 0.045);
            addParam("VgonadC_child_f", 0.00048);
            addParam("VmuscleC_child_f", 0.54);
            addParam("VrichC_child_f", 0.054);
            addParam("VbodygC_child_f", 0.044);
            addParam("VbodysC_child_f", 0.044);

            addParam("VplasmaC_child_m", 0.044);
            addParam("VfatC_child_m", 0.20);
            addParam("VliverC_child_m", 0.025);
            addParam("VbrainC_child_m", 0.020);
            addParam("VskinC_child_m", 0.045);
            addParam("VgonadC_child_m", 0.00048);
            addParam("VmuscleC_child_m", 0.54);
            addParam("VrichC_child_m", 0.054);
            addParam("VbodygC_child_m", 0.044);
            addParam("VbodysC_child_m", 0.044);

            addParam("pliver", 0.73);
            addParam("pfat", 5.0);
            addParam("pslow", 2.7);
            addParam("prich", 2.8);
            addParam("pgonad", 2.6);
            addParam("pbrain", 2.8);
            addParam("pskin", 2.15);

            addParam("geC", 3.5);
            addParam("k0C", 0);
            addParam("k1C", 2.1);
            addParam("k4C", 0);
            addParam("kGIingC", 50);
            addParam("kGIinsC", 50);

            addParam("kmgutg", 58400);
            addParam("vmaxgutgC", 361);
            addParam("fgutg", 1);
            addParam("kmguts", 0.001);
            addParam("vmaxgutsC", 0.001);
            addParam("fguts", 1);
            addParam("met1g", 0.9);
            addParam("met1s", 1);
            addParam("enterocytes", 0.1223);
            addParam("kmliver", 45800);
            addParam("vmaxliverC", 9043.2);
            addParam("fliverg", 1);
            addParam("kmlivers", 10100);
            addParam("vmaxliversC", 149);
            addParam("flivers", 1);

            addParam("EHRtime", 0);
            addParam("EHRrateC", 0.2);
            addParam("k4C_IV", 0);

            addParam("kurinebpaC", 0.06);
            addParam("kurinebpagC", 0.35);
            addParam("kurinebpasC", 0.03);
            addParam("vreabsorptiongC", 0);
            addParam("vreabsorptionsC", 0);
            addParam("kreabsorptiong", 9200);
            addParam("kreabsorptions", 9200);
            addParam("kenterobpagC", 0.2);
            addParam("kenterobpasC", 0);

            addParam("koa", 0);
            addParam("EoA_D", 0.178);
            addParam("aHL_D", 6);
            addParam("kda", 0);
            addParam("EoA_D2", 0.178);
            addParam("aHL_D2", 0.16);
            addParam("kda2", 0);

            addParam("ksiLiver", 0);
            addParam("ksiGut", 0);

            var kineticModel = new KineticModelInstance() {
                IdModelInstance = idModelInstance,
                KineticModelInstanceParameters = kineticModelParameters.ToDictionary(r => r.Parameter),
                KineticModelDefinition = MCRAKineticModelDefinitions.Definitions[idModelDefinition],
                KineticModelSubstances = [new() { Substance = substance }],
                IdModelDefinition = idModelDefinition,
                IdTestSystem = "Human",
            };
            return kineticModel;
        }
    }
}
