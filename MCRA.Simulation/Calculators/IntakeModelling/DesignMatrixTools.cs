using MCRA.General;
using MCRA.Utils;

namespace MCRA.Simulation.Calculators.IntakeModelling {
    public static class DesignMatrixTools {

        /// <summary>
        /// Returns all structures to fit the frequency model: design matrix for covariates X,
        /// Weights, Ybin, Nbin and Description.
        /// </summary>
        /// <param name="intakes"></param>
        /// <param name="covariateModel"></param>
        /// <returns></returns>
        public static FrequencyDataResult GetDMFrequency(
            this IEnumerable<IndividualFrequency> individualFrequencies,
            CovariateModelType covariateModel,
            int dfPol
        ) {
            var covariableName = "pol^";
            var data = new FrequencyDataResult() {
                DfPolynomial = dfPol,
                SimulatedIndividuals = individualFrequencies.Select(c => c.SimulatedIndividual).ToList(),
            };

            var du = new DesignUtils(); ;
            Polynomial polynomial = null;
            var orthPol = new OrthogonalPolynomial();

            var design = new List<double[]>();
            var constant = new List<double>();
            var covariable = new List<double>();
            var cofactor = new List<string>();
            var label = new List<string>();
            data.DesignMatrixDescription = [
                "constant"
            ];

            switch (covariateModel) {
                case CovariateModelType.Constant:

                    var freqConst = individualFrequencies
                        .GroupBy(fr => (fr.Frequency, fr.Nbinomial))
                        .Select(g => (
                            frequency: g.Key.Frequency,
                            nbinomial: (double)g.Key.Nbinomial,
                            //count = (double)g.Count(),
                            constant: 1D,
                            weight: g.Sum(c => c.SamplingWeight)
                        ))
                        .OrderBy(a => a.frequency)
                        .ToList();

                    constant = freqConst.Select(c => c.constant).ToList();

                    design.Add(constant.ToArray());
                    data.Weights = freqConst.Select(c => c.weight).ToList();
                    data.X = du.ConvertToDesignMatrix(design);
                    data.Ybin = freqConst.Select(c => c.frequency).ToList();
                    data.Nbin = freqConst.Select(c => c.nbinomial).ToList();

                    break;

                case CovariateModelType.Covariable:

                    var freqCovar = individualFrequencies
                          .GroupBy(fr => (fr.Frequency, fr.Covariable, fr.Nbinomial))
                          .Select(g => (
                              frequency: g.Key.Frequency,
                              covariable: g.Key.Covariable,
                              nbinomial: (double)g.Key.Nbinomial,
                              //count = (double)g.Count(),
                              constant: 1D,
                              weight: g.Sum(c => c.SamplingWeight)
                          ))
                          .OrderBy(a => a.frequency)
                          .ThenBy(a => a.covariable)
                          .ToList();

                    constant = freqCovar.Select(c => c.constant).ToList();
                    covariable = freqCovar.Select(c => c.covariable).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }

                    data.Weights = freqCovar.Select(c => c.weight).ToList();

                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, data.Weights);
                    design.Add(constant.ToArray());
                    design.AddRange(polynomial.Result);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Nbin = freqCovar.Select(c => c.nbinomial).ToList();
                    data.Ybin = freqCovar.Select(c => c.frequency).ToList();
                    data.Covariable = covariable;
                    data.PolynomialResult = polynomial;
                    break;

                case CovariateModelType.Cofactor:

                    var freqCofact = individualFrequencies
                    .GroupBy(fr => (fr.Frequency, fr.Cofactor, fr.Nbinomial))
                    .Select(g => (
                        frequency: g.Key.Frequency,
                        cofactor: g.Key.Cofactor,
                        nbinomial: (double)g.Key.Nbinomial,
                        //count = (double)g.Count(),
                        constant: 1D,
                        weight: g.Sum(c => c.SamplingWeight)
                    ))
                    .OrderBy(a => a.frequency)
                    .ThenBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                    constant = freqCofact.Select(c => c.constant).ToList();
                    cofactor = freqCofact.Select(c => c.cofactor).ToList();

                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    design.Add(constant.ToArray());
                    design.AddRange(du.MakeDummy(cofactor));

                    data.Weights = freqCofact.Select(c => c.weight).ToList();
                    data.X = du.ConvertToDesignMatrix(design);
                    data.Nbin = freqCofact.Select(c => c.nbinomial).ToList();
                    data.Ybin = freqCofact.Select(c => c.frequency).ToList();
                    data.Cofactor = cofactor;

                    break;

                case CovariateModelType.CovariableCofactor:

                    var freqCovarCofact = individualFrequencies
                    .GroupBy(fr => (fr.Frequency, fr.Cofactor, fr.Covariable, fr.Nbinomial))
                    .Select(g => (
                        frequency: g.Key.Frequency,
                        cofactor: g.Key.Cofactor,
                        covariable: g.Key.Covariable,
                        nbinomial: (double)g.Key.Nbinomial,
                        //count = (double)g.Count(),
                        constant: 1D,
                        weight: (double)g.Sum(c => c.SamplingWeight)
                    ))
                    .OrderBy(a => a.frequency)
                    .ThenBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.covariable)
                    .ToList();

