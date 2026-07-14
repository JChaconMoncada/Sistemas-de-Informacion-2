using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Sistema_contable.ViewModels;

namespace SistemaContableZulay.UI.Services
{
    public class ExportacionService
    {
        public ExportacionService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // ═══════════════════════════════════════════════════════════════
        //  HELPERS DE ESTILO PARA PDF (QuestPDF)
        // ═══════════════════════════════════════════════════════════════

        static IContainer CellStyle(IContainer c) =>
            c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignCenter().AlignMiddle();

        static IContainer NumericCellStyle(IContainer c) =>
            c.Border(1).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().AlignMiddle();

        static IContainer HeaderCellStyle(IContainer c) =>
            c.Border(1).BorderColor(Colors.Grey.Medium).Background(Colors.Grey.Lighten3)
             .Padding(3).AlignCenter().AlignMiddle();

        // ═══════════════════════════════════════════════════════════════
        //  LIBRO DIARIO — PDF
        // ═══════════════════════════════════════════════════════════════

        public void ExportarLibroDiarioAPdf(List<LineaLibroDiario> lineas, string rutaArchivo,
            string nombreEmpresa, DateTime? desde = null, DateTime? hasta = null, string tipo = null)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1.5f, Unit.Centimetre);

                        page.Header().Column(col =>
                        {
                            col.Item().Text("LIBRO DIARIO").FontSize(16).Bold();
                            col.Item().Text($"Empresa: {nombreEmpresa ?? ""}").FontSize(10);
                            var periodo = (desde.HasValue || hasta.HasValue)
                                ? $"Período: {(desde.HasValue ? desde.Value.ToString("dd/MM/yyyy") : "inicio")} al {(hasta.HasValue ? hasta.Value.ToString("dd/MM/yyyy") : "hoy")}"
                                : "Período: Todos los registros";
                            col.Item().Text(periodo).FontSize(10);
                            if (!string.IsNullOrEmpty(tipo) && tipo != "Todos")
                                col.Item().Text($"Tipo: {tipo}").FontSize(10);
                            col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}  |  Total registros: {lineas.Count}").FontSize(9).Italic();
                        });

                        page.Content().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(70);
                                cols.ConstantColumn(45);
                                cols.ConstantColumn(80);
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.ConstantColumn(80);
                                cols.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("Fecha").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Nº").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Código").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Cuenta").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Glosa").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Debe").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Haber").Bold();
                            });

                            foreach (var l in lineas)
                            {
                                table.Cell().Element(CellStyle).Text(l.Fecha.ToString("dd/MM/yyyy"));
                                table.Cell().Element(CellStyle).Text(l.Numero.ToString());
                                table.Cell().Element(CellStyle).Text(l.CodigoCuenta ?? "");
                                table.Cell().Element(CellStyle).Text(l.NombreCuenta ?? "");
                                table.Cell().Element(CellStyle).Text(l.Descripcion ?? "");
                                table.Cell().Element(NumericCellStyle).Text(l.Debe.ToString("N2"));
                                table.Cell().Element(NumericCellStyle).Text(l.Haber.ToString("N2"));
                            }

                            var totalDebe = lineas.Sum(x => x.Debe);
                            var totalHaber = lineas.Sum(x => x.Haber);
                            table.Cell().ColumnSpan(5).Element(HeaderCellStyle).Text("TOTALES").Bold();
                            table.Cell().Element(NumericCellStyle).Text(totalDebe.ToString("N2")).Bold();
                            table.Cell().Element(NumericCellStyle).Text(totalHaber.ToString("N2")).Bold();
                        });

                        page.Footer().AlignRight().Text(x =>
                        {
                            x.Span("Página "); x.CurrentPageNumber();
                        });
                    });
                });

                document.GeneratePdf(rutaArchivo);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el PDF: {ex.Message}", ex);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  LIBRO DIARIO — EXCEL (ClosedXML)
        // ═══════════════════════════════════════════════════════════════

        public void ExportarLibroDiarioAExcel(List<LineaLibroDiario> lineas, string rutaArchivo,
            string nombreEmpresa, DateTime? desde = null, DateTime? hasta = null, string tipo = null)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Libro Diario");

            ws.Cell(1, 1).Value = "LIBRO DIARIO";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"Empresa: {nombreEmpresa ?? ""}";
            var periodo = (desde.HasValue || hasta.HasValue)
                ? $"Período: {(desde.HasValue ? desde.Value.ToString("dd/MM/yyyy") : "inicio")} al {(hasta.HasValue ? hasta.Value.ToString("dd/MM/yyyy") : "hoy")}"
                : "Período: Todos los registros";
            ws.Cell(3, 1).Value = periodo;
            if (!string.IsNullOrEmpty(tipo) && tipo != "Todos")
                ws.Cell(4, 1).Value = $"Tipo: {tipo}";
            ws.Cell(5, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            int row = 7;
            string[] headers = { "Fecha", "Nº Asiento", "Código Cuenta", "Cuenta", "Glosa", "Debe (Bs.)", "Haber (Bs.)" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = ws.Cell(row, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            row++;
            int dataStart = row;
            foreach (var l in lineas)
            {
                ws.Cell(row, 1).Value = l.Fecha.ToString("dd/MM/yyyy");
                ws.Cell(row, 2).Value = l.Numero;
                ws.Cell(row, 3).Value = l.CodigoCuenta ?? "";
                ws.Cell(row, 4).Value = l.NombreCuenta ?? "";
                ws.Cell(row, 5).Value = l.Descripcion ?? "";
                ws.Cell(row, 6).Value = l.Debe;
                ws.Cell(row, 7).Value = l.Haber;
                row++;
            }

            row++;
            ws.Cell(row, 5).Value = "TOTALES:";
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 6).FormulaA1 = $"SUM(F{dataStart}:F{row - 1})";
            ws.Cell(row, 7).FormulaA1 = $"SUM(G{dataStart}:G{row - 1})";
            var totalRow = ws.Range(row, 5, row, 7);
            totalRow.Style.Font.Bold = true;
            totalRow.Style.Fill.BackgroundColor = XLColor.LightYellow;

            ws.Range(dataStart, 6, row, 7).Style.NumberFormat.Format = "#,##0.00";
            ws.Columns().AdjustToContents();
            wb.SaveAs(rutaArchivo);
        }

        // ═══════════════════════════════════════════════════════════════
        //  LIBRO MAYOR — PDF
        // ═══════════════════════════════════════════════════════════════

        public void ExportarLibroMayorAPdf(List<LineaLibroMayor> lineas, string rutaArchivo,
            string nombreEmpresa, DateTime? desde = null, DateTime? hasta = null, string cuenta = null)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1.5f, Unit.Centimetre);

                        page.Header().Column(col =>
                        {
                            col.Item().Text("LIBRO MAYOR").FontSize(16).Bold();
                            col.Item().Text($"Empresa: {nombreEmpresa ?? ""}").FontSize(10);
                            var periodoMayor = (desde.HasValue || hasta.HasValue)
                                ? $"Período: {(desde.HasValue ? desde.Value.ToString("dd/MM/yyyy") : "inicio")} al {(hasta.HasValue ? hasta.Value.ToString("dd/MM/yyyy") : "hoy")}"
                                : "Período: Todos los registros";
                            col.Item().Text(periodoMayor).FontSize(10);
                            if (!string.IsNullOrEmpty(cuenta))
                                col.Item().Text($"Cuenta: {cuenta}").FontSize(10);
                            col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}  |  Total movimientos: {lineas.Count}").FontSize(9).Italic();
                        });

                        page.Content().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(70);
                                cols.ConstantColumn(45);
                                cols.ConstantColumn(75);
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                                cols.ConstantColumn(75);
                                cols.ConstantColumn(75);
                                cols.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderCellStyle).Text("Fecha").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Nº").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Código").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Cuenta").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Descripción").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Debe").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Haber").Bold();
                                header.Cell().Element(HeaderCellStyle).Text("Saldo").Bold();
                            });

                            foreach (var l in lineas)
                            {
                                table.Cell().Element(CellStyle).Text(l.Fecha.ToString("dd/MM/yyyy"));
                                table.Cell().Element(CellStyle).Text(l.NumeroAsiento.ToString());
                                table.Cell().Element(CellStyle).Text(l.CodigoCuenta ?? "");
                                table.Cell().Element(CellStyle).Text(l.NombreCuenta ?? "");
                                table.Cell().Element(CellStyle).Text(l.Descripcion ?? "");
                                table.Cell().Element(NumericCellStyle).Text(l.Debe.ToString("N2"));
                                table.Cell().Element(NumericCellStyle).Text(l.Haber.ToString("N2"));
                                var saldoText = l.Saldo.ToString("N2");
                                if (l.Saldo < 0)
                                    table.Cell().Element(NumericCellStyle).Text(saldoText).FontColor(Colors.Red.Medium);
                                else
                                    table.Cell().Element(NumericCellStyle).Text(saldoText);
                            }
                        });

                        page.Footer().AlignRight().Text(x =>
                        {
                            x.Span("Página "); x.CurrentPageNumber();
                        });
                    });
                });

                document.GeneratePdf(rutaArchivo);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el PDF: {ex.Message}", ex);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  LIBRO MAYOR — EXCEL (ClosedXML)
        // ═══════════════════════════════════════════════════════════════

        public void ExportarLibroMayorAExcel(List<LineaLibroMayor> lineas, string rutaArchivo,
            string nombreEmpresa, DateTime? desde = null, DateTime? hasta = null, string cuenta = null)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Libro Mayor");

            ws.Cell(1, 1).Value = "LIBRO MAYOR";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = $"Empresa: {nombreEmpresa ?? ""}";
            var periodo = (desde.HasValue || hasta.HasValue)
                ? $"Período: {(desde.HasValue ? desde.Value.ToString("dd/MM/yyyy") : "inicio")} al {(hasta.HasValue ? hasta.Value.ToString("dd/MM/yyyy") : "hoy")}"
                : "Período: Todos los registros";
            ws.Cell(3, 1).Value = periodo;
            if (!string.IsNullOrEmpty(cuenta))
                ws.Cell(4, 1).Value = $"Cuenta: {cuenta}";
            ws.Cell(5, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            int row = 7;
            string[] headers = { "Fecha", "Nº Asiento", "Código", "Cuenta", "Descripción", "Debe (Bs.)", "Haber (Bs.)", "Saldo Acumulado (Bs.)" };
            for (int col = 0; col < headers.Length; col++)
            {
                var cell = ws.Cell(row, col + 1);
                cell.Value = headers[col];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            row++;
            int dataStart = row;
            foreach (var l in lineas)
            {
                ws.Cell(row, 1).Value = l.Fecha.ToString("dd/MM/yyyy");
                ws.Cell(row, 2).Value = l.NumeroAsiento;
                ws.Cell(row, 3).Value = l.CodigoCuenta ?? "";
                ws.Cell(row, 4).Value = l.NombreCuenta ?? "";
                ws.Cell(row, 5).Value = l.Descripcion ?? "";
                ws.Cell(row, 6).Value = l.Debe;
                ws.Cell(row, 7).Value = l.Haber;
                ws.Cell(row, 8).Value = l.Saldo;
                if (l.Saldo < 0)
                    ws.Cell(row, 8).Style.Font.FontColor = XLColor.Red;
                row++;
            }

            row++;
            ws.Cell(row, 7).Value = "SALDO FINAL:";
            ws.Cell(row, 7).Style.Font.Bold = true;
            ws.Cell(row, 8).Value = lineas.Count > 0 ? lineas.Last().Saldo : 0;
            ws.Cell(row, 8).Style.Font.Bold = true;
            ws.Range(row, 7, row, 8).Style.Fill.BackgroundColor = XLColor.LightYellow;

            ws.Range(dataStart, 6, row, 8).Style.NumberFormat.Format = "#,##0.00";
            ws.Columns().AdjustToContents();
            wb.SaveAs(rutaArchivo);
        }

        // ═══════════════════════════════════════════════════════════════
        //  INFORME — PDF
        // ═══════════════════════════════════════════════════════════════

        public void ExportarInformeAPdf(List<LineaReporte> lineas, string titulo, string subtitulo,
            string rutaArchivo, string nombreEmpresa, string notas = null, string conclusiones = null)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);

                        page.Header().Column(col =>
                        {
                            col.Item().Text(titulo ?? "INFORME").FontSize(16).Bold();
                            col.Item().Text(subtitulo ?? "").FontSize(11);
                            col.Item().Text($"Empresa: {nombreEmpresa ?? ""}").FontSize(10);
                            col.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).Italic();
                        });

                        page.Content().PaddingTop(10).Column(col =>
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn();
                                    cols.ConstantColumn(110);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCellStyle).Text("Cuenta / Concepto").Bold();
                                    header.Cell().Element(HeaderCellStyle).Text("Monto (Bs.)").Bold();
                                });

                                foreach (var l in lineas)
                                {
                                    if (l.EsEncabezado)
                                    {
                                        table.Cell().ColumnSpan(2)
                                            .Background(Colors.Grey.Lighten2).Padding(4)
                                            .Text(l.Nombre ?? "").Bold().FontSize(11);
                                    }
                                    else if (l.EsTotal)
                                    {
                                        table.Cell().Element(HeaderCellStyle).Text(l.Nombre ?? "").Bold();
                                        table.Cell().Element(NumericCellStyle).Text($"Bs. {l.Monto:N2}").Bold();
                                    }
                                    else
                                    {
                                        table.Cell().Element(CellStyle).Text(l.Nombre ?? "");
                                        table.Cell().Element(NumericCellStyle).Text($"Bs. {l.Monto:N2}");
                                    }
                                }
                            });

                            if (!string.IsNullOrEmpty(notas))
                            {
                                col.Item().PaddingTop(15).Column(c =>
                                {
                                    c.Item().Text("NOTAS EXPLICATIVAS").Bold().FontSize(11);
                                    c.Item().PaddingTop(4).Text(notas).Italic().FontSize(10);
                                });
                            }

                            if (!string.IsNullOrEmpty(conclusiones))
                            {
                                col.Item().PaddingTop(15).Column(c =>
                                {
                                    c.Item().Text("CONCLUSIONES Y RECOMENDACIONES").Bold().FontSize(11);
                                    c.Item().PaddingTop(4).Text(conclusiones).FontSize(10);
                                });
                            }
                        });

                        page.Footer().AlignRight().Text(x =>
                        {
                            x.Span("Página "); x.CurrentPageNumber();
                        });
                    });
                });

                document.GeneratePdf(rutaArchivo);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el PDF: {ex.Message}", ex);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        //  INFORME — EXCEL (ClosedXML)
        // ═══════════════════════════════════════════════════════════════

        public void ExportarInformeAExcel(List<LineaReporte> lineas, string titulo, string subtitulo,
            string rutaArchivo, string nombreEmpresa, string notas = null, string conclusiones = null)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Informe");

            ws.Cell(1, 1).Value = titulo ?? "INFORME";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(2, 1).Value = subtitulo ?? "";
            ws.Cell(3, 1).Value = $"Empresa: {nombreEmpresa ?? ""}";
            ws.Cell(4, 1).Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

            int row = 6;
            ws.Cell(row, 1).Value = "Cuenta / Concepto";
            ws.Cell(row, 2).Value = "Monto (Bs.)";
            var headerRange = ws.Range(row, 1, row, 2);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.LightSteelBlue;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            row++;
            int dataStart = row;
            foreach (var l in lineas)
            {
                if (l.EsEncabezado)
                {
                    var encRange = ws.Range(row, 1, row, 2);
                    encRange.Merge();
                    encRange.FirstCell().Value = l.Nombre ?? "";
                    encRange.Style.Font.Bold = true;
                    encRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                    row++;
                }
                else if (l.EsTotal)
                {
                    ws.Cell(row, 1).Value = l.Nombre ?? "";
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 2).Value = l.Monto;
                    ws.Cell(row, 2).Style.Font.Bold = true;
                    ws.Range(row, 1, row, 2).Style.Fill.BackgroundColor = XLColor.LightYellow;
                    row++;
                }
                else
                {
                    ws.Cell(row, 1).Value = l.Nombre ?? "";
                    ws.Cell(row, 2).Value = l.Monto;
                    row++;
                }
            }

            if (!string.IsNullOrEmpty(notas))
            {
                row += 2;
                ws.Cell(row, 1).Value = "NOTAS EXPLICATIVAS";
                ws.Cell(row, 1).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = notas;
                ws.Cell(row, 1).Style.Font.Italic = true;
                ws.Row(row).Height = 40;
                ws.Cell(row, 1).Style.Alignment.WrapText = true;
            }

            if (!string.IsNullOrEmpty(conclusiones))
            {
                row += 2;
                ws.Cell(row, 1).Value = "CONCLUSIONES Y RECOMENDACIONES";
                ws.Cell(row, 1).Style.Font.Bold = true;
                row++;
                ws.Cell(row, 1).Value = conclusiones;
                ws.Row(row).Height = 40;
                ws.Cell(row, 1).Style.Alignment.WrapText = true;
            }

            ws.Range(dataStart, 2, row, 2).Style.NumberFormat.Format = "#,##0.00";
            ws.Columns().AdjustToContents();
            wb.SaveAs(rutaArchivo);
        }
    }
}
