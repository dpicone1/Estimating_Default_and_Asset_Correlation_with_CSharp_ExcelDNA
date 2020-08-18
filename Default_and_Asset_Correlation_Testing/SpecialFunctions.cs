using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Default_and_Asset_Correlation_Testing
{
    public class SpecialFunctions
    {
        const double OneOverRootTwoPi = 0.398942280401433;
        // probability density for a standard Gaussian distribution
        static public double NormalDensity(double x)
        { return OneOverRootTwoPi * Math.Exp(-x * x / 2); }

        static public double InverseCumulativeNormal(double u)
        {
            double[] a = { 2.50662823884, -18.61500062529, 41.39119773534, -25.44106049637 };
            double[] b = { -8.47351093090, 23.08336743743, -21.06224101826, 3.13082909833 };
            double[] c = { 0.3374754822726147, 0.9761690190917186, 0.1607979714918209,
                0.0276438810333863,0.0038405729373609,
                0.0003951896511919,0.0000321767881768,0.0000002888167364,0.0000003960315187};
            double x = u - 0.5;
            double r;
            if (Math.Abs(x) < 0.42) // Beasley—Springer
            {
                double y = x * x;
                r = x * (((a[3] * y + a[2]) * y + a[1]) * y + a[0]) /
                    ((((b[3] * y + b[2]) * y + b[1]) * y + b[0]) * y + 1.0);
            }
            else // Moro
            {
                r = u;
                if (x > 0.0)
                    r = 1.0 - u;
                r = Math.Log(-Math.Log(r));
                r = c[0] + r * (c[1] + r * (c[2] + r * (c[3] + r * (c[4] + r * (c[5] + r * (c[6] + r * (c[7] + r * c[8])))))));
                if (x < 0.0)
                    r = -r;
            }
            return r;
        }
        // standard normal cumulative distribution function
        static public double CumulativeNormal(double x)
        {
            double[] a = { 0.319381530, -0.356563782, 1.781477937, -1.821255978, 1.330274429 };
            double result;
            if (x < -7.0)
                result = NormalDensity(x) / Math.Sqrt(1.0 + x * x);
            else
            {
                if (x > 7.0)
                    result = 1.0 - CumulativeNormal(-x);
                else
                {
                    double tmp = 1.0 / (1.0 + 0.2316419 * Math.Abs(x));
                    result = 1 - NormalDensity(x) * (tmp * (a[0] + tmp * (a[1] + 
                        tmp * (a[2] + tmp * (a[3] + tmp * a[4])))));
                    if (x <= 0.0)
                        result = 1.0 - result;
                }
            }
            return result;
        }
        static public double np(double x)
        {
            double A = 1.0 / Math.Sqrt(2.0 * 3.1415);
            return A * Math.Exp(-x * x * 0.5); // Math class in C#
        }
        static public double N(double x)
        { // The approximation to the cumulative normal distribution
            double a1 = 0.4361836;
            double a2 = -0.1201676;
            double a3 = 0.9372980;
            double k = 1.0 / (1.0 + (0.33267 * x));
            if (x >= 0.0)
            {
                return 1.0 - np(x) * (a1 * k + (a2 * k * k) + (a3 * k * k * k));
            }
            else
            {
                return 1.0 - N(-x);
            }
        }

        static public double Pz(double p, double w, double z)
        {
            double num = InverseCumulativeNormal(p) - w * z;
            double pG = CumulativeNormal(num / Math.Sqrt(1 - w * w));
            return pG;
        }

        static public double choose(double n, double k)
        {
            if (k == 0) return 1;
            return (n * choose(n - 1, k - 1)) / k;
        }

        static public double CMFz(double z, double Rho, double p, double N, double K)
        {
            double pg = Pz(p, Rho, z);
            double f = choose(N, K) * Math.Pow(pg, K) * Math.Pow(1 - pg, N - K);
            return f;
        }

        static public double LogL(double[] Rho_p, int T, double[] nVec, double[] kVec, double[] x, double[] y)
        {
            double Rho = Rho_p[0];
            double p = Rho_p[1];
            double[] L = new double[T];

            for (int i = 0; i < T; i++)
            {
                double[] ss = new double[x.Length];
                for (int j = 0; j < x.Length; j++)
                {
                    ss[j] = CMFz(x[j], Rho, p, nVec[i], kVec[i]);
                }
                L[i] =Math.Log( Enumerable.Range(0, x.Length).Sum( k => ss[k] * y[k]) );
            }
            return -L.Sum();
        }

        static public double CMFzSterling(double z, double Rho, double p, double N, double K)
        {
            double pg = Pz(p, Rho, z);
            double f  = lncombin(N, K) + Math.Log(pg) * K + Math.Log(1 - pg)* (N - K);
            double ef = Math.Exp(f);
            return ef;
        }

        static public double lncombin(double n, double k)
        {
            double res = 0.0;
            double positive = choose(n, k);
            if (double.IsInfinity(positive))
            {
                if (k == 0)
                {
                    res = 0;
                }
                else
                {
                    double lfn = (n + 0.5) * Math.Log(n) - n + 0.5 * Math.Log(2 * Math.PI);
                    double lfk = (k + 0.5) * Math.Log(k) - k + 0.5 * Math.Log(2 * Math.PI);
                    double lfnk = ((n - k) + 0.5) * Math.Log(n - k) - (n - k) + 0.5 * Math.Log(2 * Math.PI);
                    res = lfn - lfk - lfnk;
                }
            }

            else
            {
                res = Math.Log(choose(n, k));
            }
            return res;
        }

        static public double LogLSterling(double[] Rho_p, int T, double[] nVec, double[] kVec, double[] x, double[] y)
        {
            double Rho = Rho_p[0];
            double p = Rho_p[1];
            double[] L = new double[T];

            for (int i = 0; i < T; i++)
            {
                double[] ss = new double[x.Length];
                for (int j = 0; j < x.Length; j++)
                {
                    ss[j] = CMFzSterling(x[j], Rho, p, nVec[i], kVec[i]);
                }
                L[i] = Math.Log(Enumerable.Range(0, x.Length).Sum(k => ss[k] * y[k]));
            }
            return -L.Sum();
        }


        static public double CMFzSterling2Factors(double z, double Rho1, double p1, double Rho2, double p2, double N1, double K1, double N2, double K2)
        {
            double pg1 = Pz(p1, Rho1, z);
            double pg2 = Pz(p2, Rho2, z);

            double f1 = lncombin(N1, K1) + Math.Log(pg1) * K1 + Math.Log(1 - pg1) * (N1 - K1);
            double f2 = lncombin(N2, K2) + Math.Log(pg2) * K2 + Math.Log(1 - pg2) * (N2 - K2);

            double ef = Math.Exp(f1 + f2);
            return ef;
        }

        static public double LogLSterling2Factors(double[] Rho_p, int T, double[] nVec1, double[] kVec1, double[] nVec2, double[] kVec2, double[] x, double[] y)
        {
            double Rho1 = Rho_p[0];
            double p1 = Rho_p[1];

            double Rho2 = Rho_p[2];
            double p2 = Rho_p[3];

            double[] L = new double[T];

            for (int i = 0; i < T; i++)
            {
                double[] ss = new double[x.Length];
                for (int j = 0; j < x.Length; j++)
                {
                    ss[j] = CMFzSterling2Factors(x[j], Rho1, p1, Rho2, p2, nVec1[i], kVec1[i], nVec2[i], kVec2[i]);
                }
                L[i] = Math.Log(Enumerable.Range(0, x.Length).Sum(k => ss[k] * y[k]));
            }
            return -L.Sum();
        }
    }
};
