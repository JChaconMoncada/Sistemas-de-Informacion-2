using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OfficeOpenXml;
using Sistema_contable.ViewModels;

namespace SistemaContableZulay.UI.Services
{
    public class ExportacionService
    {
        public ExportacionService()
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;
#pragma warning disable CS0618
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al inicializar licencias: {ex.Message}");
            }
        }

        public void ExportarLibroDiarioAPdf(List<LineaLibroDiario> lineas, string rutaArchivo, string nombreEmpresa)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);

                        page.Header().Column(column =>
                        {
                            column.Item().Text("LIBRO DIARIO").FontSize(16).Bold();
                            column.Item().Text($"Empresa: {nombreEmpresa ?? ""}").FontSize(10);
                            column.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                        });

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(80);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Fecha").Bold();
                                header.Cell().Element(CellStyle).Text("Nº").Bold();
                                header.Cell().Element(CellStyle).Text("Código").Bold();
                                header.Cell().Element(CellStyle).Text("Cuenta").Bold();
                                header.Cell().Element(CellStyle).Text("Glosa").Bold();
                                header.Cell().Element(CellStyle).Text("Debe").Bold();
                                header.Cell().Element(CellStyle).Text("Haber").Bold();
                            });

                            foreach (var linea in lineas)
                            {
                                table.Cell().Element(CellStyle).Text(linea.Fecha.ToString("dd/MM/yyyy"));
                                table.Cell().Element(CellStyle).Text(linea.Numero.ToString());
                                table.Cell().Element(CellStyle).Text(linea.CodigoCuenta ?? "");
                                table.Cell().Element(CellStyle).Text(linea.NombreCuenta ?? "");
                                table.Cell().Element(CellStyle).Text(linea.Descripcion ?? "");
                                table.Cell().Element(CellStyle).Text(linea.Debe.ToString("N2")).AlignRight();
                                table.Cell().Element(CellStyle).Text(linea.Haber.ToString("N2")).AlignRight();
                            }
                        });

                        page.Footer().AlignRight().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                    });
                });

                document.GeneratePdf(rutaArchivo);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el PDF: {ex.Message}. Asegúrese de que el archivo no esté abierto en otro programa.", ex);
            }
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(2)
                .AlignCenter()
                .AlignMiddle();
        }

        public void ExportarLibroDiarioAExcel(List<LineaLibroDiario> lineas, string rutaArchivo, string nombreEmpresa)
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Libro Diario");

                    worksheet.Cells[1, 1].Value = "LIBRO DIARIO";
                    worksheet.Cells[1, 1].Style.Font.Bold = true;
                    worksheet.Cells[2, 1].Value = $"Empresa: {nombreEmpresa ?? ""}";
                    worksheet.Cells[3, 1].Value = $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}";

                    int row = 5;
                    worksheet.Cells[row, 1].Value = "Fecha";
                    worksheet.Cells[row, 2].Value = "Nº Asiento";
                    worksheet.Cells[row, 3].Value = "Código Cuenta";
                    worksheet.Cells[row, 4].Value = "Cuenta";
                    worksheet.Cells[row, 5].Value = "Glosa";
                    worksheet.Cells[row, 6].Value = "Debe";
                    worksheet.Cells[row, 7].Value = "Haber";

                    var headerRange = worksheet.Cells[row, 1, row, 7];
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                    row++;
                    foreach (var linea in lineas)
                    {
                        worksheet.Cells[row, 1].Value = linea.Fecha.ToString("dd/MM/yyyy");
                        worksheet.Cells[row, 2].Value = linea.Numero;
                        worksheet.Cells[row, 3].Value = linea.CodigoCuenta ?? "";
                        worksheet.Cells[row, 4].Value = linea.NombreCuenta ?? "";
                        worksheet.Cells[row, 5].Value = linea.Descripcion ?? "";
                        worksheet.Cells[row, 6].Value = linea.Debe;
                        worksheet.Cells[row, 7].Value = linea.Haber;
                        row++;
                    }

                    row++;
                    worksheet.Cells[row, 5].Value = "TOTALES:";
                    worksheet.Cells[row, 5].Style.Font.Bold = true;
                    worksheet.Cells[row, 6].Formula = $"SUM(F6:F{row - 1})";
                    worksheet.Cells[row, 6].Style.Font.Bold = true;
                    worksheet.Cells[row, 7].Formula = $"SUM(G6:G{row - 1})";
                    worksheet.Cells[row, 7].Style.Font.Bold = true;

                    worksheet.Columns[6].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Columns[7].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Columns.AutoFit();

                    package.SaveAs(new FileInfo(rutaArchivo));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el Excel: {ex.Message}. Asegúrese de que el archivo no esté abierto en otro programa.", ex);
            }
        }

        public void ExportarLibroMayorAPdf(List<LineaLibroMayor> lineas, string rutaArchivo, string nombreEmpresa)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);

                        page.Header().Column(column =>
                        {
                            column.Item().Text("LIBRO MAYOR").FontSize(16).Bold();
                            column.Item().Text($"Empresa: {nombreEmpresa ?? ""}").FontSize(10);
                            column.Item().Text($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                        });

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(50);
                                columns.ConstantColumn(80);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Fecha").Bold();
                                header.Cell().Element(CellStyle).Text("Nº").Bold();
                                header.Cell().Element(CellStyle).Text("Código").Bold();
                                header.Cell().Element(CellStyle).Text("Cuenta").Bold();
                                header.Cell().Element(CellStyle).Text("Descripción").Bold();
                                header.Cell().Element(CellStyle).Text("Debe").Bold();
                                header.Cell().Element(CellStyle).Text("Haber").Bold();
                                header.Cell().Element(CellStyle).Text("Saldo").Bold();
                            });

                            foreach (var linea in lineas)
                            {
                                table.Cell().Element(CellStyle).Text(linea.Fecha.ToString("dd/MM/yyyy"));
                                table.Cell().Element(CellStyle).Text(linea.NumeroAsiento.ToString());
                                table.Cell().Element(CellStyle).Text(linea.CodigoCuenta ?? "");
                                table.Cell().Element(CellStyle).Text(linea.NombreCuenta ?? "");
                                table.Cell().Element(CellStyle).Text(linea.Descripcion ?? "");
                                table.Cell().Element(CellStyle).Text(linea.Debe.ToString("N2")).AlignRight();
                                table.Cell().Element(CellStyle).Text(linea.Haber.ToString("N2")).AlignRight();
                                table.Cell().Element(CellStyle).Text(linea.Saldo.ToString("N2")).AlignRight();
                            }
                        });

                        page.Footer().AlignRight().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                    });
                });

                document.GeneratePdf(rutaArchivo);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el PDF: {ex.Message}. Asegúrese de que el archivo no esté abierto en otro programa.", ex);
            }
        }

        public void ExportarLibroMayorAExcel(List<LineaLibroMayor> lineas, string rutaArchivo, string nombreEmpresa)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Libro Mayor");

                // Encabezados
                worksheet.Cells[1, 1].Value = "LIBRO MAYOR";
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = $"Empresa: {nombreEmpresa}";
                worksheet.Cells[3, 1].Value = $"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}";

                // Columnas
                int row = 5;
                worksheet.Cells[row, 1].Value = "Fecha";
                worksheet.Cells[row, 2].Value = "Nº Asiento";
                worksheet.Cells[row, 3].Value = "Código";
                worksheet.Cells[row, 4].Value = "Cuenta";
                worksheet.Cells[row, 5].Value = "Descripción";
                worksheet.Cells[row, 6].Value = "Debe";
                worksheet.Cells[row, 7].Value = "Haber";
                worksheet.Cells[row, 8].Value = "Saldo Acumulado";

                var headerRange = worksheet.Cells[row, 1, row, 8];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Datos
                row++;
                foreach (var linea in lineas)
                {
                    worksheet.Cells[row, 1].Value = linea.Fecha.ToString("dd/MM/yyyy");
                    worksheet.Cells[row, 2].Value = linea.NumeroAsiento;
                    worksheet.Cells[row, 3].Value = linea.CodigoCuenta;
                    worksheet.Cells[row, 4].Value = linea.NombreCuenta;
                    worksheet.Cells[row, 5].Value = linea.Descripcion;
                    worksheet.Cells[row, 6].Value = linea.Debe;
                    worksheet.Cells[row, 7].Value = linea.Haber;
                    worksheet.Cells[row, 8].Value = linea.Saldo;
                    row++;
                }

                // Saldo final
                row++;
                worksheet.Cells[row, 7].Value = "SALDO FINAL:";
                worksheet.Cells[row, 7].Style.Font.Bold = true;
                worksheet.Cells[row, 8].Formula = $"H{row - 1}";
                worksheet.Cells[row, 8].Style.Font.Bold = true;

                // Formato de columnas
                worksheet.Columns[6].Style.Numberformat.Format = "#,##0.00";
                worksheet.Columns[7].Style.Numberformat.Format = "#,##0.00";
                worksheet.Columns[8].Style.Numberformat.Format = "#,##0.00";
                worksheet.Columns.AutoFit();

                package.SaveAs(new FileInfo(rutaArchivo));
            }
        }

        public void ExportarInformeAPdf(List<LineaReporte> lineas, string titulo, string subtitulo, string rutaArchivo, string nombreEmpresa, string notas = null, string conclusiones = null)
        {
            try
            {
                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);

                        page.Header().Column(column =>
                        {
                            column.Item().Text(titulo ?? "INFORME").FontSize(16).Bold();
                            column.Item().Text(subtitulo ?? "").FontSize(10);
                            column.Item().Text($"Empresa: {nombreEmpresa ?? ""}").FontSize(10);
                            column.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10);
                        });

                        page.Content().Column(col =>
                        {
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Código").Bold();
                                    header.Cell().Element(CellStyle).Text("Cuenta").Bold();
                                    header.Cell().Element(CellStyle).Text("Monto").Bold();
                                });

                                foreach (var linea in lineas)
                                {
                                    if (linea.EsEncabezado)
                                    {
                                        table.Cell().ColumnSpan(3).Element(CellStyle).Text(linea.Nombre ?? "").Bold();
                                    }
                                    else if (linea.EsTotal)
                                    {
                                        table.Cell().Element(CellStyle).Text("");
                                        table.Cell().Element(CellStyle).Text(linea.Nombre ?? "").Bold();
                                        table.Cell().Element(CellStyle).Text(linea.Monto.ToString("N2")).AlignRight().Bold();
                                    }
                                    else
                                    {
                                        table.Cell().Element(CellStyle).Text(linea.Codigo ?? "");
                                        table.Cell().Element(CellStyle).Text(linea.Nombre ?? "");
                                        table.Cell().Element(CellStyle).Text(linea.Monto.ToString("N2")).AlignRight();
                                    }
                                }
                            });

                            if (!string.IsNullOrEmpty(notas))
                            {
                                col.Item().PaddingTop(15).Column(c =>
                                {
                                    c.Item().Text("NOTAS EXPLICATIVAS").Bold();
                                    c.Item().Text(notas).Italic();
                                });
                            }

                            if (!string.IsNullOrEmpty(conclusiones))
                            {
                                col.Item().PaddingTop(15).Column(c =>
                                {
                                    c.Item().Text("CONCLUSIONES Y RECOMENDACIONES").Bold();
                                    c.Item().Text(conclusiones);
                                });
                            }
                        });

                        page.Footer().AlignRight().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                    });
                });

                document.GeneratePdf(rutaArchivo);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar el PDF: {ex.Message}. Asegúrese de que el archivo no esté abierto en otro programa.", ex);
            }
        }

        public void ExportarInformeAExcel(List<LineaReporte> lineas, string titulo, string subtitulo, string rutaArchivo, string nombreEmpresa, string notas = null, string conclusiones = null)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Informe");

                // Encabezados
                worksheet.Cells[1, 1].Value = titulo;
                worksheet.Cells[1, 1].Style.Font.Bold = true;
                worksheet.Cells[2, 1].Value = subtitulo;
                worksheet.Cells[3, 1].Value = $"Empresa: {nombreEmpresa}";
                worksheet.Cells[4, 1].Value = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";

                // Columnas
                int row = 6;
                worksheet.Cells[row, 1].Value = "Código";
                worksheet.Cells[row, 2].Value = "Cuenta";
                worksheet.Cells[row, 3].Value = "Monto";

                var headerRange = worksheet.Cells[row, 1, row, 3];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                // Datos
                row++;
                foreach (var linea in lineas)
                {
                    if (linea.EsEncabezado)
                    {
                        worksheet.Cells[row, 2].Value = linea.Nombre;
                        worksheet.Cells[row, 2].Style.Font.Bold = true;
                        row++;
                    }
                    else if (linea.EsTotal)
                    {
                        worksheet.Cells[row, 2].Value = linea.Nombre;
                        worksheet.Cells[row, 2].Style.Font.Bold = true;
                        worksheet.Cells[row, 3].Value = linea.Monto;
                        worksheet.Cells[row, 3].Style.Font.Bold = true;
                        row++;
                    }
                    else
                    {
                        worksheet.Cells[row, 1].Value = linea.Codigo;
                        worksheet.Cells[row, 2].Value = linea.Nombre;
                        worksheet.Cells[row, 3].Value = linea.Monto;
                        row++;
                    }
                }

                if (!string.IsNullOrEmpty(notas))
                {
                    row += 2;
                    worksheet.Cells[row, 1].Value = "NOTAS EXPLICATIVAS";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    row++;
                    worksheet.Cells[row, 1].Value = notas;
                    worksheet.Cells[row, 1].Style.Font.Italic = true;
                }

                if (!string.IsNullOrEmpty(conclusiones))
                {
                    row += 2;
                    worksheet.Cells[row, 1].Value = "CONCLUSIONES Y RECOMENDACIONES";
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    row++;
                    worksheet.Cells[row, 1].Value = conclusiones;
                }
                // Formato de columnas
                worksheet.Columns[3].Style.Numberformat.Format = "#,##0.00";
                worksheet.Columns.AutoFit();

                package.SaveAs(new FileInfo(rutaArchivo));
            }
        }
    }
}
