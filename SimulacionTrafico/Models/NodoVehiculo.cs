using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulacionTrafico.Models
{
    namespace SimulacionTrafico.Models
    {
        public class NodoVehiculo
        {
            public Vehiculo Vehiculo { get; set; }
            public NodoVehiculo Siguiente { get; set; }

            public NodoVehiculo(Vehiculo vehiculo)
            {
                Vehiculo = vehiculo;
                Siguiente = null;
            }
        }
    }
}
