using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Default_and_Asset_Correlation_Testing
{
    public abstract class CACorr
    {
        private double[] x;
        private double[] y;
        //private double rho;
        //private double p;
        private int size;
        //private int lengthofx;

        private double h;
        private int sizex;
        private int lowerbound;

        public int Sizex{get { return this.sizex; }}
        public int Size { get { return this.size; } }
        public int LowerBound { get { return this.lowerbound; } }
        public double H { get { return this.h; } }


        public double[] X { get { return this.x; } }
        public double[] Y { get { return this.y; } }

        public abstract double LogL(double[] Rho_p);

        public void LogL_f(double[] Rho_p, ref double func, object obj)
        {
            func = this.LogL(Rho_p);
        }

        public CACorr(double[] NumberOfCredits_, double[] NumberOfDefaultedCredits_)
        {
            this.size = NumberOfCredits_.Length;
            this.h = 0.1;
            this.sizex = 101;
            this.lowerbound = -5;

            this.x = new double[this.sizex];
            this.y = new double[this.sizex];
        }
    }

    public class CACorr_OneFactor : CACorr // CreditAssetCorrelation One factor
    {
        private double[] NumberOfCredits;
        private double[] NumberOfDefaultedCredits;

        public CACorr_OneFactor(double[] NumberOfCredits_, double[] NumberOfDefaultedCredits_) : base(NumberOfCredits_, NumberOfDefaultedCredits_)
        {
            this.NumberOfCredits = NumberOfCredits_;
            this.NumberOfDefaultedCredits = NumberOfDefaultedCredits_;

            for (int i = 0; i < this.Sizex; i++)
            {
                this.X[i] = this.LowerBound + i * this.H;
                this.Y[i] = SpecialFunctions.NormalDensity(this.X[i]) * this.H;
            }
        }

        public override double LogL(double[] Rho_p)
        {
            double Rho = Rho_p[0];
            double p = Rho_p[1];
            double[] L = new double[this.Size];

            for (int i = 0; i < this.Size; i++)
            {
                double[] ss = new double[X.Length];
                for (int j = 0; j < X.Length; j++)
                {
                    //ss[j] = SpecialFunctions.CMFz(this.X[j], Rho, p, this.NumberOfCredits[i], this.NumberOfDefaultedCredits[i]);
                    ss[j] = SpecialFunctions.CMFzSterling(this.X[j], Rho, p, this.NumberOfCredits[i], this.NumberOfDefaultedCredits[i]);
                }
                L[i] = Math.Log(Enumerable.Range(0, this.Sizex).Sum(k => ss[k] * this.Y[k]));
            }
            return -L.Sum();
        }
    }

    public class CACorr_TwoFactor : CACorr // CreditAssetCorrelation Two factor
    {
        private double[] NumberOfCredits;
        private double[] NumberOfDefaultedCredits;

        private double[] NumberOfCredits2;
        private double[] NumberOfDefaultedCredits2;


        public CACorr_TwoFactor(double[] NumberOfCredits_, double[] NumberOfDefaultedCredits_, double[] NumberOfCredits_2, double[] NumberOfDefaultedCredits_2) : 
            base(NumberOfCredits_, NumberOfDefaultedCredits_)
        {
            this.NumberOfCredits = NumberOfCredits_;
            this.NumberOfDefaultedCredits = NumberOfDefaultedCredits_;
            this.NumberOfCredits2 = NumberOfCredits_2;
            this.NumberOfDefaultedCredits2 = NumberOfDefaultedCredits_2;

            for (int i = 0; i < this.Sizex; i++)
            {
                this.X[i] = this.LowerBound + i * this.H;
                this.Y[i] = SpecialFunctions.NormalDensity(this.X[i]) * this.H;
            }
        }

        public override double LogL(double[] Rho_p)
        {                
            double Rho1 = Rho_p[0];
            double p1 = Rho_p[1];

            double Rho2 = Rho_p[2];
            double p2 = Rho_p[3];

            double[] L = new double[this.Size];

            for (int i = 0; i < this.Size; i++)
            {
                double[] ss = new double[X.Length];
                for (int j = 0; j < X.Length; j++)
                {
                    ss[j] = SpecialFunctions.CMFzSterling2Factors(X[j], Rho1, p1, Rho2, p2, 
                        this.NumberOfCredits[i], this.NumberOfDefaultedCredits[i], this.NumberOfCredits2[i], this.NumberOfDefaultedCredits2[i]);
                }
                L[i] = Math.Log(Enumerable.Range(0, this.Sizex).Sum(k => ss[k] * this.Y[k]));
            }
            return -L.Sum();
        }
    }
}


