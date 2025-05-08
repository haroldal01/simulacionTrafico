using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulacionTrafico.Models
{
    public class RedVial
    {
        private ListaIntersecciones _intersecciones;
        private int _contadorVehiculos;
        private Random _generador;

        public ListaIntersecciones Intersecciones => _intersecciones;

        public RedVial()
        {
            _intersecciones = new ListaIntersecciones();
            _contadorVehiculos = 1;
            _generador = new Random();
            InicializarRed();
        }

        private void InicializarRed()
        {
            var centro = new Interseccion("Centro");
            var norte = new Interseccion("Norte");
            var sur = new Interseccion("Sur");
            var este = new Interseccion("Este");
            var oeste = new Interseccion("Oeste");

            centro.NorteAdyacente = norte;
            norte.SurAdyacente = centro;

            centro.SurAdyacente = sur;
            sur.NorteAdyacente = centro;

            centro.EsteAdyacente = este;
            este.OesteAdyacente = centro;

            centro.OesteAdyacente = oeste;
            oeste.EsteAdyacente = centro;

            _intersecciones.Agregar(centro);
            _intersecciones.Agregar(norte);
            _intersecciones.Agregar(sur);
            _intersecciones.Agregar(este);
            _intersecciones.Agregar(oeste);
        }

        public Interseccion ObtenerInterseccionMasCongestionada()
        {
            Interseccion masCongestionada = null;
            int maxCongestion = 0;

            var nodoActual = _intersecciones.PrimerNodo;
            while (nodoActual != null)
            {
                int congestion = nodoActual.Interseccion.ObtenerCongestion();
                if (congestion > maxCongestion)
                {
                    maxCongestion = congestion;
                    masCongestionada = nodoActual.Interseccion;
                }
                nodoActual = _intersecciones.ObtenerSiguiente(nodoActual);
            }

            return masCongestionada;
        }
    }
}