                    constant = freqCovarCofact.Select(c => c.constant).ToList();
                    cofactor = freqCovarCofact.Select(c => c.cofactor).ToList();
                    covariable = freqCovarCofact.Select(c => c.covariable).ToList();
                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    data.Weights = freqCovarCofact.Select(c => c.weight).ToList();

                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, data.Weights);

                    design.Add(constant.ToArray());
                    design.AddRange(polynomial.Result);
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Nbin = freqCovarCofact.Select(c => c.nbinomial).ToList();
                    data.Ybin = freqCovarCofact.Select(c => c.frequency).ToList();
                    data.Covariable = covariable;
                    data.Cofactor = cofactor;
                    data.PolynomialResult = polynomial;

                    break;

                case CovariateModelType.CovariableCofactorInteraction:

                    var freqCovarCofactInt = individualFrequencies
                    .GroupBy(fr => (fr.Frequency, fr.Cofactor, fr.Covariable, fr.Nbinomial))
                    .Select(g => (
                        frequency: g.Key.Frequency,
                        cofactor: g.Key.Cofactor,
                        covariable: g.Key.Covariable,
                        nbinomial: (double)g.Key.Nbinomial,
                        //count = (double)g.Count(),
                        constant: 1D,
                        weight: (double)g.Sum(c => c.SamplingWeight)
                    ))
                    .OrderBy(a => a.frequency)
                    .ThenBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.covariable)
                    .ToList();


                    constant = freqCovarCofactInt.Select(c => c.constant).ToList();
                    cofactor = freqCovarCofactInt.Select(c => c.cofactor).ToList();
                    covariable = freqCovarCofactInt.Select(c => c.covariable).ToList();
                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }
                    foreach (var item in label.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescription.Add("level " + item + " * " + covariableName + ix);
                        }
                    }
                    data.Weights = freqCovarCofactInt.Select(c => c.weight).ToList();

                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, data.Weights);

                    design.Add(constant.ToArray());
                    design.AddRange(polynomial.Result);
                    design.AddRange(du.MakeDummy(cofactor));
                    design.AddRange(du.MakeInteraction(cofactor, polynomial.Result, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Nbin = freqCovarCofactInt.Select(c => c.nbinomial).ToList();
                    data.Ybin = freqCovarCofactInt.Select(c => c.frequency).ToList();
                    data.Covariable = covariable;
                    data.Cofactor = cofactor;
                    data.PolynomialResult = polynomial;

                    break;

                default:
                    break;
            }

            return data;
        }

        /// <summary>
        ///  Returns all structures to form conditional predictions for groups
        ///  (e.g the combination of covariates): design matrix for covariates (X),
        ///  group counts and Description. Total length of structures is equal to
        ///  the number of combinations of covariates.
        /// </summary>
        /// <param name="individualFrequencies"></param>
        /// <param name="covariateModel"></param>
        /// <param name="fdr"></param>
        /// <param name="predictions"></param>
        /// <returns></returns>
        public static FrequencyDataResult GetDMSpecifiedPredictions(
            this IEnumerable<IndividualFrequency> individualFrequencies,
            CovariateModelType covariateModel,
            FrequencyDataResult fdr,
            List<double> predictions
        ) {

            var covariableName = "pol^";
            int dfPol = fdr.DfPolynomial;

            var data = new FrequencyDataResult();

            DesignUtils du = new DesignUtils(); ;

            var pol = new List<double[]>();
            var design = new List<double[]>();
            var factorLevels = new List<string>();
            data.DesignMatrixDescription = [
                "constant"
            ];
            int combinations;
            var covariableExtended = new List<double>();
            var cofactorExtended = new List<string>();

            switch (covariateModel) {
                case CovariateModelType.Constant:
                    design.Add([1]);
                    data.X = du.ConvertToDesignMatrix(design);
                    data.GroupCounts = new int[1].ToList();
                    break;

                case CovariateModelType.Covariable:

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(predictions, dfPol);
                    design.Add(Enumerable.Repeat(1D, predictions.Count).ToArray());
                    design.AddRange(pol);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = predictions;
                    data.GroupCounts = new int[predictions.Count].ToList();
                    break;

                case CovariateModelType.Cofactor:
                    factorLevels = individualFrequencies
                        .Select(c => c.Cofactor)
                        .Distinct()
                        .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    design.Add(Enumerable.Repeat(1D, factorLevels.Count).ToArray());
                    design.AddRange(du.MakeDummy(factorLevels));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactor = factorLevels.ToList();
                    data.GroupCounts = new int[factorLevels.Count].ToList();

                    break;

                case CovariateModelType.CovariableCofactor:
                    factorLevels = individualFrequencies
                        .Select(c => c.Cofactor)
                        .Distinct()
                        .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    combinations = factorLevels.Count * predictions.Count;

                    for (int i = 0; i < factorLevels.Count; i++) {
                        for (int j = 0; j < predictions.Count; j++) {
                            cofactorExtended.Add(factorLevels[i]);
                            covariableExtended.Add(predictions[j]);
                        }
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariableExtended, dfPol);

                    design.Add(Enumerable.Repeat(1D, combinations).ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactorExtended));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariableExtended;
                    data.Cofactor = cofactorExtended;
                    data.GroupCounts = new int[combinations].ToList();

                    break;

                case CovariateModelType.CovariableCofactorInteraction:
                    factorLevels = individualFrequencies
                        .Select(c => c.Cofactor)
                        .Distinct()
                        .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescription.Add("level " + item + " * " + covariableName + ix);
                        }
                    }
                    combinations = factorLevels.Count * predictions.Count;
                    for (int i = 0; i < factorLevels.Count; i++) {
                        for (int j = 0; j < predictions.Count; j++) {
                            cofactorExtended.Add(factorLevels[i]);
                            covariableExtended.Add(predictions[j]);
                        }
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariableExtended, dfPol);

                    design.Add(Enumerable.Repeat(1D, combinations).ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactorExtended));
                    design.AddRange(du.MakeInteraction(cofactorExtended, pol, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariableExtended;
                    data.Cofactor = cofactorExtended;
                    data.GroupCounts = new int[combinations].ToList();

                    break;
                default:
                    break;
            }

            return data;
        }

        /// <summary>
        ///  Returns all structures to form conditional predictions for groups (e.g the combination of covariates): design matrix for covariates (X), group counts
        ///  and Description. Total length of structures is equal to the number of combinations of covariates.
        /// </summary>
        /// <param name="individualFrequencies"></param>
        /// <param name="covariateModel"></param>
        /// <param name="fdr"></param>
        /// <returns></returns>
        public static FrequencyDataResult GetDMConditionalPredictions(
            this IEnumerable<IndividualFrequency> individualFrequencies,
            CovariateModelType covariateModel,
            FrequencyDataResult fdr
        ) {
            var covariableName = "pol^";
            int dfPol = fdr.DfPolynomial;

            var data = new FrequencyDataResult();
            var du = new DesignUtils(); ;

            var pol = new List<double[]>();
            var design = new List<double[]>();
            var constant = new List<double>();
            var covariable = new List<double>();
            var cofactor = new List<string>();
            var count = new List<int>();
            var label = new List<string>();
            data.DesignMatrixDescription = [
                "constant"
            ];

            switch (covariateModel) {
                case CovariateModelType.Constant:
                    constant.Add(1);
                    count.Add(individualFrequencies.Count());
                    design.Add(constant.ToArray());
                    data.X = du.ConvertToDesignMatrix(design);
                    data.GroupCounts = count;
                    break;

                case CovariateModelType.Covariable:

                    var freqCovar = individualFrequencies
                          .GroupBy(fr => fr.Covariable)
                          .Select(g => (
                              covariable: g.Key,
                              count: g.Count(),
                              constant: 1D
                          //sampleWeightCount = (double)g.Sum(c => c.SampleWeight),
                          ))
                          .OrderBy(a => a.covariable)
                          .ToList();

                    constant = freqCovar.Select(c => c.constant).ToList();
                    covariable = freqCovar.Select(c => c.covariable).ToList();
                    count = freqCovar.Select(c => c.count).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.Add(constant.ToArray());
                    design.AddRange(pol);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.Cofactor:

                    var freqCofact = individualFrequencies
                    .GroupBy(fr => fr.Cofactor)
                    .Select(g => (
                        cofactor: g.Key,
                        count: g.Count(),
                        constant: 1D
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                    constant = freqCofact.Select(c => c.constant).ToList();
                    cofactor = freqCofact.Select(c => c.cofactor).ToList();
                    count = freqCofact.Select(c => c.count).ToList();

                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    design.Add(constant.ToArray());
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactor = cofactor;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.CovariableCofactor:

                    var freqCovarCofact = individualFrequencies
                    .GroupBy(fr => (fr.Cofactor, fr.Covariable))
                    .Select(g => (
                        cofactor: g.Key.Cofactor,
                        covariable: g.Key.Covariable,
                        count: g.Count(),
                        constant: 1D
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.covariable)
                    .ToList();

                    constant = freqCovarCofact.Select(c => c.constant).ToList();
                    cofactor = freqCovarCofact.Select(c => c.cofactor).ToList();
                    covariable = freqCovarCofact.Select(c => c.covariable).ToList();
                    count = freqCovarCofact.Select(c => c.count).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.Add(constant.ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.Cofactor = cofactor;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.CovariableCofactorInteraction:

                    var freqCovarCofactInt = individualFrequencies
                    .GroupBy(fr => (fr.Cofactor, fr.Covariable))
                    .Select(g => (
                        cofactor: g.Key.Cofactor,
                        covariable: g.Key.Covariable,
                        count: g.Count(),
                        constant: 1D
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.covariable)
                    .ToList();

                    constant = freqCovarCofactInt.Select(c => c.constant).ToList();
                    cofactor = freqCovarCofactInt.Select(c => c.cofactor).ToList();
                    covariable = freqCovarCofactInt.Select(c => c.covariable).ToList();
                    count = freqCovarCofactInt.Select(c => c.count).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }
                    foreach (var item in label.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescription.Add("level " + item + " * " + covariableName + ix);
                        }
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);

                    design.Add(constant.ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));
                    design.AddRange(du.MakeInteraction(cofactor, pol, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.Cofactor = cofactor;
                    data.GroupCounts = count;

                    break;
                default:
                    break;
            }

            return data;
        }

        /// <summary>
        ///  Returns all structures to form predictions for individuals: design matrix for covariates (X) and Description.
        ///  Total length of structures is equal to the number of individuals.
        /// </summary>
        /// <param name="individualFrequencies"></param>
        /// <param name="covariateModel"></param>
        /// <param name="fdr"></param>
        /// <returns></returns>
        public static FrequencyDataResult GetDMIndividualPredictions(
            this IEnumerable<IndividualFrequency> individualFrequencies,
            CovariateModelType covariateModel,
            FrequencyDataResult fdr
        ) {
            var covariableName = "pol^";
            int dfPol = fdr.DfPolynomial;

            var data = new FrequencyDataResult();

            var covariable = individualFrequencies.Select(c => c.Covariable).ToList();
            var cofactor = individualFrequencies.Select(c => c.Cofactor).ToList();
            var constant = individualFrequencies.Select(c => (double)c.NumberOfIndividuals).ToList();
            var count = individualFrequencies.Select(c => c.NumberOfIndividuals).ToList();
            var nbinomial = individualFrequencies.Select(c => (double)c.Nbinomial).ToList();
            var frequency = individualFrequencies.Select(c => c.Frequency).ToList();
            var simulatedIndividuals = individualFrequencies.Select(c => c.SimulatedIndividual).ToList();

            DesignUtils du = new DesignUtils();

            var pol = new List<double[]>();
            var design = new List<double[]>();
            var label = new List<string>();
            data.DesignMatrixDescription = [
                "constant"
            ];
            data.SimulatedIndividuals = simulatedIndividuals;

            switch (covariateModel) {
                case CovariateModelType.Constant:

                    design.Add(constant.ToArray());
                    data.X = du.ConvertToDesignMatrix(design);
                    data.GroupCounts = count;
                    data.Ybin = frequency;
                    data.Nbin = nbinomial;
                    break;

                case CovariateModelType.Covariable:

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.Add(constant.ToArray());
                    design.AddRange(pol);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.GroupCounts = count;
                    data.Ybin = frequency;
                    data.Nbin = nbinomial;
                    break;

                case CovariateModelType.Cofactor:

                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    design.Add(constant.ToArray());
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactor = cofactor;
                    data.GroupCounts = count;
                    data.Ybin = frequency;
                    data.Nbin = nbinomial;
                    break;

                case CovariateModelType.CovariableCofactor:

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.Add(constant.ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.Cofactor = cofactor;
                    data.GroupCounts = count;
                    data.Ybin = frequency;
                    data.Nbin = nbinomial;
                    break;

                case CovariateModelType.CovariableCofactorInteraction:

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }
                    foreach (var item in label.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescription.Add("level " + item + " * " + covariableName + ix);
                        }
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);

                    design.Add(constant.ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));
                    design.AddRange(du.MakeInteraction(cofactor, pol, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.Cofactor = cofactor;
                    data.GroupCounts = count;
                    data.Ybin = frequency;
                    data.Nbin = nbinomial;
                    break;
                default:
                    break;
            }

            return data;
        }

        /// <summary>
        /// Returns all structures to fit the amount model: design matrix for covariates X,
        /// Weights, IndividualId, Y and Description.
        /// </summary>
        /// <param name="transformedIntakeAmounts"></param>
        /// <param name="covariateModel"></param>
        /// <param name="dfPol"></param>
        /// <returns></returns>
        public static AmountDataResult GetDMAmount(
            this ICollection<ModelledIndividualAmount> transformedIntakeAmounts,
            CovariateModelType covariateModel,
            int dfPol
        ) {
            var covariableName = "pol^";
            var data = new AmountDataResult() {
                DfPolynomial = dfPol,
            };

            var positiveIntakes = transformedIntakeAmounts
                .SelectMany(r => r.TransformedDayAmounts, (ia, tda) => (
                    y: tda,
                    id: ia.SimulatedIndividual,
                    cofactor: ia.Cofactor,
                    covariable: ia.Covariable,
                    count: (double)ia.NumberOfPositiveIntakeDays,
                    samplingWeight: ia.SimulatedIndividual.SamplingWeight,
                    constant: 1D
                ))
                .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                .ThenBy(a => a.covariable)
                .ToList();

            DesignUtils du = new DesignUtils();
            Polynomial polynomial = null;
            OrthogonalPolynomial orthPol = null;

            var design = new List<double[]>();
            var covariable = positiveIntakes.Select(c => c.covariable).ToList();
            var cofactor = positiveIntakes.Select(c => c.cofactor).ToList();

            var label = new List<string>();
            data.DesignMatrixDescriptions = [];

            data.Ys = positiveIntakes.Select(c => c.y).ToList();
            data.SimulatedIndividuals = positiveIntakes.Select(c => c.id).ToList();

            switch (covariateModel) {
                case CovariateModelType.Constant:
                    data.X = null;
                    break;
                case CovariateModelType.Covariable:
                    orthPol = new OrthogonalPolynomial();
                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, data.IndividualSamplingWeights);
                    design.AddRange(polynomial.Result);
                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.PolynomialResult = polynomial;
                    break;
                case CovariateModelType.Cofactor:
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    design.AddRange(du.MakeDummy(cofactor));
                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactors = cofactor;
                    break;
                case CovariateModelType.CovariableCofactor:
                    orthPol = new OrthogonalPolynomial();
                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, data.IndividualSamplingWeights);
                    design.AddRange(polynomial.Result);
                    design.AddRange(du.MakeDummy(cofactor));
                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.Cofactors = cofactor;
                    data.PolynomialResult = polynomial;
                    break;
                case CovariateModelType.CovariableCofactorInteraction:
                    orthPol = new OrthogonalPolynomial();
                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    foreach (var item in label.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescriptions.Add("level " + item + " * " + covariableName + ix);
                        }
                    }
                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, data.IndividualSamplingWeights);
                    design.AddRange(polynomial.Result);
                    design.AddRange(du.MakeDummy(cofactor));
                    design.AddRange(du.MakeInteraction(cofactor, polynomial.Result, dfPol));
                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.Cofactors = cofactor;
                    data.PolynomialResult = polynomial;
                    break;
                default:
                    break;
            }
            return data;
        }

        /// <summary>
        /// Returns all structures to form predictions for groups (e.g the combination of covariates): design matrix for covariates (X), group counts
        /// and Description. Total length of structures is equal to the number of combinations of covariates.
        /// </summary>
        /// <param name="intakeAmounts"></param>
        /// <param name="settings"></param>
        /// <param name="polynomial"></param>
        /// <returns></returns>
        public static AmountDataResult GetDMSpecifiedPredictions(
            this IEnumerable<ModelledIndividualAmount> intakeAmounts,
            CovariateModelType covariateModel,
            AmountDataResult adr,
            List<double> predictions
        ) {
            var covariableName = "pol^";
            int dfPol = adr.DfPolynomial;

            var data = new AmountDataResult();

            DesignUtils du = new DesignUtils(); ;

            var pol = new List<double[]>();
            var design = new List<double[]>();
            var factorLevels = new List<string>();
            int combinations;
            var covariableExtended = new List<double>();
            var cofactorExtended = new List<string>();

            if (adr.Ys.Count == 0) {
                data.GroupCounts = [];
                return data;
            }

            switch (covariateModel) {
                case CovariateModelType.Constant:
                    data.X = null;
                    data.GroupCounts = new int[1].ToList();
                    break;


                case CovariateModelType.Covariable:
                    data.DesignMatrixDescriptions = [];

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(predictions, dfPol);
                    design.AddRange(pol);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = predictions;
                    data.GroupCounts = new int[predictions.Count].ToList();

                    break;

                case CovariateModelType.Cofactor:

                    factorLevels = intakeAmounts
                       .Select(c => c.Cofactor)
                       .Distinct()
                       .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                       .ToList();

                    data.DesignMatrixDescriptions = [];
                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }

                    design.AddRange(du.MakeDummy(factorLevels));
                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactors = factorLevels;
                    data.GroupCounts = new int[factorLevels.Count].ToList();

                    break;

                case CovariateModelType.CovariableCofactor:

                    factorLevels = intakeAmounts
                       .Select(c => c.Cofactor)
                       .Distinct()
                       .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                       .ToList();

                    data.DesignMatrixDescriptions = [];

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    combinations = factorLevels.Count * predictions.Count;
                    for (int i = 0; i < factorLevels.Count; i++) {
                        for (int j = 0; j < predictions.Count; j++) {
                            cofactorExtended.Add(factorLevels[i]);
                            covariableExtended.Add(predictions[j]);
                        }
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariableExtended, dfPol);
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactorExtended));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariableExtended;
                    data.Cofactors = cofactorExtended;
                    data.GroupCounts = new int[combinations].ToList();

                    break;

                case CovariateModelType.CovariableCofactorInteraction:

                    factorLevels = intakeAmounts
                      .Select(c => c.Cofactor)
                      .Distinct()
                      .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                      .ToList();


                    data.DesignMatrixDescriptions = [];


                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescriptions.Add("level " + item + " * " + covariableName + ix);
                        }
                    }
                    combinations = factorLevels.Count * predictions.Count;
                    for (int i = 0; i < factorLevels.Count; i++) {
                        for (int j = 0; j < predictions.Count; j++) {
                            cofactorExtended.Add(factorLevels[i]);
                            covariableExtended.Add(predictions[j]);
                        }
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariableExtended, dfPol);

                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactorExtended));
                    design.AddRange(du.MakeInteraction(cofactorExtended, pol, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariableExtended;
                    data.Cofactors = cofactorExtended;
                    data.GroupCounts = new int[combinations].ToList();

                    break;
                default:
                    break;
            }
            return data;
        }

        /// <summary>
        /// Returns all structures to form predictions for groups (e.g the combination of covariates): design matrix for covariates (X), group counts
        /// and Description. Total length of structures is equal to the number of combinations of covariates.
        /// </summary>
        /// <param name="intakeAmounts"></param>
        /// <param name="covariateModel"></param>
        /// <param name="adr"></param>
        /// <returns></returns>
        public static AmountDataResult GetDMConditionalPredictions(
            this IEnumerable<ModelledIndividualAmount> intakeAmounts,
            CovariateModelType covariateModel,
            AmountDataResult adr
        ) {
            var covariableName = "pol^";
            int dfPol = adr.DfPolynomial;

            var data = new AmountDataResult();

            DesignUtils du = new DesignUtils(); ;

            var pol = new List<double[]>();
            var design = new List<double[]>();
            var covariable = new List<double>();
            var cofactor = new List<string>();
            var count = new List<int>();
            var label = new List<string>();
            if (adr.Ys.Count == 0) {
                data.GroupCounts = [];
                return data;
            }
            switch (covariateModel) {
                case CovariateModelType.Constant:

                    data.X = null;
                    data.GroupCounts = [
                        intakeAmounts.Select(idi => idi.SimulatedIndividual).Distinct().Count(),
                    ];
                    break;

                case CovariateModelType.Covariable:

                    var freqCovar = intakeAmounts
                          .GroupBy(fr => fr.Covariable)
                          .Select(g => (
                              covariable: g.Key,
                              count: g.Count()
                          ))
                          .OrderBy(a => a.covariable)
                          .ToList();
                    data.DesignMatrixDescriptions = [];

                    covariable = freqCovar.Select(c => c.covariable).ToList();
                    count = freqCovar.Select(c => c.count).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.AddRange(pol);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.Cofactor:

                    var freqCofact = intakeAmounts
                    .GroupBy(fr => fr.Cofactor)
                    .Select(g => (
                        cofactor: g.Key,
                        count: g.Count()
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                    data.DesignMatrixDescriptions = [];

                    cofactor = freqCofact.Select(c => c.cofactor).ToList();
                    count = freqCofact.Select(c => c.count).ToList();

                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }

                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactors = cofactor;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.CovariableCofactor:

                    var freqCovarCofact = intakeAmounts
                    .GroupBy(fr => (fr.Cofactor, fr.Covariable))
                    .Select(g => (
                        cofactor: g.Key.Cofactor,
                        covariable: g.Key.Covariable,
                        count: g.Count()
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.covariable)
                    .ToList();

                    data.DesignMatrixDescriptions = [];

                    cofactor = freqCovarCofact.Select(c => c.cofactor).ToList();
                    covariable = freqCovarCofact.Select(c => c.covariable).ToList();
                    count = freqCovarCofact.Select(c => c.count).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.Cofactors = cofactor;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.CovariableCofactorInteraction:

                    var freqCovarCofactInt = intakeAmounts
                    .GroupBy(fr => (fr.Cofactor, fr.Covariable))
                    .Select(g => (
                        cofactor: g.Key.Cofactor,
                        covariable: g.Key.Covariable,
                        count: g.Count()
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.covariable)
                    .ToList();

                    data.DesignMatrixDescriptions = [];

                    cofactor = freqCovarCofactInt.Select(c => c.cofactor).ToList();
                    covariable = freqCovarCofactInt.Select(c => c.covariable).ToList();
                    count = freqCovarCofactInt.Select(c => c.count).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    foreach (var item in label.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescriptions.Add("level " + item + " * " + covariableName + ix);
                        }
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);

                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));
                    design.AddRange(du.MakeInteraction(cofactor, pol, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.Cofactors = cofactor;
                    data.GroupCounts = count;

                    break;
                default:
                    break;
            }
            return data;
        }

        /// <summary>
        /// Returns all structures to form predictions for individuals: design matrix for covariates (X) and Description.
        /// Total length of structures is equal to the number of individuals.
        /// </summary>
        /// <param name="intakeAmounts"></param>
        /// <param name="covariateModel"></param>
        /// <param name="adr"></param>
        /// <returns></returns>
        public static AmountDataResult GetDMIndividualPredictions(
            this IEnumerable<ModelledIndividualAmount> intakeAmounts,
            CovariateModelType covariateModel,
            AmountDataResult adr
        ) {
            var covariableName = "pol^";
            int dfPol = adr.DfPolynomial;


            var covariable = intakeAmounts.Select(c => c.Covariable).ToList();
            var cofactor = intakeAmounts.Select(c => c.Cofactor).ToList();
            var count = intakeAmounts.Select(c => c.NumberOfIndividuals).ToList();
            var individuals = intakeAmounts.Select(c => c.SimulatedIndividual).ToList();
            var nDays = intakeAmounts.Select(c => c.NumberOfPositiveIntakeDays).ToList();
            var amount = intakeAmounts.Select(c => c.TransformedAmount).ToList();

            var data = new AmountDataResult {
                Amounts = amount,
                NDays = nDays,
                SimulatedIndividuals = individuals,
                GroupCounts = count,
                DesignMatrixDescriptions = []
            };

            DesignUtils du = new DesignUtils();

            var pol = new List<double[]>();
            var design = new List<double[]>();
            var label = new List<string>();

            if (adr.Ys.Count == 0) {
                data.GroupCounts = [];
                return data;
            }
            switch (covariateModel) {
                case CovariateModelType.Constant:
                    data.X = null;
                    break;

                case CovariateModelType.Covariable:

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.AddRange(pol);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    break;

                case CovariateModelType.Cofactor:

                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }

                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactors = cofactor;
                    break;

                case CovariateModelType.CovariableCofactor:

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.Cofactors = cofactor;

                    break;

                case CovariateModelType.CovariableCofactorInteraction:

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    foreach (var item in label.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescriptions.Add("level " + item + " * " + covariableName + ix);
                        }
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);

                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));
                    design.AddRange(du.MakeInteraction(cofactor, pol, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.Cofactors = cofactor;

                    break;
                default:
                    break;
            }
            return data;
        }



        public static FrequencyDataResult GetDMFrequencyLNN(
            this IEnumerable<SimpleIndividualDayIntake> individualDayAmounts,
            CovariateModelType covariateModel,
            int dfPol
        ) {
            var covariableName = "pol^";
            var data = new FrequencyDataResult() {
                DfPolynomial = dfPol,
            };
            var du = new DesignUtils();
            OrthogonalPolynomial orthPol = null;
            var polynomial = new Polynomial();

            var design = new List<double[]>();
            var covariable = individualDayAmounts.Select(c => c.SimulatedIndividual.Covariable).ToList();
            var cofactor = individualDayAmounts.Select(c => c.SimulatedIndividual.Cofactor).ToList();
            var constant = Enumerable.Repeat(1D, individualDayAmounts.Count()).ToList();
            var weights = individualDayAmounts.Select(c => c.SimulatedIndividual.SamplingWeight).ToList();

            var label = new List<string>();
            data.DesignMatrixDescription = [
                "constant"
            ];
            data.Weights = weights;
            design.Add(constant.ToArray());

            switch (covariateModel) {
                case CovariateModelType.Constant:
                    data.X = du.ConvertToDesignMatrix(design);
                    break;

                case CovariateModelType.Covariable:
                    orthPol = new OrthogonalPolynomial();
                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }

                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, data.Weights);
                    design.AddRange(polynomial.Result);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.PolynomialResult = polynomial;

                    break;

                case CovariateModelType.Cofactor:
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }
                    design.AddRange(du.MakeDummy(cofactor));
                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactor = cofactor;

                    break;

                case CovariateModelType.CovariableCofactor:

                    orthPol = new OrthogonalPolynomial();
                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, data.Weights);

                    design.AddRange(polynomial.Result);
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.Cofactor = cofactor;
                    data.PolynomialResult = polynomial;

                    break;
                case CovariateModelType.CovariableCofactorInteraction:

                    orthPol = new OrthogonalPolynomial();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }
                    foreach (var item in label.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescription.Add("level " + item + " * " + covariableName + ix);
                        }
                    }

                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, data.Weights);

                    design.AddRange(polynomial.Result);
                    design.AddRange(du.MakeDummy(cofactor));
                    design.AddRange(du.MakeInteraction(cofactor, polynomial.Result, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.Cofactor = cofactor;
                    data.PolynomialResult = polynomial;

                    break;
                default:
                    break;
            }
            return data;

        }

        public static AmountDataResult GetDMAmountLNN(
            this IEnumerable<SimpleIndividualDayIntake> dietaryIndividualDayIntakes,
            CovariateModelType covariateModel,
            int dfPol
        ) {
            var covariableName = "pol^";
            var data = new AmountDataResult() {
                DfPolynomial = dfPol,
            };

            var du = new DesignUtils();
            OrthogonalPolynomial orthPol = null;
            var polynomial = new Polynomial();
            var design = new List<double[]>();
            var y = dietaryIndividualDayIntakes.Select(c => c.Amount).ToList();
            var covariable = dietaryIndividualDayIntakes.Select(c => c.SimulatedIndividual.Covariable).ToList();
            var cofactor = dietaryIndividualDayIntakes.Select(c => c.SimulatedIndividual.Cofactor).ToList();
            var constant = Enumerable.Repeat(1D, dietaryIndividualDayIntakes.Count()).ToList();

            var label = new List<string>();
            data.DesignMatrixDescriptions = [
                "constant"
            ];
            var individualSamplingWeights = dietaryIndividualDayIntakes
                .Select(c => c.Amount > 0 ? c.SimulatedIndividual.SamplingWeight : 0)
                .ToList();
            design.Add(constant.ToArray());
            switch (covariateModel) {
                case CovariateModelType.Constant:
                    data.X = du.ConvertToDesignMatrix(design);
                    break;

                case CovariateModelType.Covariable:
                    orthPol = new OrthogonalPolynomial();
                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }

                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, individualSamplingWeights);
                    design.AddRange(polynomial.Result);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.PolynomialResult = polynomial;

                    break;

                case CovariateModelType.Cofactor:
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    design.AddRange(du.MakeDummy(cofactor));
                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactors = cofactor;

                    break;

                case CovariateModelType.CovariableCofactor:

                    orthPol = new OrthogonalPolynomial();
                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }

                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, individualSamplingWeights);

                    design.AddRange(polynomial.Result);
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.Cofactors = cofactor;
                    data.PolynomialResult = polynomial;

                    break;
                case CovariateModelType.CovariableCofactorInteraction:

                    orthPol = new OrthogonalPolynomial();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    foreach (var item in label.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescriptions.Add("level " + item + " * " + covariableName + ix);
                        }
                    }

                    polynomial = orthPol.CalculateOrthPol(covariable, dfPol, individualSamplingWeights);

                    design.AddRange(polynomial.Result);
                    design.AddRange(du.MakeDummy(cofactor));
                    design.AddRange(du.MakeInteraction(cofactor, polynomial.Result, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.Cofactors = cofactor;
                    data.PolynomialResult = polynomial;

                    break;
                default:
                    break;
            }
            return data;
        }

        public static AmountDataResult GetDMConditionalPredictionsLNN(
            this IEnumerable<SimpleIndividualDayIntake> individualDayAmounts,
            CovariateModelType covariateModel,
            AmountDataResult adr
        ) {
            var covariableName = "pol^";
            int dfPol = adr.DfPolynomial;

            var data = new AmountDataResult();

            DesignUtils du = new DesignUtils(); ;

            var pol = new List<double[]>();
            var design = new List<double[]>();
            var covariable = new List<double>();
            var cofactor = new List<string>();
            var count = new List<int>();
            var constant = new List<double>();
            var label = new List<string>();
            data.DesignMatrixDescriptions = [
                "constant"
            ];

            switch (covariateModel) {
                case CovariateModelType.Constant:
                    data.X = new double[1, 1] { { 1 } };
                    data.GroupCounts = [
                        individualDayAmounts.Select(idi => idi.SimulatedIndividual.Id).Distinct().Count(),
                    ];
                    break;

                case CovariateModelType.Covariable:
                    var freqCovar = individualDayAmounts
                          .GroupBy(fr => fr.SimulatedIndividual.Covariable)
                          .Select(g => (
                              covariable: g.Key,
                              count: g.Count(),
                              constant: 1D
                          ))
                          .OrderBy(a => a.covariable)
                          .ToList();

                    covariable = freqCovar.Select(c => c.covariable).ToList();
                    count = freqCovar.Select(c => c.count).ToList();
                    constant = freqCovar.Select(c => c.constant).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.Add(constant.ToArray());
                    design.AddRange(pol);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.Cofactor:

                    var freqCofact = individualDayAmounts
                    .GroupBy(fr => fr.SimulatedIndividual.Cofactor)
                    .Select(g => (
                        cofactor: g.Key,
                        count: g.Count(),
                        constant: 1D
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ToList();


                    cofactor = freqCofact.Select(c => c.cofactor).ToList();
                    count = freqCofact.Select(c => c.count).ToList();
                    constant = freqCofact.Select(c => c.constant).ToList();

                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    design.Add(constant.ToArray());
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactors = cofactor;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.CovariableCofactor:

                    var freqCovarCofact = individualDayAmounts
                    .GroupBy(fr => (fr.SimulatedIndividual.Cofactor, fr.SimulatedIndividual.Covariable))
                    .Select(g => (
                        cofactor: g.Key.Cofactor,
                        covariable: g.Key.Covariable,
                        count: g.Count(),
                        constant: 1D
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.covariable)
                    .ToList();

                    cofactor = freqCovarCofact.Select(c => c.cofactor).ToList();
                    covariable = freqCovarCofact.Select(c => c.covariable).ToList();
                    count = freqCovarCofact.Select(c => c.count).ToList();
                    constant = freqCovarCofact.Select(c => c.constant).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.Add(constant.ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.Cofactors = cofactor;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.CovariableCofactorInteraction:

                    var freqCovarCofactInt = individualDayAmounts
                    .GroupBy(fr => (fr.SimulatedIndividual.Cofactor, fr.SimulatedIndividual.Covariable))
                    .Select(g => (
                        cofactor: g.Key.Cofactor,
                        covariable: g.Key.Covariable,
                        count: g.Count(),
                        constant: 1D
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.covariable)
                    .ToList();

                    cofactor = freqCovarCofactInt.Select(c => c.cofactor).ToList();
                    covariable = freqCovarCofactInt.Select(c => c.covariable).ToList();
                    count = freqCovarCofactInt.Select(c => c.count).ToList();
                    constant = freqCovarCofactInt.Select(c => c.constant).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    foreach (var item in label.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescriptions.Add("level " + item + " * " + covariableName + ix);
                        }
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);

                    design.Add(constant.ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));
                    design.AddRange(du.MakeInteraction(cofactor, pol, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariable;
                    data.Cofactors = cofactor;
                    data.GroupCounts = count;

                    break;
                default:
                    break;
            }
            return data;
        }
        public static AmountDataResult GetDMSpecifiedPredictionsLNN(
            this IEnumerable<SimpleIndividualDayIntake> individualDayAmounts,
            CovariateModelType covariateModel,
            AmountDataResult adr,
            List<double> predictions
        ) {
            var covariableName = "pol^";
            int dfPol = adr.DfPolynomial;

            var data = new AmountDataResult();

            DesignUtils du = new DesignUtils(); ;

            var pol = new List<double[]>();
            var design = new List<double[]>();
            var factorLevels = new List<string>();
            data.DesignMatrixDescriptions = [
                "constant"
            ];
            int combinations;
            var covariableExtended = new List<double>();
            var cofactorExtended = new List<string>();

            switch (covariateModel) {
                case CovariateModelType.Constant:
                    data.X = new double[1, 1] { { 1 } };
                    data.GroupCounts = new int[1].ToList();
                    break;

                case CovariateModelType.Covariable:

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(predictions, dfPol);
                    design.Add(Enumerable.Repeat(1D, predictions.Count).ToArray());
                    design.AddRange(pol);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = predictions;
                    data.GroupCounts = new int[predictions.Count].ToList();

                    break;

                case CovariateModelType.Cofactor:

                    factorLevels = individualDayAmounts
                            .Select(c => c.SimulatedIndividual.Cofactor)
                            .Distinct()
                            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                            .ToList();

                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }

                    design.Add(Enumerable.Repeat(1D, factorLevels.Count).ToArray());
                    design.AddRange(du.MakeDummy(factorLevels));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactors = factorLevels;
                    data.GroupCounts = new int[factorLevels.Count].ToList();


                    break;

                case CovariateModelType.CovariableCofactor:

                    factorLevels = individualDayAmounts
                           .Select(c => c.SimulatedIndividual.Cofactor)
                           .Distinct()
                           .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                           .ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }

                    combinations = factorLevels.Count * predictions.Count;
                    for (int i = 0; i < factorLevels.Count; i++) {
                        for (int j = 0; j < predictions.Count; j++) {
                            cofactorExtended.Add(factorLevels[i]);
                            covariableExtended.Add(predictions[j]);
                        }
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariableExtended, dfPol);

                    design.Add(Enumerable.Repeat(1D, combinations).ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactorExtended));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariableExtended;
                    data.Cofactors = cofactorExtended;
                    data.GroupCounts = new int[combinations].ToList();

                    break;

                case CovariateModelType.CovariableCofactorInteraction:

                    factorLevels = individualDayAmounts
                      .Select(c => c.SimulatedIndividual.Cofactor)
                      .Distinct()
                      .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                      .ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescriptions.Add(covariableName + ix);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescriptions.Add("level " + item);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescriptions.Add("level " + item + " * " + covariableName + ix);
                        }
                    }
                    combinations = factorLevels.Count * predictions.Count;
                    for (int i = 0; i < factorLevels.Count; i++) {
                        for (int j = 0; j < predictions.Count; j++) {
                            cofactorExtended.Add(factorLevels[i]);
                            covariableExtended.Add(predictions[j]);
                        }
                    }

                    pol = adr.PolynomialResult.CalculateComponentsPolynomial(covariableExtended, dfPol);

                    design.Add(Enumerable.Repeat(1D, combinations).ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactorExtended));
                    design.AddRange(du.MakeInteraction(cofactorExtended, pol, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariables = covariableExtended;
                    data.Cofactors = cofactorExtended;
                    data.GroupCounts = new int[combinations].ToList();
                    break;
                default:
                    break;
            }
            return data;
        }

        public static FrequencyDataResult GetDMConditionalPredictionsLNN(
            this IEnumerable<SimpleIndividualDayIntake> individualDayAmounts,
            CovariateModelType covariateModel,
            FrequencyDataResult fdr
        ) {
            var covariableName = "pol^";

            int dfPol = fdr.DfPolynomial;

            var data = new FrequencyDataResult();

            DesignUtils du = new DesignUtils(); ;

            var pol = new List<double[]>();
            var design = new List<double[]>();
            var covariable = new List<double>();
            var cofactor = new List<string>();
            var count = new List<int>();
            var constant = new List<double>();
            var label = new List<string>();
            data.DesignMatrixDescription = [
                "constant"
            ];

            switch (covariateModel) {
                case CovariateModelType.Constant:
                    data.X = new double[1, 1] { { 1 } };
                    data.GroupCounts = [
                        individualDayAmounts.Select(idi => idi.SimulatedIndividual.Id).Distinct().Count(),
                    ];
                    break;

                case CovariateModelType.Covariable:

                    var freqCovar = individualDayAmounts
                          .GroupBy(fr => fr.SimulatedIndividual.Covariable)
                          .Select(g => (
                              covariable: g.Key,
                              count: g.Count(),
                              constant: 1D
                          ))
                          .OrderBy(a => a.covariable)
                          .ToList();

                    covariable = freqCovar.Select(c => c.covariable).ToList();
                    count = freqCovar.Select(c => c.count).ToList();
                    constant = freqCovar.Select(c => c.constant).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.Add(constant.ToArray());
                    design.AddRange(pol);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.Cofactor:

                    var freqCofact = individualDayAmounts
                    .GroupBy(fr => fr.SimulatedIndividual.Cofactor)
                    .Select(g => (
                        cofactor: g.Key,
                        count: g.Count(),
                        constant: 1D
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ToList();


                    cofactor = freqCofact.Select(c => c.cofactor).ToList();
                    count = freqCofact.Select(c => c.count).ToList();
                    constant = freqCofact.Select(c => c.constant).ToList();

                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }
                    design.Add(constant.ToArray());
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactor = cofactor;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.CovariableCofactor:

                    var freqCovarCofact = individualDayAmounts
                    .GroupBy(fr => (fr.SimulatedIndividual.Cofactor, fr.SimulatedIndividual.Covariable))
                    .Select(g => (
                        cofactor: g.Key.Cofactor,
                        covariable: g.Key.Covariable,
                        count: g.Count(),
                        constant: 1D
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.covariable)
                    .ToList();

                    cofactor = freqCovarCofact.Select(c => c.cofactor).ToList();
                    covariable = freqCovarCofact.Select(c => c.covariable).ToList();
                    count = freqCovarCofact.Select(c => c.count).ToList();
                    constant = freqCovarCofact.Select(c => c.constant).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);
                    design.Add(constant.ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.Cofactor = cofactor;
                    data.GroupCounts = count;

                    break;

                case CovariateModelType.CovariableCofactorInteraction:

                    var freqCovarCofactInt = individualDayAmounts
                    .GroupBy(fr => (fr.SimulatedIndividual.Cofactor, fr.SimulatedIndividual.Covariable))
                    .Select(g => (
                        cofactor: g.Key.Cofactor,
                        covariable: g.Key.Covariable,
                        count: g.Count(),
                        constant: 1D
                    ))
                    .OrderBy(a => a.cofactor, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(a => a.covariable)
                    .ToList();

                    cofactor = freqCovarCofactInt.Select(c => c.cofactor).ToList();
                    covariable = freqCovarCofactInt.Select(c => c.covariable).ToList();
                    count = freqCovarCofactInt.Select(c => c.count).ToList();
                    constant = freqCovarCofactInt.Select(c => c.constant).ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    label = cofactor.Distinct().OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList();
                    foreach (var item in label.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }
                    foreach (var item in label.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescription.Add("level " + item + " * " + covariableName + ix);
                        }
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariable, dfPol);

                    design.Add(constant.ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactor));
                    design.AddRange(du.MakeInteraction(cofactor, pol, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariable;
                    data.Cofactor = cofactor;
                    data.GroupCounts = count;

                    break;
                default:
                    break;
            }
            return data;
        }

        public static FrequencyDataResult GetDMSpecifiedPredictionsLNN(
            this IEnumerable<SimpleIndividualDayIntake> individualDayAmounts,
            CovariateModelType covariateModel,
            FrequencyDataResult fdr,
            List<double> predictionLevels
        ) {
            var covariableName = "pol^";
            int dfPol = fdr.DfPolynomial;

            var data = new FrequencyDataResult();

            DesignUtils du = new DesignUtils(); ;

            var pol = new List<double[]>();
            var design = new List<double[]>();
            var factorLevels = new List<string>();
            data.DesignMatrixDescription = [
                "constant"
            ];
            int combinations;
            var covariableExtended = new List<double>();
            var cofactorExtended = new List<string>();

            switch (covariateModel) {
                case CovariateModelType.Constant:
                    data.X = new double[1, 1] { { 1 } };
                    data.GroupCounts = new int[1].ToList();
                    break;
                case CovariateModelType.Covariable:
                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(predictionLevels, dfPol);
                    design.Add(Enumerable.Repeat(1D, predictionLevels.Count).ToArray());
                    design.AddRange(pol);

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = predictionLevels;
                    data.GroupCounts = new int[predictionLevels.Count].ToList();

                    break;
                case CovariateModelType.Cofactor:
                    factorLevels = individualDayAmounts
                            .Select(c => c.SimulatedIndividual.Cofactor)
                            .Distinct()
                            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                            .ToList();

                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    design.Add(Enumerable.Repeat(1D, factorLevels.Count).ToArray());
                    design.AddRange(du.MakeDummy(factorLevels));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Cofactor = factorLevels;
                    data.GroupCounts = new int[factorLevels.Count].ToList();

                    break;
                case CovariateModelType.CovariableCofactor:
                    factorLevels = individualDayAmounts
                            .Select(c => c.SimulatedIndividual.Cofactor)
                            .Distinct()
                            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                            .ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }

                    combinations = factorLevels.Count * predictionLevels.Count;
                    for (int i = 0; i < factorLevels.Count; i++) {
                        for (int j = 0; j < predictionLevels.Count; j++) {
                            cofactorExtended.Add(factorLevels[i]);
                            covariableExtended.Add(predictionLevels[j]);
                        }
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariableExtended, dfPol);

                    design.Add(Enumerable.Repeat(1D, combinations).ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactorExtended));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariableExtended;
                    data.Cofactor = cofactorExtended;
                    data.GroupCounts = new int[combinations].ToList();
                    break;
                case CovariateModelType.CovariableCofactorInteraction:
                    factorLevels = individualDayAmounts
                        .Select(c => c.SimulatedIndividual.Cofactor)
                        .Distinct()
                        .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    for (int i = 0; i < dfPol; i++) {
                        var ix = Convert.ToString(i + 1);
                        data.DesignMatrixDescription.Add(covariableName + ix);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        data.DesignMatrixDescription.Add("level " + item);
                    }
                    foreach (var item in factorLevels.Skip(1)) {
                        for (int i = 0; i < dfPol; i++) {
                            var ix = Convert.ToString(i + 1);
                            data.DesignMatrixDescription.Add("level " + item + " * " + covariableName + ix);
                        }
                    }
                    combinations = factorLevels.Count * predictionLevels.Count;
                    for (int i = 0; i < factorLevels.Count; i++) {
                        for (int j = 0; j < predictionLevels.Count; j++) {
                            cofactorExtended.Add(factorLevels[i]);
                            covariableExtended.Add(predictionLevels[j]);
                        }
                    }

                    pol = fdr.PolynomialResult.CalculateComponentsPolynomial(covariableExtended, dfPol);

                    design.Add(Enumerable.Repeat(1D, combinations).ToArray());
                    design.AddRange(pol);
                    design.AddRange(du.MakeDummy(cofactorExtended));
                    design.AddRange(du.MakeInteraction(cofactorExtended, pol, dfPol));

                    data.X = du.ConvertToDesignMatrix(design);
                    data.Covariable = covariableExtended;
                    data.Cofactor = cofactorExtended;
                    data.GroupCounts = new int[combinations].ToList();
                    break;
                default:
                    break;
            }
            return data;
        }
    }
}
