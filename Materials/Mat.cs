using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Materials
{
    public class PartialSafetyFactors
    {

        public double GammaC { get; set; } // Concrete
        public double GammaS { get; set; } // Reinforcement steel
        public double GammaSpenn { get; set; } // Prestressing steel

        public PartialSafetyFactors(double gammaC, double gammaS, double gammaSpenn)
        {
            GammaC = gammaC;
            GammaS = gammaS;
            GammaSpenn = gammaSpenn;
        }


    }

    public static class DesignSituationFactors
    {
        
        public static readonly Dictionary<DesignSituation, PartialSafetyFactors> Factors
            = new Dictionary<DesignSituation, PartialSafetyFactors>
        {
        { DesignSituation.SLS, new PartialSafetyFactors(1.00, 1.00, 1.00) },
        { DesignSituation.PermanentOrTransient, new PartialSafetyFactors(1.50, 1.15, 1.15) },
        { DesignSituation.Fatigue,              new PartialSafetyFactors(1.50, 1.15, 1.15) },
        { DesignSituation.Accidental,           new PartialSafetyFactors(1.20, 1.00, 1.00) }
        };

        public static PartialSafetyFactors GetFactors(DesignSituation situation) =>
        Factors[situation];
    }


    public enum DesignSituation
    {
        SLS,
        PermanentOrTransient,
        Fatigue,
        Accidental
    }
}
