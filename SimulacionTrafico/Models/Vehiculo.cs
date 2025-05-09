using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulacionTrafico.Models
{
    // Vehiculo.cs
    public class Vehiculo
    {
        public int Id { get; }
        public string Ruta { get; }
        public int TiempoEspera { get; set; }
        public DateTime StartTime { get; } // Time when vehicle enters simulation

        public Vehiculo(int id, string ruta)
        {
            Id = id;
            Ruta = ruta;
            TiempoEspera = 0;
            StartTime = DateTime.Now;
        }

        public double GetTotalTravelTime()
        {
            return (DateTime.Now - StartTime).TotalSeconds;
        }
    }
}
