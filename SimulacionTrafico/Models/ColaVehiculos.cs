using SimulacionTrafico.Models.SimulacionTrafico.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulacionTrafico.Models
{
    public class ColaVehiculos
    {
        private NodoVehiculo _primero;
        private NodoVehiculo _ultimo;
        private int _cantidad;

        public int Cantidad => _cantidad;
        public NodoVehiculo Primero => _primero;


        public void Encolar(Vehiculo vehiculo)
        {
            NodoVehiculo nuevo = new NodoVehiculo(vehiculo);

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

        public Vehiculo Desencolar()
        {
            if (_primero == null) return null;

            Vehiculo vehiculo = _primero.Vehiculo;
            _primero = _primero.Siguiente;

            if (_primero == null)
                _ultimo = null;

            _cantidad--;
            return vehiculo;
        }
    }
}
