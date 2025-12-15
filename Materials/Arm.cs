using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materials
{
    public static class ArmeringMainProp
    {

        public static Dictionary<string, (double Agt, double Fyk,double Eud)> Armering = new Dictionary<string, (double, double, double)>
        {
            { "B500", new(25e-3, 500,5e-3) },
            { "B500A", new(25e-3, 1.01*500,10e-3) },
            { "B500B", new(50e-3, 1.02 * 500, 20e-3) },
            { "B500C", new(75e-3, 1.04 * 500, 30e-3) },

        };

        public static List<string> ArmeringsListe => Armering.Keys.ToList();
    }

    public struct ArmeringsMaterial
    {

    
        private readonly double E_s=200e3;
        private readonly double Agt;
        private readonly double Fyk;
        private readonly double epsilon_yd;
        public  double Fyd;
        public ArmeringsMaterial(string label)
        {
            if (!ArmeringMainProp.Armering.TryGetValue(label, out var values))
                throw new ArgumentException($"Invalid concrete class label: {label}");
            Agt = values.Item1;
            Fyk = values.Item2;
            epsilon_yd = values.Item3;
        }
        public double GetAgt() => Agt;
        public double GetFyk() => Fyk;
        public double GetEud() => epsilon_yd;
        public void SetFydULS(DesignSituation situation)
        {
            var factors = DesignSituationFactors.GetFactors(situation);
            Fyd= Fyk / factors.GammaS;
        }

        public double stal_spenningULS(double y)
        {
            if (Math.Abs(E_s * y) > Fyd)
                return Math.Sign(y) * Fyd;
            else
                return E_s * y;
            //if (Math.Abs(y) > epsilon_yd)
            //    return Math.Sign(y) * Fyd;

            //return E_s * y;
        }

        public double stal_spenningSLS(double y)
        {
            // For SLS, use pure elastic behavior (no yield limit)
            // Stress is limited by strain compatibility
            return E_s * y;
        }

        /// <summary>
        /// Get stress-strain function based on design situation
        /// </summary>
        public Func<double, double> GetStressStrainFunction(DesignSituation situation)
        {
            return situation == DesignSituation.SLS ? stal_spenningSLS : stal_spenningULS;
        }
    }
}
