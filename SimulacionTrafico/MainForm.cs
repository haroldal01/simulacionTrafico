using System;
using System.Drawing;
using System.Windows.Forms;
using SimulacionTrafico.Models;
namespace SimulacionTrafico
{
    public partial class MainForm : Form
    {
        private RedVial _redVial;
        private Timer _timerSimulacion;
        private int _contadorTiempo = 0;
        private Random _random = new Random();

        public MainForm()
        {
            InitializeComponent();
            _redVial = new RedVial();
        }

        private void DibujarInterseccion(Graphics g, Interseccion inter, int centerX, int centerY, int radio)
        {
            Point posicion = ObtenerPosicion(inter.Id, centerX, centerY);
            int congestion = inter.ObtenerCongestion();
            Color color = congestion == 0 ? Color.Green :
                         congestion < 5 ? Color.Yellow : Color.Red;

            g.FillEllipse(new SolidBrush(color), posicion.X - radio, posicion.Y - radio, radio * 2, radio * 2);
            g.DrawEllipse(Pens.Black, posicion.X - radio, posicion.Y - radio, radio * 2, radio * 2);
            g.DrawString(inter.Id, Font, Brushes.Black, posicion.X - 15, posicion.Y - 10);
        }

        private void DibujarConexiones(Graphics g, Interseccion inter, int centerX, int centerY)
        {
            Point pInter = ObtenerPosicion(inter.Id, centerX, centerY);

            if (inter.NorteAdyacente != null)
                g.DrawLine(new Pen(Color.Blue, 2), pInter, ObtenerPosicion(inter.NorteAdyacente.Id, centerX, centerY));

            if (inter.SurAdyacente != null)
                g.DrawLine(new Pen(Color.Blue, 2), pInter, ObtenerPosicion(inter.SurAdyacente.Id, centerX, centerY));

            if (inter.EsteAdyacente != null)
                g.DrawLine(new Pen(Color.Red, 2), pInter, ObtenerPosicion(inter.EsteAdyacente.Id, centerX, centerY));

            if (inter.OesteAdyacente != null)
                g.DrawLine(new Pen(Color.Red, 2), pInter, ObtenerPosicion(inter.OesteAdyacente.Id, centerX, centerY));
        }

        private void TimerSimulacion_Tick(object sender, EventArgs e)
        {
            _contadorTiempo++;

            // 1. Cambiar semáforos periódicamente
            if (_contadorTiempo % 5 == 0) // Cada 5 segundos
            {
                var nodoActual = _redVial.Intersecciones.PrimerNodo;
                while (nodoActual != null)
                {
                    nodoActual.Interseccion.CambiarSemaforo();
                    nodoActual = _redVial.Intersecciones.ObtenerSiguiente(nodoActual);
                }
            }

            // 2. Mover vehículos según semáforos
            MoverVehiculos();

            // 3. Generar nuevos vehículos aleatoriamente (20% de probabilidad)
            if (_random.Next(0, 100) < 20)
            {
                GenerarVehiculoAleatorio();
            }

            // 4. Actualizar la interfaz
            ActualizarUI();
            panelMapa.Invalidate(); // Redibujar el mapa
        }

        private void MoverVehiculos()
        {
            var nodoActual = _redVial.Intersecciones.PrimerNodo;
            while (nodoActual != null)
            {
                var inter = nodoActual.Interseccion;

                if (inter.SemaforoNorteSur)
                {
                    // Mover de norte a sur
                    MoverEntreColas(inter.Norte, inter.SurAdyacente?.Sur);

                    // Mover de sur a norte
                    MoverEntreColas(inter.Sur, inter.NorteAdyacente?.Norte);
                }
                else
                {
                    // Mover de este a oeste
                    MoverEntreColas(inter.Este, inter.OesteAdyacente?.Oeste);

                    // Mover de oeste a este
                    MoverEntreColas(inter.Oeste, inter.EsteAdyacente?.Este);
                }

                nodoActual = _redVial.Intersecciones.ObtenerSiguiente(nodoActual);
            }
        }

        private void MoverEntreColas(ColaVehiculos origen, ColaVehiculos destino)
        {
            if (origen.Cantidad > 0 && destino != null)
            {
                var vehiculo = origen.Desencolar();
                destino.Encolar(vehiculo);
            }
        }

