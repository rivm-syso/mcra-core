namespace MCRA.Utils.NumericalRecipes {

    /// <summary>
    /// Optimize a one-dimensional function.
    /// </summary>
    public class OptimizeOneD {

        /// <summary>
        /// Initialize with default values MaxCycle=1000 and Tolerance=1.0e-6.
        /// </summary>
        public OptimizeOneD() {
            MaxCycles = 1000;
            Tolerance = 1.0e-6;
        }

        /// <summary>
        /// Maximum number of cycles in minimization routine; default 1000;
        /// </summary>
        public int MaxCycles { get; set; }

        /// <summary>
        /// Convergence criterion in minimization routines; default 1.0e-6.
        /// </summary>
        public double Tolerance { get; set; }

        /// <summary>
        /// Number of function evaluations.
        /// </summary>
        public int Evaluations { get; private set; }

        /// <summary>
        /// Number of cycles in minimization routine.
        /// </summary>
        public int Cycles { get; private set; }

        /// <summary>
        /// Elapsed optimization time.
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }

        /// <summary>
        /// Whether the minimization routine has convergence in MaxCycles
        /// </summary>
        public bool Convergence { get; private set; }

        /// <summary>
        /// One-dimensional function.
        /// </summary>
        /// <param name="arg">Argument of function.</param>
        /// <param name="data">Data to be passed to the function.</param>
        /// <returns>Function value.</returns>
        public delegate double Function(double arg, object data = null);

        /// <summary>
        /// Data to be passed to the Function delegate
        /// </summary>
        public object Data { get; set; }

        // Finds the minimum of a one-dimensional function using Brent's method (without the derivative).
        /// <summary>
        /// Finds the minimum of a one-dimensional function using Brent's method (without the derivative).
        /// </summary>
        /// <param name="ax">Initial estimate for minimum.</param>
        /// <param name="step">Stepsize for finding a bracketing triplet.</param>
        /// <param name="function">One-dimensional function with a single double argument which returns a function value.</param>
        /// <param name="xmin">Returns the minimum.</param>
        /// <returns>The minimum function value.</returns>
        /// <remarks>This routine isolates the minimum to a fractional precision of about Tolerance using Brent's method.
        /// The abscissa of the minimum is returned as xmin, and the minimum function value is returned by the method.
        /// Initial bracketing is performed by the Bracket routine using (initial) and (initial+step) as initial entry points.
        /// </remarks>
        /// <remarks>Uses Numerical Recipes routines MNBRACK and BRENT ported from C++ to C#.</remarks>
        public double Minimize(double ax, double step, Function function, out double xmin) {
            double bx, fmin;
            bx = ax + step;
            BracketMinimum(ref ax, ref bx, out var cx, out var fa, out var fb, out var fc, function);
            int bracketEval = Evaluations;
            TimeSpan bracketTime = ElapsedTime;
            fmin = Minimize(ax, bx, cx, function, out xmin);
            Evaluations += bracketEval;
            ElapsedTime += bracketTime;
            return fmin;
        }

        /// <summary>
        /// Finds the minimum of a one-dimensional function using Brent's method employing the derivative.
        /// </summary>
        /// <param name="initial">Initial estimate for minimum.</param>
        /// <param name="step">Stepsize for finding a bracketing triplet.</param>
        /// <param name="function">One-dimensional function with a single double argument which returns the function value.</param>
        /// <param name="dfunction">One-dimensional derivative function with a single double argument which returns the derivative function value.</param>
        /// <param name="xmin">Returns the minimum.</param>
        /// <returns>The minimum function value.</returns>
        /// <remarks>this routine isolates the minimum to a fractional precision of about Tolerance using a modification of
        /// Brent's method that uses derivatives. The abscissa of the minimum is returned as xmin, and the minimum
        /// function value is returned by the method.
        /// Initial bracketing is performed by the Bracket routine using (initial) and (initial+step) as initial entry points.
        /// </remarks>
        /// <remarks>Uses Numerical Recipes routine MNBRACK and DBRENT ported from C++ to C#.</remarks>
        public double MinimizeUsingDerivative(double initial, double step, Function function, Function dfunction, out double xmin) {
            double bx, fmin;
            bx = initial + step;
            BracketMinimum(ref initial, ref bx, out var cx, out var fa, out var fb, out var fc, function);
            int bracketEval = Evaluations;
            TimeSpan bracketTime = ElapsedTime;
            fmin = MinimizeUsingDerivative(initial, bx, cx, function, dfunction, out xmin);
            Evaluations += bracketEval;
            ElapsedTime += bracketTime;
            return fmin;
        }

        /// <summary>
        /// Finds the minimum of a one-dimensional function using Brent's method (without the derivative).
        /// </summary>
        /// <param name="ax">Lower limit of bracketing triplet.</param>
        /// <param name="bx">Middle value of bracketing triplet.</param>
        /// <param name="cx">Upper limit of bracketing triplet.</param>
        /// <param name="function">One-dimensional function with a single double argument which returns a function value.</param>
        /// <param name="xmin">Returns the minimum.</param>
        /// <returns>The minimum function value.</returns>
        /// <remarks>Given a one-dimensional function, and given a bracketing triplet of abscissas ax, bx and cx
        /// (such that bx is between ax and cx, and F(bx) is less than both F(ax) and F(cx)), this routine isolates
        /// the minimum to a fractional precision of about Tolerance using Brent's method. The abscissa of the minimum
        /// is returned as xmin, and the minimum function value is returned by the method.
        /// </remarks>
        /// <remarks>Numerical Recipes routine BRENT ported from C++ to C#.</remarks>
        public double Minimize(double ax, double bx, double cx, Function function, out double xmin) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Evaluations = 0;
            // Proceed
            const double CGOLD = 0.3819660;
            const double ZEPS = 1.0e-16;         // Nog aanpassen
            double a, b, d, e, etemp, fu, fv, fw, fx, p, q, r, tol1, tol2, u, v, w, x, xm;
            a = 0.0; b = 0.0; d = 0.0; e = 0.0;
            a = (ax < cx ? ax : cx);
            b = (ax > cx ? ax : cx);
            x = w = v = bx;
            Evaluations += 1;
            fw = fv = fx = function(x, Data);
            Convergence = false;
            for (Cycles = 1; Cycles <= MaxCycles; Cycles++) {
                xm = 0.5 * (a + b);
                tol2 = 2.0 * (tol1 = Tolerance * Math.Abs(x) + ZEPS);
                if (Math.Abs(x - xm) <= (tol2 - 0.5 * (b - a))) {
                    Convergence = true;
                    break;
                }
                if (Math.Abs(e) > tol1) {
                    r = (x - w) * (fx - fv);
                    q = (x - v) * (fx - fw);
                    p = (x - v) * q - (x - w) * r;
                    q = 2.0 * (q - r);
                    if (q > 0.0) {
                        p = -p;
                    }

                    q = Math.Abs(q);
                    etemp = e;
                    e = d;
                    if (Math.Abs(p) >= Math.Abs(0.5 * q * etemp) || p <= q * (a - x) || p >= q * (b - x)) {
                        d = CGOLD * (e = (x >= xm ? a - x : b - x));
                    } else {
                        d = p / q;
                        u = x + d;
                        if (u - a < tol2 || b - u < tol2) {
                            d = sign(tol1, xm - x);
                        }
                    }
                }
                else {
                    d = CGOLD * (e = (x >= xm ? a - x : b - x));
                }
                u = (Math.Abs(d) >= tol1 ? x + d : x + sign(tol1, d));
                Evaluations += 1;
                fu = function(u, Data);
                if (fu <= fx) {
                    if (u >= x) {
                        a = x;
                    } else {
                        b = x;
                    }
                    shift(ref v, ref w, ref x, u);
                    shift(ref fv, ref fw, ref fx, fu);
                }
                else {
                    if (u < x) {
                        a = u;
                    } else {
                        b = u;
                    }

                    if (fu <= fw || w == x) {
                        v = w;
                        w = u;
                        fv = fw;
                        fw = fu;
                    }
                    else if (fu <= fv || v == x || v == w) {
                        v = u;
                        fv = fu;
                    }
                }
            }
            ElapsedTime = sw.Elapsed;
            xmin = x;
            return fx;
        }

        // Finds the minimum of a one-dimensional function using Brent's method employing the derivative.
        /// <summary>
        /// Finds the minimum of a one-dimensional function using Brent's method employing the derivative.
        /// </summary>
        /// <param name="ax">Lower limit of bracketing triplet.</param>
        /// <param name="bx">Middle value of bracketing triplet.</param>
        /// <param name="cx">Upper limit of bracketing triplet.</param>
        /// <param name="function">One-dimensional function with a single double argument which returns the function value.</param>
        /// <param name="dfunction">One-dimensional derivative function with a single double argument which returns the derivative function value.</param>
        /// <param name="xmin">Returns the minimum.</param>
        /// <returns>The minimum function value.</returns>
        /// <remarks>Given a one-dimensional function and its derivative function, and given a bracketing triplet of abscissas ax, bx and cx
        /// (such that bx is between ax and cx, and F(bx) is less than both F(ax) and F(cx)), this routine isolates
        /// the minimum to a fractional precision of about Tolerance using a modification of Brent's method that uses derivatives.
        /// The abscissa of the minimum is returned as xmin, and the minimum function value is returned by the method.
        /// </remarks>
        /// <remarks>Numerical Recipes routine DBRENT ported from C++ to C#.</remarks>
        public double MinimizeUsingDerivative(double ax, double bx, double cx, Function function, Function dfunction, out double xmin) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Evaluations = 0;
            // Proceed
            const double ZEPS = 1.0e-16;         // Nog aanpassen
            bool ok1, ok2;
            double a, b, d = 0.0, d1, d2, du, dv, dw, dx, e = 0.0;
            double fu, fv, fw, fx, olde, tol1, tol2, u, u1, u2, v, w, x, xm;
            a = (ax < cx ? ax : cx);
            b = (ax > cx ? ax : cx);
            x = w = v = bx;
            Evaluations += 2;
            fw = fv = fx = function(x, Data);
            dw = dv = dx = dfunction(x, Data);
            Convergence = false;
            for (Cycles = 1; Cycles <= MaxCycles; Cycles++) {
                xm = 0.5 * (a + b);
                tol1 = Tolerance * Math.Abs(x) + ZEPS;
                tol2 = 2.0 * tol1;
                if (Math.Abs(x - xm) <= (tol2 - 0.5 * (b - a))) {
                    Convergence = true;
                    break;
                }
                if (Math.Abs(e) > tol1) {
                    d1 = 2.0 * (b - a);
                    d2 = d1;
                    if (dw != dx) {
                        d1 = (w - x) * dx / (dx - dw);
                    }

                    if (dv != dx) {
                        d2 = (v - x) * dx / (dx - dv);
                    }

                    u1 = x + d1;
                    u2 = x + d2;
                    ok1 = (a - u1) * (u1 - b) > 0.0 && dx * d1 <= 0.0;
                    ok2 = (a - u2) * (u2 - b) > 0.0 && dx * d2 <= 0.0;
                    olde = e;
                    e = d;
                    if (ok1 || ok2) {
                        if (ok1 && ok2) {
                            d = (Math.Abs(d1) < Math.Abs(d2) ? d1 : d2);
                        } else if (ok1) {
                            d = d1;
                        } else {
                            d = d2;
                        }

                        if (Math.Abs(d) <= Math.Abs(0.5 * olde)) {
                            u = x + d;
                            if ((u - a < tol2) || (b - u < tol2)) {
                                d = sign(tol1, xm - x);
                            }
                        }
                        else {
                            d = 0.5 * (e = (dx >= 0.0 ? a - x : b - x));
                        }
                    }
                    else {
                        d = 0.5 * (e = (dx >= 0.0 ? a - x : b - x));
                    }
                }
                else {
                    d = 0.5 * (e = (dx >= 0.0 ? a - x : b - x));
                }
                if (Math.Abs(d) >= tol1) {
                    u = x + d;
                    Evaluations += 1;
                    fu = function(u, Data);
                }
                else {
                    u = x + sign(tol1, d);
                    Evaluations += 1;
                    fu = function(u, Data);
                    if (fu > fx) {
                        Convergence = true;
                        break;
                    }
                }
                Evaluations += 1;
                du = dfunction(u, Data);
                if (fu <= fx) {
                    if (u >= x) {
                        a = x;
                    } else {
                        b = x;
                    }
                    mov3(out v, out fv, out dv, w, fw, dw);
                    mov3(out w, out fw, out dw, x, fx, dx);
                    mov3(out x, out fx, out dx, u, fu, du);
                }
                else {
                    if (u < x) {
                        a = u;
                    } else {
                        b = u;
                    }

                    if (fu <= fw || w == x) {
                        mov3(out v, out fv, out dv, w, fw, dw);
                        mov3(out w, out fw, out dw, u, fu, du);
                    }
                    else if (fu < fv || v == x || v == w) {
                        mov3(out v, out fv, out dv, u, fu, du);
                    }
                }
            }
            ElapsedTime = sw.Elapsed;
            xmin = x;
            return fx;
        }

        /// <summary>
        /// Brackets a minimum for a one-dimensional function.
        /// </summary>
        /// <param name="ax">Initial point on entry, returns Lower limit of bracketing triplet.</param>
        /// <param name="bx">Initial point on entry, returns Middle value of bracketing triplet.</param>
        /// <param name="cx">Returns Upper limit of bracketing triplet.</param>
        /// <param name="fa">Returns function value for ax.</param>
        /// <param name="fb">Returns function value for bx.</param>
        /// <param name="fc">Returns function value for cx.</param>
        /// <param name="function">One-dimensional function with a single double argument which returns a function value.</param>
        /// <remarks>Given a function, and given initial points ax and bx, this routine searches in the downhill
        /// direction (defined by the function as evaluated at the initial points) and returns new points
        /// ax, bx and cx that bracket a minimum. Also returned are the function values fa, fb and fc at the three points</remarks>
        /// <remarks>Numerical Recipes routine MNBRAK ported from C++ to C#.</remarks>
        public void BracketMinimum(ref double ax, ref double bx, out double cx, out double fa, out double fb, out double fc, Function function) {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            Evaluations = 0;
            // Proceed
            const double GLIMIT = 100.0;
            const double TINY = 1.0e-20;
            const double GOLD = 1.618034;
            double ulim, u, r, q, fu;
            Evaluations += 3;
            fa = function(ax, Data);
            fb = function(bx, Data);
            if (fb > fa) {
                swap(ref ax, ref bx);
                swap(ref fa, ref fb);
            }
            cx = bx + GOLD * (bx - ax);
            fc = function(cx, Data);
            while (fb > fc) {
                r = (bx - ax) * (fb - fc);
                q = (bx - cx) * (fb - fa);
                u = (bx) - ((bx - cx) * q - (bx - ax) * r) / (2.0 * sign(Math.Max(Math.Abs(q - r), TINY), q - r));
                ulim = bx + GLIMIT * (cx - bx);
                if ((bx - u) * (u - cx) > 0.0) {
                    Evaluations += 1;
                    fu = function(u, Data);
                    if (fu < fc) {
                        ax = bx;
                        bx = u;
                        fa = fb;
                        fb = fu;
                        break;
                    }
                    else if (fu > fb) {
                        cx = u;
                        fc = fu;
                        break;
                    }
                    u = cx + GOLD * (cx - bx);
                    Evaluations += 1;
                    fu = function(u, Data);
                }
                else if ((cx - u) * (u - ulim) > 0.0) {
                    Evaluations += 1;
                    fu = function(u, Data);
                    if (fu < fc) {
                        shift(ref bx, ref cx, ref u, cx + GOLD * (cx - bx));
                        Evaluations += 1;
                        shift(ref fb, ref fc, ref fu, function(u, Data));
                    }
                }
                else if ((u - ulim) * (ulim - cx) >= 0.0) {
                    u = ulim;
                    Evaluations += 1;
                    fu = function(u, Data);
                }
                else {
                    u = cx + GOLD * (cx - bx);
                    Evaluations += 1;
                    fu = function(u, Data);
                }
                shift(ref ax, ref bx, ref cx, u);
                shift(ref fa, ref fb, ref fc, fu);
            }
            ElapsedTime = sw.Elapsed;
        }

        /// <summary>
        /// Shifts four arguments (a=b, b=c, c=d).
        /// </summary>
        private static void shift(ref double a, ref double b, ref double c, double d) {
            a = b; b = c; c = d;
        }

        /// <summary>
        /// Swaps two arguments.
        /// </summary>
        private static void swap(ref double a, ref double b) {
            (b, a) = (a, b);
        }

        /// <summary>
        /// Copies (d,e,f) to (a,b,c).
        /// </summary>
        private static void mov3(out double a, out double b, out double c, double d, double e, double f) {
            a = d; b = e; c = f;
        }

        /// <summary>
        /// Signed absolute value.
        /// </summary>
        /// <param name="a">Value</param>
        /// <param name="b">Sign</param>
        /// <returns>Signed (by b) absolute value of a</returns>
        private static double sign(double a, double b) {
            if (b >= 0.0) {
                return Math.Abs(a);
            } else {
                return -Math.Abs(a);
            }
        }
    }
}
