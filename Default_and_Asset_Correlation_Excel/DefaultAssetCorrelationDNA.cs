using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ExcelDna.Integration;
using ExcelDna.Logging;

using Default_and_Asset_Correlation_Testing;

namespace Default_and_Asset_Correlation_Excel
{
    public class DefaultAssetCorrelationDNA : IExcelAddIn
    {
        //Excel Auto Open 
        public void AutoOpen() { }
        //Excel Auto Close 
        public void AutoClose() { }

        [ExcelFunction(Description = "Estimate Asset Correlation and Probability of Default")]
        public static object[,] OneFactorEstimation(
               [ExcelArgument(Description = @"Number of Credits at beg of period as an array")] double[] Credits,
               [ExcelArgument(Description = @"Number of Defaulted Credits as an array")] double[] DefaultedCredits)
        {

            CACorr_OneFactor CACorr = new CACorr_OneFactor(Credits, DefaultedCredits);

            // ALGLIB to minimize!!
            double[] InitialGuess = new double[] { 0.5, 0.0001 };
            double epsg = 0.00001;
            double epsf = 0.0;
            double epsx = 0.0;
            int maxits = 30;
            double diffstep = 1.0e-6;
            double[] bndl = new double[] { 0.00001, 0.00001 };
            double[] bndu = new double[] { 0.9, 0.9 };

            alglib.minbleicstate state;
            alglib.minbleicreport rep;
            alglib.minbleiccreatef(InitialGuess, diffstep, out state);
            alglib.minbleicsetcond(state, epsg, epsf, epsx, maxits);
            alglib.minbleicsetbc(state, bndl, bndu);
            alglib.minbleicoptimize(state, CACorr.LogL_f, null, null);
            alglib.minbleicresults(state, out InitialGuess, out rep);

            object[,] a = new object[1, 3];
            a[0, 0] = InitialGuess[0];
            a[0, 1] = InitialGuess[1];
            a[0, 2] = CACorr.LogL(InitialGuess);

            return a;
        }

        [ExcelFunction(Description = "2 Factor Estimate Asset Correlation and Probability of Default")]
        public static object[,] TwoFactorEstimation(
               [ExcelArgument(Description = @"Number of Credits 1 at beg of period as an array")] double[] Credits1,
               [ExcelArgument(Description = @"Number of Defaulted Credits 1 as an array")] double[] DefaultedCredits1,
               [ExcelArgument(Description = @"Number of Credits 2 at beg of period as an array")] double[] Credits2,
               [ExcelArgument(Description = @"Number of Defaulted Credits 2 as an array")] double[] DefaultedCredits2)
        {

            CACorr_TwoFactor CACorr = new CACorr_TwoFactor(Credits1, DefaultedCredits1, Credits2, DefaultedCredits2);


            // ALGLIB to minimize!!
            
            double epsg = 0.00001;
            double epsf = 0.0;
            double epsx = 0.0;
            int maxits = 30;
            double diffstep = 1.0e-6;
            
            double[] InitialGuess = new double[] { 0.5, 0.0001, 0.5, 0.0001 };
            double[] bndl = new double[] { 0.00001, 0.00001, 0.00001, 0.00001 };
            double[] bndu = new double[] { 0.9, 0.9, 0.9, 0.9 };

            alglib.minbleicstate state;
            alglib.minbleicreport rep;
            alglib.minbleiccreatef(InitialGuess, diffstep, out state);
            alglib.minbleicsetcond(state, epsg, epsf, epsx, maxits);
            alglib.minbleicsetbc(state, bndl, bndu);
            alglib.minbleicoptimize(state, CACorr.LogL_f, null, null);
            alglib.minbleicresults(state, out InitialGuess, out rep);

            object[,] a = new object[1, 5];
            a[0, 0] = InitialGuess[0];
            a[0, 1] = InitialGuess[1];
            a[0, 2] = InitialGuess[2];
            a[0, 3] = InitialGuess[3];
            a[0, 4] = CACorr.LogL(InitialGuess);

            return a;
        }
    }
}
