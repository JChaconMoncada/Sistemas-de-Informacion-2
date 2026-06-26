using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

using System.IO;
using System.Xml.Serialization;
using SistemaContableZulay.UI.Domain;

namespace SistemaContableZulay.UI.Services
{
    public class IpcService
    {
        public static IpcService Instance { get; } = new IpcService();
        private readonly HttpClient _httpClient;
        private readonly string _ipcFile;

        // Simulador de base de datos de IPC histórica
        private Dictionary<DateTime, decimal> _ipcHistorico = new();

        private IpcService()
        {
            _httpClient = new HttpClient();
            // Timeout corto para que si no hay internet o falla, pase rápido al local
            _httpClient.Timeout = TimeSpan.FromSeconds(3);

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var datosDir = Path.Combine(baseDir, "Datos");
            if (!Directory.Exists(datosDir)) Directory.CreateDirectory(datosDir);
            
            _ipcFile = Path.Combine(datosDir, "ipc_historico.xml");
            CargarIpcHistorico();
        }

        private void CargarIpcHistorico()
        {
            if (File.Exists(_ipcFile))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(List<IpcRecord>));
                    using var reader = new StreamReader(_ipcFile);
                    var registros = (List<IpcRecord>)serializer.Deserialize(reader);
                    if (registros != null)
                    {
                        _ipcHistorico = registros.ToDictionary(r => r.Fecha, r => r.Valor);
                    }
                }
                catch { /* Si falla la carga, iniciamos vacío o con default */ }
            }

            // Precargar valores default si está vacío
            if (!_ipcHistorico.Any())
            {
                _ipcHistorico = new Dictionary<DateTime, decimal>
                {
                    { new DateTime(2023, 1, 1), 1250.50m },
                    { new DateTime(2023, 2, 1), 1300.75m },
                    { new DateTime(2023, 3, 1), 1350.20m },
                    { new DateTime(2023, 12, 1), 1580.75m },
                    { new DateTime(2024, 1, 1), 1620.00m },
                    { new DateTime(2024, 5, 1), 1800.50m }
                };
                GuardarIpcHistorico();
            }
        }

        private void GuardarIpcHistorico()
        {
            try
            {
                var registros = _ipcHistorico.Select(kv => new IpcRecord { Fecha = kv.Key, Valor = kv.Value }).ToList();
                var serializer = new XmlSerializer(typeof(List<IpcRecord>));
                using var writer = new StreamWriter(_ipcFile);
                serializer.Serialize(writer, registros);
            }
            catch { /* Manejo silencioso */ }
        }

        public async Task<decimal> ObtenerIpcAsync(DateTime fecha)
        {
            var fechaBuscada = new DateTime(fecha.Year, fecha.Month, 1);

            // Si ya lo tenemos en el historial guardado localmente, lo usamos directo (Offline-First)
            if (_ipcHistorico.TryGetValue(fechaBuscada, out decimal valorExistente))
            {
                return valorExistente;
            }

            // 1. Intentar obtener el IPC desde una API externa real
            try
            {
                string url = $"https://api.example.bcv.org.ve/ipc?mes={fecha.Month}&anio={fecha.Year}";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Suponiendo que la API devuelve { "valor": 1234.56 }
                    var result = JsonSerializer.Deserialize<Dictionary<string, decimal>>(content);
                    if (result != null && result.ContainsKey("valor"))
                    {
                        var valorNuevo = result["valor"];
                        // Guardarlo para el futuro
                        _ipcHistorico[fechaBuscada] = valorNuevo;
                        GuardarIpcHistorico();
                        return valorNuevo;
                    }
                }
            }
            catch
            {
                // Falla de red, API caída o URL de prueba. Caen al fallback de interpolación.
            }

            // 2. Fallback / Interpolación (Flujo Alternativo 3a)
            // Si la API falló y NO teníamos la fecha exacta, interpolamos.

            // Interpolación o búsqueda más cercana si no existe el mes exacto
            var anterior = _ipcHistorico.Where(x => x.Key < fechaBuscada).OrderByDescending(x => x.Key).FirstOrDefault();
            var posterior = _ipcHistorico.Where(x => x.Key > fechaBuscada).OrderBy(x => x.Key).FirstOrDefault();

            if (anterior.Key != default && posterior.Key != default)
            {
                // Promedio simple como interpolación lineal básica
                return (anterior.Value + posterior.Value) / 2m;
            }

            if (anterior.Key != default) return anterior.Value;
            if (posterior.Key != default) return posterior.Value;

            return 1000m; // Base por defecto
        }
    }
}
