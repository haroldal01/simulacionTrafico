using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulacionTrafico.Models
{
    public class Vehiculo
    {
        public int Id { get; }
        public string Ruta { get; }
        public int TiempoEspera { get; set; }

        public Vehiculo(int id, string ruta)
        {
            Id = id;
            Ruta = ruta;
            TiempoEspera = 0;
        }
    }
}