        private void GenerarVehiculoAleatorio()
        {
            if (_redVial.Intersecciones.Cantidad < 2) return;

            // Seleccionar intersección origen aleatoria
            int indiceOrigen = _random.Next(0, _redVial.Intersecciones.Cantidad);
            var origen = _redVial.Intersecciones.ObtenerPorIndice(indiceOrigen);

            // Seleccionar intersección destino diferente
            int indiceDestino;
            do
            {
                indiceDestino = _random.Next(0, _redVial.Intersecciones.Cantidad);
            } while (indiceDestino == indiceOrigen);
            var destino = _redVial.Intersecciones.ObtenerPorIndice(indiceDestino);

            // Crear y agregar vehículo
            var vehiculo = new Vehiculo(_random.Next(1000, 9999), $"{origen.Id}→{destino.Id}");

            string[] direcciones = { "norte", "sur", "este", "oeste" };
            string direccion = direcciones[_random.Next(0, 4)];

            origen.AgregarVehiculo(vehiculo, direccion);
        }

        private void ActualizarUI()
        {
            var masCongestionada = _redVial.ObtenerInterseccionMasCongestionada();
            lblCongestion.Text = $"Tiempo: {_contadorTiempo}s | Mayor congestión: {masCongestionada?.Id ?? "N/A"} ({masCongestionada?.ObtenerCongestion() ?? 0} vehículos)";
        }



        private Point ObtenerPosicion(string id, int centerX, int centerY)
        {
            int distancia = 100;
            if (id == "Centro") return new Point(centerX, centerY);
            if (id == "Norte") return new Point(centerX, centerY - distancia);
            if (id == "Sur") return new Point(centerX, centerY + distancia);
            if (id == "Este") return new Point(centerX + distancia, centerY);
            if (id == "Oeste") return new Point(centerX - distancia, centerY);
            return new Point(centerX, centerY);
        }

        private void panelMapa_Paint(object sender, PaintEventArgs e)
        {
            if (_redVial?.Intersecciones == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(System.Drawing.Color.White);

            int centerX = panelMapa.Width / 2;
            int centerY = panelMapa.Height / 2;
            int radio = 50;

            //dibujar las concexiones
            var nodoActual = _redVial.Intersecciones.PrimerNodo;
            while (nodoActual != null)
            {
                if (nodoActual.Interseccion != null)
                {
                    DibujarConexiones(g, nodoActual.Interseccion, centerX, centerY);
                }
                nodoActual = _redVial.Intersecciones.ObtenerSiguiente(nodoActual);
            }
            // Dibujar las intersecciones
            nodoActual = _redVial.Intersecciones.PrimerNodo;
            while (nodoActual != null)
            {
                if (nodoActual.Interseccion != null)
                {
                    DibujarInterseccion(g, nodoActual.Interseccion, centerX, centerY, radio);
                }
                nodoActual = _redVial.Intersecciones.ObtenerSiguiente(nodoActual);
            }

        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            // Inicializar la red vial si no está creada
            if (_redVial == null)
            {
                _redVial = new RedVial();
            }

            // Configurar el temporizador
            if (_timerSimulacion == null)
            {
                _timerSimulacion = new Timer();
                _timerSimulacion.Interval = 1000; 
                _timerSimulacion.Tick += TimerSimulacion_Tick;
            }

            // Iniciar la simulación
            _timerSimulacion.Start();
            btnIniciar.Enabled = false;
            btnDetener.Enabled = true;
            lblCongestion.Text = "Simulación en curso...";
        }

        private void btnDetener_Click(object sender, EventArgs e)
        {
            // Detener la simulación
            if (_timerSimulacion != null)
            {
                _timerSimulacion.Stop();
            }

            btnIniciar.Enabled = true;
            btnDetener.Enabled = false;

            // Mostrar estadísticas finales
            var masCongestionada = _redVial.ObtenerInterseccionMasCongestionada();
            lblCongestion.Text = $"Simulación detenida. Mayor congestión: {masCongestionada?.Id ?? "N/A"}";
        }
    }
}
