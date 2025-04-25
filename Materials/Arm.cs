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
}
