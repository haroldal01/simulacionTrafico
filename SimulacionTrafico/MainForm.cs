using System;
using System.Drawing;
using System.Windows.Forms;
using SimulacionTrafico.Models;
using SimulacionTrafico.Models.SimulacionTrafico.Models;
namespace SimulacionTrafico
{
    public partial class MainForm : Form
    {
        private RedVial _redVial;
        private Timer _timerSimulacion;
        private int _contadorTiempo = 0;
        private Random _random = new Random();
        private DatabaseManager _dbManager;

        public MainForm()
        {
            InitializeComponent();
            _redVial = new RedVial();
            _dbManager = new DatabaseManager();
        }

        private void DibujarInterseccion(Graphics g, Interseccion inter, int centerX, int centerY, int roadWidth)
        {
            Point posicion = ObtenerPosicion(inter.Id, centerX, centerY);
            int intersectionSize = roadWidth; // Size of the intersection (same as road width for clarity)
            int congestion = inter.ObtenerCongestion();
            Color color = congestion == 0 ? Color.Green :
                         congestion < 5 ? Color.Yellow : Color.Red;

            // Draw intersection as a small circle with congestion color
            g.FillEllipse(new SolidBrush(color), posicion.X - intersectionSize / 2, posicion.Y - intersectionSize / 2, intersectionSize, intersectionSize);
            g.DrawEllipse(Pens.Black, posicion.X - intersectionSize / 2, posicion.Y - intersectionSize / 2, intersectionSize, intersectionSize);

            // Draw vehicles on the roads leading to the intersection
            int squareSize = 10;
            int laneOffset = roadWidth / 2 + 5; // Offset from the center of the road
            DibujarVehiculos(g, inter.Norte, posicion, 0, -laneOffset, squareSize, "norte"); // North
            DibujarVehiculos(g, inter.Sur, posicion, 0, laneOffset, squareSize, "sur");     // South
            DibujarVehiculos(g, inter.Este, posicion, laneOffset, 0, squareSize, "este");   // East
            DibujarVehiculos(g, inter.Oeste, posicion, -laneOffset, 0, squareSize, "oeste"); // West

            // Draw traffic light indicator for the "Este" intersection
            if (inter.Id == "Este")
            {
                int lightSize = 20;
                int lightX = posicion.X + 50;
                int lightY = posicion.Y - lightSize / 2;
                Color lightColor = inter.SemaforoNorteSur ? Color.Green : Color.Red; // Green for NS, Red for EW
                g.FillRectangle(new SolidBrush(lightColor), lightX, lightY, lightSize, lightSize);
                g.DrawRectangle(Pens.Black, lightX, lightY, lightSize, lightSize);
            }
        }

        private void DibujarVehiculos(Graphics g, ColaVehiculos cola, Point center, int dx, int dy, int squareSize, string direccion)
        {
            NodoVehiculo nodo = new NodoVehiculo(null); // Temporary node to traverse
            nodo.Siguiente = cola.Primero; // Use the public property
            int count = 0;
            while (nodo.Siguiente != null && count < cola.Cantidad)
            {
                int x, y;
                if (direccion == "norte" || direccion == "sur")
                {
                    x = center.X + dx - squareSize / 2;
                    y = center.Y + dy - squareSize / 2 - (count * (squareSize + 5)); // Stack vertically
                    if (direccion == "sur") y = center.Y + dy - squareSize / 2 + (count * (squareSize + 5));
                }
                else // este or oeste
                {
                    x = center.X + dx - squareSize / 2 + (count * (squareSize + 5)); // Stack horizontally
                    y = center.Y + dy - squareSize / 2;
                    if (direccion == "oeste") x = center.X + dx - squareSize / 2 - (count * (squareSize + 5));
                }
                g.FillRectangle(Brushes.Red, x, y, squareSize, squareSize); // Red squares for vehicles
                g.DrawRectangle(Pens.Black, x, y, squareSize, squareSize); // Outline for clarity
                nodo = nodo.Siguiente;
                count++;
            }
        }

        private void DibujarConexiones(Graphics g, int centerX, int centerY, int roadWidth, int roadLength)
        {
            // Draw straight roads forming a cross
            Pen roadPen = new Pen(Color.Black, roadWidth);
            Pen lanePen = new Pen(Color.Yellow, 2);

            // Vertical road (Norte-Sur)
            g.DrawLine(roadPen, centerX, centerY - roadLength, centerX, centerY + roadLength);
            g.DrawLine(lanePen, centerX, centerY - roadLength, centerX, centerY + roadLength); // Center lane

            // Horizontal road (Este-Oeste)
            g.DrawLine(roadPen, centerX - roadLength, centerY, centerX + roadLength, centerY);
            g.DrawLine(lanePen, centerX - roadLength, centerY, centerX + roadLength, centerY); // Center lane
        }

