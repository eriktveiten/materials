using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Materials
{
    public static class SpennMainProp
    {
        // Dictionary med standard spennstålegenskaper fra EC2
        public static readonly Dictionary<string, (double fpk, double fp01k, double ep, double epsilonUk)> Spennstaal
            = new Dictionary<string, (double, double, double, double)>
        {
            // Vaier (Strands) - Y1860S7
            { "Y1860S7", (1860, 1640, 195000, 0.035) },  // fpk=1860 MPa, fp0.1k=1600 MPa, Ep=195 GPa

            // Vaier (Strands) - Y1770S7
            { "Y1770S7", (1770, 1520, 195000, 0.035) },

            // Enkelttråd (Wire) - Y1670W
            { "Y1670W", (1670, 1420, 205000, 0.035) },

            // Enkelttråd (Wire) - Y1570W
            { "Y1570W", (1570, 1330, 205000, 0.035) },

            // Stenger (Bars) - Y1030H
            { "Y1030H", (1030, 835, 200000, 0.035) },

            // Stenger (Bars) - Y1050H
            { "Y1050H", (1050, 900, 200000, 0.035) }
        };

        public static List<string> SpennListe => Spennstaal.Keys.ToList();
    }

    /// <summary>
    /// Spennarmering material properties according to Eurocode 2
    /// </summary>
    public struct SpennMaterial
    {
        // Karakteristiske verdier
        private readonly double Fpk;        // Characteristic tensile strength [MPa]
        private readonly double Fp01k;      // Characteristic 0.1% proof stress [MPa]
        private readonly double Ep;         // Modulus of elasticity [MPa]
        private readonly double EpsilonUk;  // Characteristic strain at max load

        // Designverdier
        public double Fpd;        // Design strength [MPa]
        public double Fp01d;      // Design 0.1% proof stress [MPa]

        // Forspennkraft og tap
        public double Pm0;                // Initial prestress force [kN] - per tendon
        public double DeltaPImmediate;     // Immediate losses [kN]
        public double DeltaPLongTerm;      // Long-term losses [kN]

        /// <summary>
        /// Constructor for prestressing material
        /// </summary>
        /// <param name="materialLabel">Material designation (e.g., "Y1860S7")</param>
        public SpennMaterial(string label)
        {
            if (!SpennMainProp.Spennstaal.TryGetValue(label, out var props))
                throw new ArgumentException($"Invalid prestressing steel label: {label}");

            Fpk = props.fpk;
            Fp01k = props.fp01k;
            Ep = props.ep;
            EpsilonUk = props.epsilonUk;

            // Initialiser designverdier
            Fpd = Fpk;
            Fp01d = Fp01k;

            // Initialiser forspennkraft og tap
            Pm0 = 0;
            DeltaPImmediate = 0;
            DeltaPLongTerm = 0;
        }

        /// <summary>
        /// Set design strength for ULS based on design situation
        /// </summary>
        public void SetFpdULS(DesignSituation designSituation)
        {
            var factors = DesignSituationFactors.GetFactors(designSituation);
            Fpd = Fpk / factors.GammaSpenn;
            Fp01d = Fp01k / factors.GammaSpenn;
        }

        /// <summary>
        /// Stress-strain relationship for prestressing steel at ULS
        /// Simplified bilinear model (EC2 Figure 3.10)
        /// </summary>
        public double spenn_spenningULS(double epsilon)
        {
            // Tøyning ved 0.1% proof stress
            double epsilon_p01 = Fp01d / Ep;

            if (Math.Abs(epsilon) <= epsilon_p01)
            {
                // Elastisk område
                return epsilon * Ep;
            }
            else
            {
                // Plastisk område - forenklet horisontal flyt
                // (I virkeligheten har spennstål en hardt hardende kurve, men dette er konservativt)
                return Math.Sign(epsilon) * Fp01d;
            }
        }

        /// <summary>
        /// Stress-strain relationship for prestressing steel at SLS
        /// Linear elastic
        /// </summary>
        public double spenn_spenningSLS(double epsilon)
        {
            return epsilon * Ep;
        }

        /// <summary>
        /// Get stress-strain function based on design situation
        /// </summary>
        public Func<double, double> GetStressStrainFunction(DesignSituation situation)
        {
            if (situation == DesignSituation.SLS)
                return spenn_spenningSLS;
            else
                return spenn_spenningULS;
        }

        /// <summary>
        /// Get effective prestress after losses
        /// </summary>
        /// <param name="includeTimeDependent">Include long-term losses</param>
        public double GetEffectivePrestress(bool includeTimeDependent = true)
        {
            double losses = DeltaPImmediate;
            if (includeTimeDependent)
                losses += DeltaPLongTerm;

            return Pm0 - losses;
        }

        /// <summary>
        /// Calculate initial stress in prestressing steel
        /// </summary>
        public double GetInitialStress(double Ap)
        {
            if (Ap <= 0) return 0;
            return (Pm0 * 1000) / Ap;  // Convert kN to N, divide by mm²
        }

        /// <summary>
        /// Calculate effective stress after losses
        /// </summary>
        public double GetEffectiveStress(double Ap, bool includeTimeDependent = true)
        {
            if (Ap <= 0) return 0;
            return (GetEffectivePrestress(includeTimeDependent) * 1000) / Ap;
        }

        // Getter methods
        public double GetFpk() => Fpk;
        public double GetFp01k() => Fp01k;
        public double GetEp() => Ep;
        public double GetEpsilonUk() => EpsilonUk;
        public double GetFpd() => Fpd;

        /// <summary>
        /// Simple loss estimation according to EC2
        /// </summary>
        public static class LossEstimation
        {
            /// <summary>
            /// Estimate immediate losses (friction + anchorage slip + elastic deformation)
            /// </summary>
            public static double EstimateImmediateLosses(double Pm0, double length, double mu = 0.19, double k = 0.01, double wobble = 0.005)
            {
                // Simplified: 5-10% for pretensioned, 10-15% for post-tensioned
                // User should input more accurate values based on detailed analysis
                return Pm0 * 0.10; // 10% default
            }

            /// <summary>
            /// Estimate long-term losses (creep + shrinkage + relaxation)
            /// </summary>
            public static double EstimateLongTermLosses(double Pm0)
            {
                // Simplified: typically 10-20% of initial prestress
                // User should input more accurate values based on detailed analysis
                return Pm0 * 0.15; // 15% default
            }
        }
    }
}
