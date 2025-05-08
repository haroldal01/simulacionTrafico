using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulacionTrafico.Models
{
    public class ListaIntersecciones
    {
        private NodoInterseccion _primero;
        private NodoInterseccion _ultimo;
        private int _cantidad;

        public NodoInterseccion PrimerNodo => _primero;
        public int Cantidad => _cantidad;

        public void Agregar(Interseccion interseccion)
        {
            NodoInterseccion nuevo = new NodoInterseccion(interseccion);

            if (_ultimo == null)
            {
                _primero = nuevo;
                _ultimo = nuevo;
            }
            else
            {
                _ultimo.Siguiente = nuevo;
                _ultimo = nuevo;
            }
            _cantidad++;
        }

        public NodoInterseccion ObtenerSiguiente(NodoInterseccion nodoActual)
        {
            return nodoActual?.Siguiente;
        }


        public Interseccion ObtenerPorIndice(int indice)
        {
            if (indice < 0 || indice >= _cantidad) return null;

            NodoInterseccion actual = _primero;
            for (int i = 0; i < indice; i++)
            {
                actual = actual.Siguiente;
            }
            return actual.Interseccion;
        }
    }
}
