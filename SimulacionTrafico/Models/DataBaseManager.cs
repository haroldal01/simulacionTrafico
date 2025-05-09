using System;
using System.Data.SQLite;
using System.IO;

namespace SimulacionTrafico.Models
{
    public class DatabaseManager
    {
        private readonly string _dbPath;

        public DatabaseManager(string dbPath = "Data/SimulacionTrafico.db")
        {
            _dbPath = dbPath;
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            // Ensure the directory exists
            string directory = Path.GetDirectoryName(_dbPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Create the database file if it doesn't exist
            if (!File.Exists(_dbPath))
            {
                SQLiteConnection.CreateFile(_dbPath);
            }

            // Connect to the database and create tables
            using (var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
            {
                connection.Open();

                // Create Vehicles table
                string createVehiclesTable = @"
                    CREATE TABLE IF NOT EXISTS Vehicles (
                        Id INTEGER PRIMARY KEY,
                        Ruta TEXT NOT NULL,
                        TiempoEspera INTEGER NOT NULL
                    )";
                using (var command = new SQLiteCommand(createVehiclesTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create Intersections table
                string createIntersectionsTable = @"
                    CREATE TABLE IF NOT EXISTS Intersections (
                        Id TEXT PRIMARY KEY,
                        SemaforoNorteSur BOOLEAN NOT NULL,
                        NorteCount INTEGER NOT NULL,
                        SurCount INTEGER NOT NULL,
                        EsteCount INTEGER NOT NULL,
                        OesteCount INTEGER NOT NULL
                    )";
                using (var command = new SQLiteCommand(createIntersectionsTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Create SimulationLog table
                string createSimulationLogTable = @"
                    CREATE TABLE IF NOT EXISTS SimulationLog (
                        EventId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Timestamp DATETIME NOT NULL,
                        InterseccionId TEXT,
                        Evento TEXT NOT NULL,
                        VehiculoId INTEGER,
                        FOREIGN KEY (InterseccionId) REFERENCES Intersections(Id),
                        FOREIGN KEY (VehiculoId) REFERENCES Vehicles(Id)
                    )";
                using (var command = new SQLiteCommand(createSimulationLogTable, connection))
                {
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        public void InsertVehicle(Vehiculo vehiculo)
        {
            using (var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
            {
                connection.Open();
                string insertQuery = @"
                INSERT INTO Vehicles (Id, Ruta, TiempoEspera)
                VALUES (@Id, @Ruta, @TiempoEspera)";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", vehiculo.Id);
                    command.Parameters.AddWithValue("@Ruta", vehiculo.Ruta);
                    command.Parameters.AddWithValue("@TiempoEspera", vehiculo.TiempoEspera);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void UpdateIntersection(Interseccion interseccion)
        {
            using (var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
            {
                connection.Open();
                string upsertQuery = @"
                INSERT OR REPLACE INTO Intersections (
                    Id, SemaforoNorteSur, NorteCount, SurCount, EsteCount, OesteCount
                )
                VALUES (@Id, @SemaforoNorteSur, @NorteCount, @SurCount, @EsteCount, @OesteCount)";
                using (var command = new SQLiteCommand(upsertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Id", interseccion.Id);
                    command.Parameters.AddWithValue("@SemaforoNorteSur", interseccion.SemaforoNorteSur ? 1 : 0);
                    command.Parameters.AddWithValue("@NorteCount", interseccion.Norte.Cantidad);
                    command.Parameters.AddWithValue("@SurCount", interseccion.Sur.Cantidad);
                    command.Parameters.AddWithValue("@EsteCount", interseccion.Este.Cantidad);
                    command.Parameters.AddWithValue("@OesteCount", interseccion.Oeste.Cantidad);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public void LogEvent(string interseccionId, string evento, int? vehiculoId = null)
        {
            using (var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
            {
                connection.Open();
                string insertQuery = @"
                INSERT INTO SimulationLog (Timestamp, InterseccionId, Evento, VehiculoId)
                VALUES (@Timestamp, @InterseccionId, @Evento, @VehiculoId)";
                using (var command = new SQLiteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
                    command.Parameters.AddWithValue("@InterseccionId", interseccionId);
                    command.Parameters.AddWithValue("@Evento", evento);
                    command.Parameters.AddWithValue("@VehiculoId", vehiculoId.HasValue ? (object)vehiculoId.Value : DBNull.Value);
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
    }
}