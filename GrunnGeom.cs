using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SjekkInterface
{
    public class GrunnGeom
    {

        private double Areal {  get; set; }
        private double Omkrets { get; set; }
        private double ForsteArealMoment { get; set; }
        private double AndreArealMoment { get; set; }


        protected void SetAreal(double a) => Areal = a;
        protected void SetOmkrets(double o) => Omkrets = o;
    }





    public class Rektangel : GrunnGeom
    {
        public double Bredde {  get; set; }
        public double Hoyde { get; set; }


        public Rektangel()
        {
            
        }

        public Rektangel(double bredde,double hoyde)
        {
            this.Bredde = bredde;
            this.Hoyde = hoyde;
          
            
        }

        public void CalculateGeom() 
        {
        
            SetAreal(Bredde*Hoyde);
            SetOmkrets(2 * (Bredde + Hoyde));

        }




    }
}
