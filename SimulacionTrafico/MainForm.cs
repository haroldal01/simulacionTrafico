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
            int intersectionSize = roadWidth;
            int congestion = inter.ObtenerCongestion();
            Color color = congestion == 0 ? Color.Green :
                         congestion < 5 ? Color.Yellow : Color.Red;

            g.FillEllipse(new SolidBrush(color), posicion.X - intersectionSize / 2, posicion.Y - intersectionSize / 2, intersectionSize, intersectionSize);
            g.DrawEllipse(Pens.Black, posicion.X - intersectionSize / 2, posicion.Y - intersectionSize / 2, intersectionSize, intersectionSize);

            
            g.DrawString($"T: {inter.AverageTransitTime:F1}s", new Font("Arial", 8), Brushes.Black, posicion.X + 10, posicion.Y + 10);

            // Indica el tipo de calle
            foreach (var street in inter.StreetTypes)
            {
                Point offset;
                switch (street.Key)
                {
                    case "norte":
                        offset = new Point(0, -30);
                        break;
                    case "sur":
                        offset = new Point(0, 30);
                        break;
                    case "este":
                        offset = new Point(30, 0);
                        break;
                    case "oeste":
                        offset = new Point(-30, 0);
                        break;
                    default:
                        offset = new Point(0, 0);
                        break;
                }
                string label = street.Value == Interseccion.StreetType.Bidirectional ? "B" : "U";
                g.DrawString(label, new Font("Arial", 8), Brushes.Blue, posicion.X + offset.X, posicion.Y + offset.Y);
            }

            // dibuja los vehiculos
            int squareSize = 10;
            int laneOffset = roadWidth / 2 + 5;
            DibujarVehiculos(g, inter.Norte, posicion, 0, -laneOffset, squareSize, "norte");
            DibujarVehiculos(g, inter.Sur, posicion, 0, laneOffset, squareSize, "sur");
            DibujarVehiculos(g, inter.Este, posicion, laneOffset, 0, squareSize, "este");
            DibujarVehiculos(g, inter.Oeste, posicion, -laneOffset, 0, squareSize, "oeste");

            // dibbuja el semaforo para este
            if (inter.Id == "Este")
            {
                int lightSize = 20;
                int lightX = posicion.X + 50;
                int lightY = posicion.Y - lightSize / 2;
                Color lightColor = inter.SemaforoNorteSur ? Color.Green : Color.Red;
                g.FillRectangle(new SolidBrush(lightColor), lightX, lightY, lightSize, lightSize);
                g.DrawRectangle(Pens.Black, lightX, lightY, lightSize, lightSize);
            }
        }

        private void DibujarVehiculos(Graphics g, ColaVehiculos cola, Point center, int dx, int dy, int squareSize, string direccion)
        {
            NodoVehiculo nodo = new NodoVehiculo(null); // Temporal nodo
            nodo.Siguiente = cola.Primero; 
            int count = 0;
            while (nodo.Siguiente != null && count < cola.Cantidad)
            {
                int x, y;
                if (direccion == "norte" || direccion == "sur")
                {
                    x = center.X + dx - squareSize / 2;
                    y = center.Y + dy - squareSize / 2 - (count * (squareSize + 5)); 
                    if (direccion == "sur") y = center.Y + dy - squareSize / 2 + (count * (squareSize + 5));
                }
                else 
                {
                    x = center.X + dx - squareSize / 2 + (count * (squareSize + 5)); 
                    y = center.Y + dy - squareSize / 2;
                    if (direccion == "oeste") x = center.X + dx - squareSize / 2 - (count * (squareSize + 5));
                }
                g.FillRectangle(Brushes.Red, x, y, squareSize, squareSize); 
                g.DrawRectangle(Pens.Black, x, y, squareSize, squareSize); 
                nodo = nodo.Siguiente;
                count++;
            }
        }

        private void DibujarConexiones(Graphics g, int centerX, int centerY, int roadWidth, int roadLength)
        {
            // dibujar las lineas de la carretera
            Pen roadPen = new Pen(Color.Black, roadWidth);
            Pen lanePen = new Pen(Color.Yellow, 2);

            // Vertical 
            g.DrawLine(roadPen, centerX, centerY - roadLength, centerX, centerY + roadLength);
            g.DrawLine(lanePen, centerX, centerY - roadLength, centerX, centerY + roadLength); 

            // Horizontal
            g.DrawLine(roadPen, centerX - roadLength, centerY, centerX + roadLength, centerY);
            g.DrawLine(lanePen, centerX - roadLength, centerY, centerX + roadLength, centerY); 
        }

        private void panelMapa_Paint(object sender, PaintEventArgs e)
        {
            if (_redVial?.Intersecciones == null) return;
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(System.Drawing.Color.White);

            int centerX = panelMapa.Width / 2;
            int centerY = panelMapa.Height / 2;
            int roadWidth = 30; // ancho de la carretera
            int roadLength = 150; //largo de la carretera

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

            //cambia las luces de los semáforos basandose en la congestion
            var nodoActual = _redVial.Intersecciones.PrimerNodo;
            while (nodoActual != null)
            {
                var inter = nodoActual.Interseccion;
                int nsCongestion = inter.Norte.Cantidad + inter.Sur.Cantidad;
                int ewCongestion = inter.Este.Cantidad + inter.Oeste.Cantidad;

                
                if (_contadorTiempo % 5 == 0 && nsCongestion > ewCongestion * 1.5)
                {
                    inter.CambiarSemaforo();
                    _dbManager.UpdateIntersection(inter);
                    _dbManager.LogEvent(inter.Id, $"Semáforo cambiado a {(inter.SemaforoNorteSur ? "Norte-Sur" : "Este-Oeste")} por congestión");
                }
                else if (_contadorTiempo % 5 == 0 && ewCongestion > nsCongestion * 1.5)
                {
                    inter.CambiarSemaforo();
                    _dbManager.UpdateIntersection(inter);
                    _dbManager.LogEvent(inter.Id, $"Semáforo cambiado a {(inter.SemaforoNorteSur ? "Norte-Sur" : "Este-Oeste")} por congestión");
                }

                nodoActual = _redVial.Intersecciones.ObtenerSiguiente(nodoActual);
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
                // Find the origin intersection and direction
                NodoInterseccion interseccion = _redVial.Intersecciones.PrimerNodo;
                string direccion = null;
                while (interseccion != null)
                {
                    if (interseccion.Interseccion.Norte == origen)
                    {
                        direccion = "norte";
                        break;
                    }
                    else if (interseccion.Interseccion.Sur == origen)
                    {
                        direccion = "sur";
                        break;
                    }
                    else if (interseccion.Interseccion.Este == origen)
                    {
                        direccion = "este";
                        break;
                    }
                    else if (interseccion.Interseccion.Oeste == origen)
                    {
                        direccion = "oeste";
                        break;
                    }
                    interseccion = _redVial.Intersecciones.ObtenerSiguiente(interseccion);
                }

                if (interseccion == null || direccion == null)
                {
                    return; 
                }

                // verifica si la interseccion es unidireccional
                if (interseccion.Interseccion.StreetTypes[direccion] == Interseccion.StreetType.Unidirectional)
                {
                    Interseccion adyacente = GetAdyacente(interseccion.Interseccion, direccion);
                    if (adyacente == null)
                    {
                        return; 
                    }

                    // determina que la cola de destino es la correcta
                    ColaVehiculos expectedDestino = null;
                    switch (direccion)
                    {
                        case "norte":
                            expectedDestino = adyacente.Sur; // si se mueve hacia el norte, los vehiculos entran desde el sur
                            break;
                        case "sur":
                            expectedDestino = adyacente.Norte; //si se mueve hacia el sur, los vehiculos entran desde el norte
                            break;
                        case "este":
                            expectedDestino = adyacente.Oeste; // si se mueve hacia el este, los vehiculos entran desde el oeste
                            break;
                        case "oeste":
                            expectedDestino = adyacente.Este; // si se mueve hacia el oeste, los vehiculos entran desde el este
                            break;
                    }

                    // compara el destino esperado con el destino real
                    if (destino != expectedDestino)
                    {
                        return; //salta si no es el destino correcto
                    }
                }

                // mueve el behiculo 
                var vehiculo = origen.Desencolar();
                double transitTime = vehiculo.TiempoEspera + 1;
                destino.Encolar(vehiculo);
                interseccion?.Interseccion.RecordTransitTime(transitTime);
                _dbManager.UpdateIntersection(interseccion?.Interseccion);
                _dbManager.LogEvent(destino.ToString(), $"Vehículo {vehiculo.Id} movido a destino (Tiempo: {transitTime}s)", vehiculo.Id);
            }
        }

        private Interseccion GetAdyacente(Interseccion inter, string direccion)
        {
            switch (direccion)
            {
                case "norte":
                    return inter.NorteAdyacente;
                case "sur":
                    return inter.SurAdyacente;
                case "este":
                    return inter.EsteAdyacente;
                case "oeste":
                    return inter.OesteAdyacente;
                default:
                    return null;
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
            txtReporte.Text = _redVial.GenerarReporteCuellosDeBotella();
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
                _timerSimulacion.Interval = 1000;// tiempo de actualización
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
