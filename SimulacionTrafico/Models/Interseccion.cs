using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulacionTrafico.Models
{
    public class Interseccion
    {
        public string Id { get; }
        public ColaVehiculos Norte { get; }
        public ColaVehiculos Sur { get; }
        public ColaVehiculos Este { get; }
        public ColaVehiculos Oeste { get; }
        public bool SemaforoNorteSur { get; private set; }
        public Interseccion NorteAdyacente { get; set; }
        public Interseccion SurAdyacente { get; set; }
        public Interseccion EsteAdyacente { get; set; }
        public Interseccion OesteAdyacente { get; set; }

        public Interseccion(string id)
        {
            Id = id;
            Norte = new ColaVehiculos();
            Sur = new ColaVehiculos();
            Este = new ColaVehiculos();
            Oeste = new ColaVehiculos();
            SemaforoNorteSur = true;
        }

        public void CambiarSemaforo()
        {
            SemaforoNorteSur = !SemaforoNorteSur;
        }

        public int ObtenerCongestion()
        {
            return Norte.Cantidad + Sur.Cantidad + Este.Cantidad + Oeste.Cantidad;
        }

        public void AgregarVehiculo(Vehiculo vehiculo, string direccion)
        {
            // Validar dirección y agregar a la cola correspondiente
            switch (direccion.ToLower())
            {
                case "norte":
                    Norte.Encolar(vehiculo);
                    break;
                case "sur":
                    Sur.Encolar(vehiculo);
                    break;
                case "este":
                    Este.Encolar(vehiculo);
                    break;
                case "oeste":
                    Oeste.Encolar(vehiculo);
                    break;
                default:
                    throw new ArgumentException($"Dirección '{direccion}' no válida. Use: norte, sur, este u oeste");
            }

            // Actualizar tiempo de espera del vehículo
            vehiculo.TiempoEspera = 0;
        }
    }
}
