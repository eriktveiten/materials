using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materials
{
    public static class BetongMainProp
    {
        public static Dictionary<string, (int fck, int fckCube)> Betong = new Dictionary<string, (int, int)>
        {
            { "B12", new(12, 15) },
            { "B16", new(16, 20) },
            { "B20", new(20, 25) },
            { "B25", new(25, 30) },
            { "B30", new(30, 37) },
            { "B35", new(35, 45) },
            { "B40", new(40, 50) },
            { "B45", new(45, 55) },
            { "B50", new(50, 60) },
            { "B55", new(55, 67) },
            { "B60", new(60, 75) },
            { "B70", new(70, 85) },
            { "B80", new(80, 95) },
            { "B90", new(90, 105) }
        };
        public static List<string> BetongListe => Betong.Keys.ToList();


    }

    public struct BetongMaterial
    {
        private readonly int fck;
        private readonly int fckCube;

        public BetongMaterial(string label)
        {
            if (!BetongMainProp.Betong.TryGetValue(label, out var values))
                throw new ArgumentException($"Invalid concrete class label: {label}");

            fck = values.Item1;
            fckCube = values.Item2;
        }

        public int Fck => fck;
        public int FckCube => fckCube;

        public double Fcm => Fck + 8;
        public double Ecm => 22 * Math.Pow(Fcm / 10.0, 0.3); // Example from Eurocode

        public double Fctm =>
         Fck <= 50 ? 0.3 * Math.Pow(Fck, 2.0 / 3.0) : 2.12 * Math.Log(1 + Fcm / 10.0);

        public double Fctk005 => 0.7 * Fctm;
        public double Fctk095 => 1.3 * Fctm;
        public double EpsilonC1 => (Math.Min(0.7 * Math.Pow(Fcm, 0.31), 2.8)) / 1000;
        public double EpsilonCu1 =>
            Fck <= 50 ? 3.5 / 1000 : (2.8 + 27 * Math.Pow((98 - Fcm) / 100, 4)) / 1000;

        public double EpsilonC2 =>
            Fck <= 50 ? 2.0 / 1000 : (2 + 0.085 * Math.Pow(Fck - 50, 0.53)) / 1000;

        public double EpsilonCu2 =>
            Fck <= 50 ? 3.5 / 1000 : (2.6 + 35 * Math.Pow((90 - Fck) / 100.0, 4)) / 1000;

        public double StressStrainExponentN => Fck <= 50 ? 2.0 : 1.4 + 23.4 * Math.Pow((90 - Fck) / 100.0, 4);


        public double EpsilonC3 =>
            Fck <= 50 ? 1.75 / 1000 : (1.75 + 0.55 * ((Fck - 50) / 40.0)) / 1000;


        public double EpsilonCu3 =>
      Fck <= 50 ? 3.5 / 1000 : (2.6 + 35 * Math.Pow((90 - Fck) / 100.0, 4)) / 1000;

        public double FcdULS { get; set; }
        public void SetFcdULS(DesignSituation situation)
        {
            var factors = DesignSituationFactors.GetFactors(situation);
            FcdULS = 0.85*Fck / factors.GammaC;
        }
        public double FcdALS { get; set; }

        public void SetFcALS(DesignSituation situation)
        {
            var factors = DesignSituationFactors.GetFactors(situation);
            FcdALS = 0.85 * Fck / factors.GammaC;
        }

        public double betong_spenningULS(double eps)
        {
            if (eps < 0)
                return 0.0;
            
            if (eps > EpsilonC2)
                return FcdULS;

            var sigma = FcdULS * (1 - Math.Pow(1 - eps / EpsilonC2, StressStrainExponentN));
            return sigma;
        }
    }



}
