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
            try
            {
                
                string directory = Path.GetDirectoryName(_dbPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

          
                if (!File.Exists(_dbPath))
                {
                    SQLiteConnection.CreateFile(_dbPath);
                }

            
                using (var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                {
                    connection.Open();

            
                    string createVehiclesTable = @"
                        CREATE TABLE IF NOT EXISTS Vehicles (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Ruta TEXT NOT NULL,
                            TiempoEspera INTEGER NOT NULL
                        )";
                    using (var command = new SQLiteCommand(createVehiclesTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }

   
                    string createIntersectionsTable = @"
                        CREATE TABLE IF NOT EXISTS Intersections (
                            Id TEXT PRIMARY KEY,
                            SemaforoNorteSur BOOLEAN NOT NULL,
                            NorteCount INTEGER NOT NULL,
                            SurCount INTEGER NOT NULL,
                            EsteCount INTEGER NOT NULL,
                            OesteCount INTEGER NOT NULL,
                            AverageTransitTime REAL NOT NULL DEFAULT 0,
                            NorteStreetType TEXT NOT NULL DEFAULT 'Unidirectional',
                            SurStreetType TEXT NOT NULL DEFAULT 'Unidirectional',
                            EsteStreetType TEXT NOT NULL DEFAULT 'Unidirectional',
                            OesteStreetType TEXT NOT NULL DEFAULT 'Unidirectional'
                        )";
                    using (var command = new SQLiteCommand(createIntersectionsTable, connection))
                    {
                        command.ExecuteNonQuery();
                    }


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
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing database: {ex.Message}");
                throw; 
            }
        }

        public void InsertVehicle(Vehiculo vehiculo)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting vehicle: {ex.Message}");
                throw;
            }
        }

        public void UpdateIntersection(Interseccion interseccion)
        {
            try
            {
                using (var connection = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                {
                    connection.Open();
                    string upsertQuery = @"
                        INSERT OR REPLACE INTO Intersections (
                            Id, SemaforoNorteSur, NorteCount, SurCount, EsteCount, OesteCount,
                            AverageTransitTime, NorteStreetType, SurStreetType, EsteStreetType, OesteStreetType
                        )
                        VALUES (@Id, @SemaforoNorteSur, @NorteCount, @SurCount, @EsteCount, @OesteCount,
                                @AverageTransitTime, @NorteStreetType, @SurStreetType, @EsteStreetType, @OesteStreetType)";
                    using (var command = new SQLiteCommand(upsertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Id", interseccion.Id);
                        command.Parameters.AddWithValue("@SemaforoNorteSur", interseccion.SemaforoNorteSur ? 1 : 0);
                        command.Parameters.AddWithValue("@NorteCount", interseccion.Norte.Cantidad);
                        command.Parameters.AddWithValue("@SurCount", interseccion.Sur.Cantidad);
                        command.Parameters.AddWithValue("@EsteCount", interseccion.Este.Cantidad);
                        command.Parameters.AddWithValue("@OesteCount", interseccion.Oeste.Cantidad);
                        command.Parameters.AddWithValue("@AverageTransitTime", interseccion.AverageTransitTime);
                        command.Parameters.AddWithValue("@NorteStreetType", interseccion.StreetTypes["norte"].ToString());
                        command.Parameters.AddWithValue("@SurStreetType", interseccion.StreetTypes["sur"].ToString());
                        command.Parameters.AddWithValue("@EsteStreetType", interseccion.StreetTypes["este"].ToString());
                        command.Parameters.AddWithValue("@OesteStreetType", interseccion.StreetTypes["oeste"].ToString());
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating intersection: {ex.Message}");
                throw;
            }
        }

        public void LogEvent(string interseccionId, string evento, int? vehiculoId = null)
        {
            try
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error logging event: {ex.Message}");
                throw;
            }
        }
    }
}