        private void panelMapa_Paint(object sender, PaintEventArgs e)
        {
            if (_redVial?.Intersecciones == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(System.Drawing.Color.White);

            int centerX = panelMapa.Width / 2;
            int centerY = panelMapa.Height / 2;
            int roadWidth = 30; // Width of the roads
            int roadLength = 150; // Length of each road segment

            // Draw the roads first
            DibujarConexiones(g, centerX, centerY, roadWidth, roadLength);

            // Draw intersections and vehicles
            var nodoActual = _redVial.Intersecciones.PrimerNodo;
            while (nodoActual != null)
            {
                if (nodoActual.Interseccion != null)
                {
                    DibujarInterseccion(g, nodoActual.Interseccion, centerX, centerY, roadWidth);
                }
                nodoActual = _redVial.Intersecciones.ObtenerSiguiente(nodoActual);
            }
        }

        private void TimerSimulacion_Tick(object sender, EventArgs e)
        {
            _contadorTiempo++;

            if (_contadorTiempo % 5 == 0)
            {
                var nodoActual = _redVial.Intersecciones.PrimerNodo;
                while (nodoActual != null)
                {
                    nodoActual.Interseccion.CambiarSemaforo();
                    _dbManager.UpdateIntersection(nodoActual.Interseccion);
                    nodoActual = _redVial.Intersecciones.ObtenerSiguiente(nodoActual);
                }
            }

            MoverVehiculos();
            if (_random.Next(0, 100) < 20)
            {
                GenerarVehiculoAleatorio();
            }

            ActualizarUI();
            panelMapa.Invalidate();
        }

        private void MoverVehiculos()
        {
            var nodoActual = _redVial.Intersecciones.PrimerNodo;
            while (nodoActual != null)
            {
                var inter = nodoActual.Interseccion;

                if (inter.SemaforoNorteSur)
                {
                    MoverEntreColas(inter.Norte, inter.SurAdyacente?.Sur);
                    MoverEntreColas(inter.Sur, inter.NorteAdyacente?.Norte);
                }
                else
                {
                    MoverEntreColas(inter.Este, inter.OesteAdyacente?.Oeste);
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
                _dbManager.LogEvent(destino.ToString(), $"Vehículo {vehiculo.Id} movido a destino", vehiculo.Id);
            }
        }

        private void GenerarVehiculoAleatorio()
        {
            if (_redVial.Intersecciones.Cantidad < 2) return;

            int indiceOrigen = _random.Next(0, _redVial.Intersecciones.Cantidad);
            var origen = _redVial.Intersecciones.ObtenerPorIndice(indiceOrigen);

            int indiceDestino;
            do
            {
                indiceDestino = _random.Next(0, _redVial.Intersecciones.Cantidad);
            } while (indiceDestino == indiceOrigen);
            var destino = _redVial.Intersecciones.ObtenerPorIndice(indiceDestino);

            var vehiculo = new Vehiculo(_random.Next(1000, 9999), $"{origen.Id}→{destino.Id}");
            _dbManager.InsertVehicle(vehiculo);

            string[] direcciones = { "norte", "sur", "este", "oeste" };
            string direccion = direcciones[_random.Next(0, 4)];

            origen.AgregarVehiculo(vehiculo, direccion);
            _dbManager.LogEvent(origen.Id, $"Vehículo {vehiculo.Id} creado en {direccion}", vehiculo.Id);
        }

        private void ActualizarUI()
        {
            var masCongestionada = _redVial.ObtenerInterseccionMasCongestionada();
            lblCongestion.Text = $"Tiempo: {_contadorTiempo}s | Mayor congestión: {masCongestionada?.Id ?? "N/A"} ({masCongestionada?.ObtenerCongestion() ?? 0} vehículos)";
        }

        private Point ObtenerPosicion(string id, int centerX, int centerY)
        {
            int distancia = 150; // Matches roadLength
            if (id == "Centro") return new Point(centerX, centerY);
            if (id == "Norte") return new Point(centerX, centerY - distancia);
            if (id == "Sur") return new Point(centerX, centerY + distancia);
            if (id == "Este") return new Point(centerX + distancia, centerY);
            if (id == "Oeste") return new Point(centerX - distancia, centerY);
            return new Point(centerX, centerY);
        }

        private void btnIniciar_Click(object sender, EventArgs e)
        {
            if (_redVial == null)
            {
                _redVial = new RedVial();
            }

            if (_timerSimulacion == null)
            {
                _timerSimulacion = new Timer();
                _timerSimulacion.Interval = 1000; // Faster updates for smoother animation
                _timerSimulacion.Tick += TimerSimulacion_Tick;
            }

            _timerSimulacion.Start();
            btnIniciar.Enabled = false;
            btnDetener.Enabled = true;
            lblCongestion.Text = "Simulación en curso...";
        }

        private void btnDetener_Click(object sender, EventArgs e)
        {
            if (_timerSimulacion != null)
            {
                _timerSimulacion.Stop();
            }

            btnIniciar.Enabled = true;
            btnDetener.Enabled = false;

            var masCongestionada = _redVial.ObtenerInterseccionMasCongestionada();
            lblCongestion.Text = $"Simulación detenida. Mayor congestión: {masCongestionada?.Id ?? "N/A"}";
        }
    }
}
