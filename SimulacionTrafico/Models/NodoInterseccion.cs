using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulacionTrafico.Models
{
    public class NodoInterseccion
    {
        public Interseccion Interseccion { get; set; }
        public NodoInterseccion Siguiente { get; set; }

        public NodoInterseccion(Interseccion interseccion)
        {
            Interseccion = interseccion;
            Siguiente = null;
        }
    }
}
