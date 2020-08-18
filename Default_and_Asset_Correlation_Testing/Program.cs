using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Reflection;


namespace Default_and_Asset_Correlation_Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            // Read data
            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string NewPath       = Path.GetFullPath(Path.Combine(currentDirectory, @"..\..\"));
            StreamReader reader  = new StreamReader(File.OpenRead(NewPath + "DefaultHistory.csv"));

            var Col1 = new List<string>();
            var Col2 = new List<string>();
            var Col3 = new List<string>();
            var Col4 = new List<string>();
            var Col5 = new List<string>();

            while (!reader.EndOfStream)
            {
                var splits = reader.ReadLine().Split(',');
                Col1.Add(splits[0]);
                Col2.Add(splits[1]);
                Col3.Add(splits[2]);
                Col4.Add(splits[3]);
                Col5.Add(splits[4]);
            }
            // store data
            int size = Col1.Count - 1;

            double[] Col1_Array = new double[size];
            double[] Col2_Array = new double[size];
            double[] Col3_Array = new double[size];
            double[] Col4_Array = new double[size];
            double[] Col5_Array = new double[size];
           
            for (int i = 0; i < size; i++)
            {
                Col1_Array[i] = double.Parse(Col1[i + 1]);
                Col2_Array[i] = double.Parse(Col2[i + 1]);
                Col3_Array[i] = double.Parse(Col3[i + 1]);
                Col4_Array[i] = double.Parse(Col4[i + 1]);
                Col5_Array[i] = double.Parse(Col5[i + 1]);
            }

            int sizex = 101;
            double h = 0.1;
            double[] x = new double[sizex];
            double[] y = new double[sizex];

            for (int i = 0; i < 101; i++)
            {
                x[i] = -5 + i * h;
                y[i] = SpecialFunctions.NormalDensity(x[i])*h;
            }
            // Check the value of function CMFz
            //Console.WriteLine(SpecialFunctions.CMFz(-4, 0.5, 0.25, 10, 5));

            // IG data set
            Console.WriteLine("IG --------------------------- ");
            double[] Rho_p = new double[2];
            Rho_p[1] = 0.00121497751624143;
            Rho_p[0] = 0.276457395273792;
            Console.WriteLine(SpecialFunctions.LogL(Rho_p, size, Col4_Array, Col2_Array, x, y));

            CACorr_OneFactor CACorrIG = new CACorr_OneFactor(Col4_Array, Col2_Array);
            Console.WriteLine(CACorrIG.LogL(Rho_p));
            Console.WriteLine("--------------------------- ");

            Console.WriteLine();
            Console.WriteLine("SG --------------------------- ");
            // SG data set
            Rho_p[1] = 0.04385974;
            Rho_p[0] = 0.29139055;
            Console.WriteLine(SpecialFunctions.LogL(Rho_p, size, Col5_Array, Col3_Array, x, y)); // this will return NaN

            // Check the value of function lncombin
            //Console.WriteLine(SpecialFunctions.lncombin(2415,223));
            Console.WriteLine(SpecialFunctions.LogLSterling(Rho_p, size, Col5_Array, Col3_Array, x, y));
            CACorr_OneFactor CACorrSG = new CACorr_OneFactor(Col5_Array, Col3_Array);
            Console.WriteLine(CACorrSG.LogL(Rho_p));

            Console.WriteLine();
            Console.WriteLine("IG and SG -------------------- ");

            // IG and SG data set together
            double[] Rho_p4 = new double[4];
            Rho_p4[1] = 0.0012931;
            Rho_p4[0] = 0.20353734;
            Rho_p4[3] = 0.04347081;
            Rho_p4[2] = 0.27907251;
            Console.WriteLine(SpecialFunctions.LogLSterling2Factors(Rho_p4, size, Col4_Array, Col2_Array, Col5_Array, Col3_Array, x, y));
            CACorr_TwoFactor CACorrIGSG = new CACorr_TwoFactor(Col4_Array, Col2_Array,Col5_Array, Col3_Array);
            Console.WriteLine(CACorrIGSG.LogL(Rho_p4));

            Console.WriteLine();
            Console.WriteLine("ALGLIB to minimize - SG ");

            // ALGLIB to minimize!!
            // 1 on the SG dataset
            double[] InitialGuess = new double[] { 0.5, 0.0001 };
            double epsg = 0.00001;
            double epsf = 0.0;
            double epsx = 0.0;
            int    maxits = 30;
            double diffstep = 1.0e-6;
            double[] bndl = new double[] { 0.00001, 0.00001 };
            double[] bndu = new double[] { 0.9, 0.9 };

            double[] InitialGuess2 = new double[] { 0.5, 0.0001 };
            alglib.minbleicstate state2;
            alglib.minbleicreport rep2;
            alglib.minbleiccreatef(InitialGuess2, diffstep, out state2);
            alglib.minbleicsetcond(state2, epsg, epsf, epsx, maxits);
            alglib.minbleicsetbc(state2, bndl, bndu);
            alglib.minbleicoptimize(state2, CACorrSG.LogL_f, null, null);
            alglib.minbleicresults(state2, out InitialGuess2, out rep2);
            System.Console.WriteLine("  {0}", alglib.ap.format(InitialGuess2, 10));
            System.Console.WriteLine(CACorrSG.LogL(InitialGuess2));
            System.Console.WriteLine("--------------------------------------------------");

            // 2 on the IG dataset
            Console.WriteLine();
            Console.WriteLine("ALGLIB to minimize - IG ");

            alglib.minbleicstate state;
            alglib.minbleicreport rep;
            alglib.minbleiccreatef(InitialGuess, diffstep, out state);
            alglib.minbleicsetcond(state, epsg, epsf, epsx, maxits);
            alglib.minbleicsetbc(state, bndl, bndu);
            alglib.minbleicoptimize(state, CACorrIG.LogL_f, null, null);
            alglib.minbleicresults(state, out InitialGuess, out rep);
            System.Console.WriteLine("  {0}", alglib.ap.format(InitialGuess, 10));
            System.Console.WriteLine(CACorrIG.LogL(InitialGuess));
            System.Console.WriteLine("--------------------------------------------------");

            Console.WriteLine();
            Console.WriteLine("ALGLIB to minimize - IG and SG together ");

            // 3 on the IG and SG dataset 
            double[] InitialGuess4 = new double[] { 0.5, 0.0001, 0.5, 0.0001 };
            double[] bndl4 = new double[] { 0.00001, 0.00001, 0.00001 , 0.00001 };
            double[] bndu4 = new double[] { 0.9, 0.9, 0.9, 0.9 };

            alglib.minbleicstate state4;
            alglib.minbleicreport rep4;
            alglib.minbleiccreatef(InitialGuess4, diffstep, out state4);
            alglib.minbleicsetcond(state, epsg, epsf, epsx, maxits);
            alglib.minbleicsetbc(state4, bndl4, bndu4);
            alglib.minbleicoptimize(state4, CACorrIGSG.LogL_f, null, null);
            alglib.minbleicresults(state4, out InitialGuess4, out rep4);
            System.Console.WriteLine("  {0}", alglib.ap.format(InitialGuess4, 10));
            System.Console.WriteLine(CACorrIGSG.LogL(InitialGuess4));
            System.Console.WriteLine("--------------------------------------------------");

        }
    }
}
