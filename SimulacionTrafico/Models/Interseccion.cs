using System;
using System.Collections.Generic;

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
        public double AverageTransitTime { get; private set; } // tiempo promedio en cruzar
        public Dictionary<string, StreetType> StreetTypes { get; } //tipo de calle (unidireccional o bidireccional)
        private int _transitCount; // conteo de vehículos que han cruzado
        private double _totalTransitTime; //suma de tiempos de cruce

        public enum StreetType
        {
            Unidirectional,
            Bidirectional
        }

        public Interseccion(string id)
        {
            Id = id;
            Norte = new ColaVehiculos();
            Sur = new ColaVehiculos();
            Este = new ColaVehiculos();
            Oeste = new ColaVehiculos();
            SemaforoNorteSur = true;
            StreetTypes = new Dictionary<string, StreetType>
            {
                { "norte", StreetType.Bidirectional },
                { "sur", StreetType.Bidirectional },
                { "este", StreetType.Unidirectional },
                { "oeste", StreetType.Unidirectional }
            };
            AverageTransitTime = 0;
            _transitCount = 0;
            _totalTransitTime = 0;
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
                    throw new ArgumentException($"Dirección '{direccion}' no válida.");
            }
            vehiculo.TiempoEspera = 0;
        }

        public void RecordTransitTime(double transitTime)
        {
            _totalTransitTime += transitTime;
            _transitCount++;
            AverageTransitTime = _transitCount > 0 ? _totalTransitTime / _transitCount : 0;
        }
    }
}
