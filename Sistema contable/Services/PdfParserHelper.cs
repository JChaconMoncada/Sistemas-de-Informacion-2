using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;

namespace Sistema_contable.Services
{
    public class ExtractedTransaction
    {
        public DateTime? Fecha { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Debito { get; set; }
        public decimal Credito { get; set; }
        public bool IsError { get; set; }
        public string RawLine { get; set; } = string.Empty;
    }

    public static class PdfParserHelper
    {
        public static string ExtractRawText(string filePath)
        {
            if (!File.Exists(filePath))
                return "El archivo no existe.";

            try
            {
                var sb = new StringBuilder();
                using (var pdf = PdfDocument.Open(filePath))
                {
                    foreach (var page in pdf.GetPages())
                    {
                        sb.AppendLine($"--- PÁGINA {page.Number} ---");
                        sb.AppendLine(page.Text);
                        sb.AppendLine();
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"Error al extraer texto: {ex.Message}";
            }
        }

        public static List<ExtractedTransaction> ParseBankStatement(string filePath)
        {
            var transactions = new List<ExtractedTransaction>();

            if (!File.Exists(filePath))
                return transactions;

            try
            {
                using (var pdf = PdfDocument.Open(filePath))
                {
                    foreach (var page in pdf.GetPages())
                    {
                        var words = page.GetWords().ToList();
                        if (!words.Any()) continue;

                        // Agrupar palabras que estén en la misma línea visual (tolerancia de ~3 puntos en el eje Y)
                        // BoundingBox.Bottom representa la base de las letras. Ordenamos de mayor a menor para ir de arriba a abajo en la página.
                        var lineGroups = words.GroupBy(w => Math.Round(w.BoundingBox.Bottom / 3.0))
                                              .OrderByDescending(g => g.Key);

                        foreach (var group in lineGroups)
                        {
                            // Ordenamos de izquierda a derecha por coordenada X
                            var sortedWords = group.OrderBy(w => w.BoundingBox.Left).Select(w => w.Text);
                            var trimmedLine = string.Join(" ", sortedWords).Trim();
                            
                            if (string.IsNullOrWhiteSpace(trimmedLine))
                                continue;

                            // Intentamos parsear la línea
                            var tx = TryParseLineByTokens(trimmedLine);
                            if (tx != null)
                            {
                                transactions.Add(tx);
                            }
                            else
                            {
                                // Si no se pudo parsear, se agrega como error para que el usuario la corrija
                                transactions.Add(new ExtractedTransaction
                                {
                                    IsError = true,
                                    RawLine = trimmedLine
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // En caso de error general, se podría manejar aquí
            }

            return transactions;
        }

        private static ExtractedTransaction? TryParseLineByTokens(string line)
        {
            // Separar la línea por espacios, omitiendo los vacíos
            var tokens = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (tokens.Count < 3) 
                return null;

            // Buscamos un token que coincida exactamente con una fecha (dd/MM/yyyy o dd-MM-yyyy)
            int dateIdx = -1;
            for (int i = 0; i < tokens.Count; i++)
            {
                if (Regex.IsMatch(tokens[i], @"^\d{2}[/\-]\d{2}[/\-]\d{4}$"))
                {
                    dateIdx = i;
                    break;
                }
            }

            if (dateIdx == -1) 
                return null;

            // Parsear fecha
            if (!DateTime.TryParseExact(tokens[dateIdx], new[] { "dd/MM/yyyy", "dd-MM-yyyy" }, 
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime fecha))
            {
                return null;
            }

            // Extraer la referencia y la descripción de los tokens anteriores a la fecha
            string referencia = string.Empty;
            int startDesc = 0;

            if (dateIdx > 0)
            {
                // Si el primer token consiste puramente de dígitos y tiene un largo razonable (referencia)
                if (Regex.IsMatch(tokens[0], @"^\d+$") && tokens[0].Length >= 4)
                {
                    referencia = tokens[0];
                    startDesc = 1;
                }
            }

            // Unir los tokens restantes de la descripción
            string descripcion = string.Empty;
            if (dateIdx > startDesc)
            {
                descripcion = string.Join(" ", tokens.Skip(startDesc).Take(dateIdx - startDesc));
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                descripcion = "Transacción Bancaria";
            }

            // Extraer números (Débito, Crédito, Saldo) de los tokens que están después de la fecha
            var postDateTokens = tokens.Skip(dateIdx + 1).ToList();
            var decimalTokens = new List<decimal>();

            foreach (var token in postDateTokens)
            {
                // Si el token parece un número (dígitos, comas, puntos o signos menos)
                if (Regex.IsMatch(token, @"^[\d\.,\-]+$") && token.Any(char.IsDigit))
                {
                    decimalTokens.Add(ParseDecimal(token));
                }
            }

            decimal debito = 0;
            decimal credito = 0;

            if (decimalTokens.Count >= 2)
            {
                // Usualmente el formato trae: Débito, Crédito, Saldo
                // Tomaremos los últimos 3 si los hay, o los 2 si sólo hay dos
                if (decimalTokens.Count >= 3)
                {
                    debito = decimalTokens[decimalTokens.Count - 3];
                    credito = decimalTokens[decimalTokens.Count - 2];
                }
                else
                {
                    debito = decimalTokens[decimalTokens.Count - 2];
                    credito = decimalTokens[decimalTokens.Count - 1];
                }
            }
            else if (decimalTokens.Count == 1)
            {
                // Solo encontramos un número
                debito = decimalTokens[0];
            }
            else
            {
                // No se encontraron valores numéricos
                return null;
            }

            if (debito == 0 && credito == 0)
            {
                return null;
            }

            return new ExtractedTransaction
            {
                Fecha = fecha,
                Referencia = referencia,
                Descripcion = descripcion,
                Debito = Math.Abs(debito),
                Credito = Math.Abs(credito),
                IsError = false,
                RawLine = line
            };
        }

        private static decimal ParseDecimal(string value)
        {
            if (string.IsNullOrEmpty(value)) return 0;

            value = value.Trim();
            value = value.Replace("-", "");

            if (value.Contains(",") && value.Contains("."))
            {
                if (value.IndexOf(',') > value.IndexOf('.'))
                {
                    value = value.Replace(".", "").Replace(",", ".");
                }
                else
                {
                    value = value.Replace(",", "");
                }
            }
            else if (value.Contains(","))
            {
                if (value.IndexOf(',') == value.Length - 3)
                {
                    value = value.Replace(",", ".");
                }
                else
                {
                    value = value.Replace(",", "");
                }
            }

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }

            return 0;
        }
    }
}
