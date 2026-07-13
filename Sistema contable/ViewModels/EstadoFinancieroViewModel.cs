using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Sistema_contable.Models;
using SistemaContableZulay.UI.Domain;
using SistemaContableZulay.UI.Services;
using Microsoft.Win32;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Sistema_contable.ViewModels
{
    public class NodoEstadoFinanciero
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal Monto { get; set; }
        public string MontoFormateado { get; set; }
        public bool EsTotal { get; set; }
        public bool EsEncabezado { get; set; }
        public bool EsCuentaPadre { get; set; }
        public ObservableCollection<NodoEstadoFinanciero> SubNodos { get; set; } = new ObservableCollection<NodoEstadoFinanciero>();
        public bool TieneHistorial { get; set; }
        public bool EsDetalleReexpresion { get; set; }
        public bool EstaExpandido { get; set; } = true;
        public string Descripcion { get; set; }
        public bool TieneDescripcion => !string.IsNullOrWhiteSpace(Descripcion);

        public HistorialReexpresion HistorialAsociado { get; set; }
        public bool PuedeDeshacer => HistorialAsociado != null;

        public string HistorialTooltipText
        {
            get
            {
                if (HistorialAsociado == null) return string.Empty;
                return $"Valor Original (Compra): Bs. {HistorialAsociado.ValorOriginal:N2}\n" +
                       $"Ajuste por Inflación: Bs. {HistorialAsociado.MontoAjuste:N2}\n" +
                       $"Fecha: {HistorialAsociado.FechaCalculo:dd/MM/yyyy}";
            }
        }
    }

    public class EstadoFinancieroViewModel : ViewModelBase
    {
        private readonly ContabilidadService _contabilidadService;

        private ObservableCollection<NodoEstadoFinanciero> _nodos;
        private DateTime _fechaInicio;
        private DateTime _fechaCorte;
        private string _tituloReporte;
        private string _subtituloReporte;
        private bool _tieneDatos;

        private ObservableCollection<string> _opcionesMoneda;
        private string _monedaSeleccionada;
        private decimal _tasaCambio;
        private bool _esTasaPersonalizada;

        public ObservableCollection<NodoEstadoFinanciero> Nodos
        {
            get => _nodos;
            set => SetProperty(ref _nodos, value);
        }

        public DateTime FechaInicio
        {
            get => _fechaInicio;
            set { if (SetProperty(ref _fechaInicio, value)) GenerarReporte(); }
        }

        public DateTime FechaCorte
        {
            get => _fechaCorte;
            set { if (SetProperty(ref _fechaCorte, value)) GenerarReporte(); }
        }

        public string TituloReporte
        {
            get => _tituloReporte;
            set => SetProperty(ref _tituloReporte, value);
        }

        public string SubtituloReporte
        {
            get => _subtituloReporte;
            set => SetProperty(ref _subtituloReporte, value);
        }

        public bool TieneDatos
        {
            get => _tieneDatos;
            set
            {
                if (SetProperty(ref _tieneDatos, value))
                {
                    OnPropertyChanged(nameof(SinDatosVisibility));
                }
            }
        }

        public System.Windows.Visibility SinDatosVisibility =>
            TieneDatos ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

        public ObservableCollection<string> OpcionesMoneda
        {
            get => _opcionesMoneda;
            set => SetProperty(ref _opcionesMoneda, value);
        }

        public string MonedaSeleccionada
        {
            get => _monedaSeleccionada;
            set
            {
                if (SetProperty(ref _monedaSeleccionada, value))
                {
                    OnPropertyChanged(nameof(MostrarTasaCambio));
                    ActualizarTasaSegunMoneda();
                }
            }
        }

        public System.Windows.Visibility MostrarTasaCambio => 
            (MonedaSeleccionada == "Bolívares (Bs)" || MonedaSeleccionada == "Bolívares (Bs) - Oficial") ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;

        public decimal TasaCambio
        {
            get => _tasaCambio;
            set
            {
                if (SetProperty(ref _tasaCambio, value))
                {
                    _tasaCambioTexto = value > 0 ? value.ToString(System.Globalization.CultureInfo.InvariantCulture) : "";
                    OnPropertyChanged(nameof(TasaCambioTexto));
                    GenerarReporte();
                }
            }
        }

        private string _tasaCambioTexto;
        public string TasaCambioTexto
        {
            get => _tasaCambioTexto;
            set
            {
                if (SetProperty(ref _tasaCambioTexto, value))
                {
                    string cleanVal = (value ?? "").Replace(",", ".");
                    if (decimal.TryParse(cleanVal, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal parsed))
                    {
                        if (_tasaCambio != parsed)
                        {
                            _tasaCambio = parsed;
                            OnPropertyChanged(nameof(TasaCambio));
                            GenerarReporte();
                        }
                    }
                    else
                    {
                        if (_tasaCambio != 0)
                        {
                            _tasaCambio = 0;
                            OnPropertyChanged(nameof(TasaCambio));
                            GenerarReporte();
                        }
                    }
                }
            }
        }

        public bool EsTasaPersonalizada
        {
            get => _esTasaPersonalizada;
            set => SetProperty(ref _esTasaPersonalizada, value);
        }

        public ICommand GenerarReporteCommand { get; }
        public ICommand RestaurarHistorialCommand { get; }
        public ICommand ExportarPdfCommand { get; }
        public ICommand ExportarExcelCommand { get; }

        public EstadoFinancieroViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Nodos = new ObservableCollection<NodoEstadoFinanciero>();

            _fechaInicio = new DateTime(DateTime.Now.Year, 1, 1);
            _fechaCorte = DateTime.Now;
            _tasaCambio = 1m;

            OpcionesMoneda = new ObservableCollection<string>
            {
                "Bolívares (Bs)",
                "Dólares (USD)",
                "Euros (EUR)",
                "Pesos Colombianos (COP)",
            };
            _monedaSeleccionada = "Bolívares (Bs)";

            _contabilidadService.OnEmpresaCambiada += GenerarReporte;

            GenerarReporteCommand = new RelayCommand(GenerarReporte);
            ExportarPdfCommand = new RelayCommand(ExportarPdf, () => TieneDatos);
            ExportarExcelCommand = new RelayCommand(ExportarExcel, () => TieneDatos);
            RestaurarHistorialCommand = new RelayCommand<HistorialReexpresion>(RestaurarHistorial);

            GenerarReporte();
        }

        private void ActualizarTasaSegunMoneda()
        {
            if (MonedaSeleccionada == "Bolívares (Bs)" || MonedaSeleccionada == "Bolívares (Bs) - Oficial")
            {
                TasaCambio = 1m;
            }
            else
            {
                // Reiniciar la tasa cada vez que se cambia de moneda
                TasaCambio = 0m;
                TasaCambioTexto = ""; 
            }
            
            GenerarReporte();
        }

        private void GenerarReporte()
        {
            if (_contabilidadService.EmpresaActivaId == null)
            {
                Nodos = new ObservableCollection<NodoEstadoFinanciero>();
                TituloReporte = "Seleccione una empresa";
                SubtituloReporte = "Debe seleccionar una empresa activa en el panel lateral.";
                TieneDatos = false;
                return;
            }

            var cuentas = _contabilidadService.ObtenerCuentasContables().ToList();
            var historiales = _contabilidadService.ObtenerHistorialReexpresiones();
            var saldos = new System.Collections.Generic.Dictionary<string, decimal>();

            // Calcular saldos base de cuentas sin hijos
            decimal tasa = TasaCambio <= 0 ? 1 : TasaCambio;

            foreach (var c in cuentas.Where(x => x.AceptaMovimiento))
            {
                if (c.Codigo == "3.2.01.00") continue; // Ignorar REI a petición del usuario
                decimal saldoMonto = _contabilidadService.ObtenerSaldoCuentaEntreFechas(c.Codigo, FechaInicio, FechaCorte);
                
                if (MonedaSeleccionada.Contains("COP") || MonedaSeleccionada.Contains("Pesos"))
                {
                    saldos[c.Codigo] = saldoMonto * tasa;
                }
                else
                {
                    saldos[c.Codigo] = saldoMonto / tasa;
                }
            }

            // Subir saldos a los padres
            foreach (var c in cuentas.OrderByDescending(x => x.Nivel).Where(x => !x.AceptaMovimiento))
            {
                saldos[c.Codigo] = cuentas.Where(h => h.CuentaPadre == c.Codigo && h.Codigo != "3.2.01.00").Sum(h => saldos.ContainsKey(h.Codigo) ? saldos[h.Codigo] : 0);
            }

            var rootNodos = new ObservableCollection<NodoEstadoFinanciero>();

            decimal totalActivo = 0, totalPasivo = 0, totalPatrimonio = 0;
            
            string monedaSymbol = "Bs.";
            if (MonedaSeleccionada != null)
            {
                if (MonedaSeleccionada.Contains("USD") || MonedaSeleccionada.Contains("Dólar")) monedaSymbol = "$";
                else if (MonedaSeleccionada.Contains("EUR") || MonedaSeleccionada.Contains("Euro")) monedaSymbol = "€";
                else if (MonedaSeleccionada.Contains("COP") || MonedaSeleccionada.Contains("Peso")) monedaSymbol = "COP";
            }

            void ConstruirArbol(NodoEstadoFinanciero nodoPadre, string tipo, string codigoPadre)
            {
                var hijos = string.IsNullOrEmpty(codigoPadre)
                    ? cuentas.Where(c => c.Tipo == tipo && string.IsNullOrEmpty(c.CuentaPadre)).OrderBy(c => c.Codigo).ToList()
                    : cuentas.Where(c => c.CuentaPadre == codigoPadre).OrderBy(c => c.Codigo).ToList();

                foreach (var hijo in hijos)
                {
                    decimal saldo = saldos.ContainsKey(hijo.Codigo) ? saldos[hijo.Codigo] : 0;
                    bool tieneMonto = saldo != 0 || historiales.Any(h => h.CodigoCuenta == hijo.Codigo && h.IdComprobanteAsociado > 0);
                    if (!tieneMonto && hijo.Nivel > 1) continue;

                    var nodoHijo = new NodoEstadoFinanciero
                    {
                        Codigo = hijo.Codigo,
                        Nombre = hijo.Nombre,
                        Monto = saldo,
                        MontoFormateado = $"{monedaSymbol} {saldo:N2}",
                        EsCuentaPadre = !hijo.AceptaMovimiento
                    };

                    if (hijo.AceptaMovimiento)
                    {
                        var histAsociado = historiales.FirstOrDefault(h => h.CodigoCuenta == hijo.Codigo && h.IdComprobanteAsociado > 0);
                        if (histAsociado != null)
                        {
                            nodoHijo.TieneHistorial = true;
                            nodoHijo.HistorialAsociado = histAsociado;
                        }
                    }

                    ConstruirArbol(nodoHijo, tipo, hijo.Codigo);
                    nodoPadre.SubNodos.Add(nodoHijo);
                }
            }

            void AgregarGrupo(string nombreRaiz, string encabezado, ref decimal totalGrupo)
            {
                var rootGrupo = new NodoEstadoFinanciero { Nombre = encabezado, EsEncabezado = true, EsCuentaPadre = true };
                ConstruirArbol(rootGrupo, nombreRaiz, "");

                totalGrupo = cuentas.Where(c => c.Tipo == nombreRaiz && c.Nivel == 1).Sum(c => saldos.ContainsKey(c.Codigo) ? saldos[c.Codigo] : 0);
                if (totalGrupo == 0)
                {
                    totalGrupo = cuentas.Where(c => c.Tipo == nombreRaiz && string.IsNullOrEmpty(c.CuentaPadre)).Sum(c => saldos.ContainsKey(c.Codigo) ? saldos[c.Codigo] : 0);
                }

                rootGrupo.Monto = totalGrupo;
                rootGrupo.MontoFormateado = $"{monedaSymbol} {totalGrupo:N2}";

                if (rootGrupo.SubNodos.Any())
                {
                    rootGrupo.SubNodos.Add(new NodoEstadoFinanciero { Nombre = $"Total {encabezado}", Monto = totalGrupo, MontoFormateado = $"{monedaSymbol} {totalGrupo:N2}", EsTotal = true });
                    rootNodos.Add(rootGrupo);
                }
            }

            AgregarGrupo("Activo", "ACTIVO", ref totalActivo);
            AgregarGrupo("Pasivo", "PASIVO", ref totalPasivo);
            AgregarGrupo("Patrimonio", "PATRIMONIO", ref totalPatrimonio);

            if (totalPasivo != 0 || totalPatrimonio != 0)
            {
                decimal sum = totalPasivo + totalPatrimonio;
                rootNodos.Add(new NodoEstadoFinanciero
                {
                    Nombre = "TOTAL PASIVO + PATRIMONIO",
                    Monto = sum,
                    MontoFormateado = $"{monedaSymbol} {sum:N2}",
                    EsTotal = true,
                    EsEncabezado = true
                });
            }

            Nodos = rootNodos;
            
            string sufijoMoneda = MonedaSeleccionada == "Bolívares (Bs) - Oficial" ? "" : $" (Expresado en {MonedaSeleccionada})";
            TituloReporte = $"ESTADO FINANCIERO DETALLADO{sufijoMoneda}";
            SubtituloReporte = $"Del {FechaInicio:dd/MM/yyyy} al {FechaCorte:dd/MM/yyyy}";
            TieneDatos = rootNodos.Any();
        }

        private void RestaurarHistorial(HistorialReexpresion historial)
        {
            if (historial == null) return;
        }

        private void ExportarExcel()
        {
            if (!TieneDatos) return;

            var sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook|*.xlsx",
                Title = "Exportar Estado Financiero a Excel",
                FileName = $"EstadoFinanciero_{FechaCorte:yyyyMMdd}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var ws = workbook.Worksheets.Add("Estado Financiero");
                        
                        // Encabezados
                        ws.Cell(1, 1).Value = TituloReporte;
                        ws.Cell(1, 1).Style.Font.Bold = true;
                        ws.Cell(1, 1).Style.Font.FontSize = 14;
                        ws.Range(1, 1, 1, 2).Merge();

                        ws.Cell(2, 1).Value = SubtituloReporte;
                        ws.Cell(2, 1).Style.Font.Italic = true;
                        ws.Range(2, 1, 2, 2).Merge();

                        ws.Cell(4, 1).Value = "Cuenta";
                        
                        string monedaSymbol = "Bs.";
                        if (MonedaSeleccionada != null)
                        {
                            if (MonedaSeleccionada.Contains("USD") || MonedaSeleccionada.Contains("Dólar")) monedaSymbol = "$";
                            else if (MonedaSeleccionada.Contains("EUR") || MonedaSeleccionada.Contains("Euro")) monedaSymbol = "€";
                            else if (MonedaSeleccionada.Contains("COP") || MonedaSeleccionada.Contains("Peso")) monedaSymbol = "COP";
                            else if (MonedaSeleccionada.Contains("Personalizado")) monedaSymbol = "M.";
                        }
                        
                        ws.Cell(4, 2).Value = $"Monto ({monedaSymbol})";
                        
                        var headerRange = ws.Range(4, 1, 4, 2);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                        int currentRow = 5;

                        // Función recursiva para agregar filas
                        void AgregarFilaExcel(NodoEstadoFinanciero nodo, int nivel)
                        {
                            // Indentar el nombre según el nivel
                            string indent = new string(' ', nivel * 4);
                            ws.Cell(currentRow, 1).Value = indent + nodo.Nombre;
                            
                            if (nodo.Monto != 0 || nodo.EsTotal)
                            {
                                ws.Cell(currentRow, 2).Value = nodo.Monto;
                                ws.Cell(currentRow, 2).Style.NumberFormat.Format = "#,##0.00";
                            }

                            if (nodo.EsEncabezado || nodo.EsTotal)
                            {
                                ws.Range(currentRow, 1, currentRow, 2).Style.Font.Bold = true;
                            }
                            
                            if (nodo.TieneHistorial)
                            {
                                ws.Cell(currentRow, 1).Style.Font.FontColor = XLColor.OrangeRed;
                            }

                            currentRow++;

                            foreach (var subNodo in nodo.SubNodos)
                            {
                                AgregarFilaExcel(subNodo, nivel + 1);
                            }
                        }

                        foreach (var nodo in Nodos)
                        {
                            AgregarFilaExcel(nodo, 0);
                        }

                        ws.Columns().AdjustToContents();
                        workbook.SaveAs(sfd.FileName);
                        MessageBox.Show("El archivo Excel se exportó exitosamente.", "Exportación Completa", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar a Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportarPdf()
        {
            if (!TieneDatos) return;

            QuestPDF.Settings.License = LicenseType.Community;

            var sfd = new SaveFileDialog
            {
                Filter = "Documento PDF|*.pdf",
                Title = "Exportar Estado Financiero a PDF",
                FileName = $"EstadoFinanciero_{FechaCorte:yyyyMMdd}.pdf"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    Document.Create(container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.Letter);
                            page.Margin(2, Unit.Centimetre);
                            page.PageColor(Colors.White);
                            page.DefaultTextStyle(x => x.FontSize(10));

                            page.Header().Element(ComposeHeader);
                            page.Content().Element(ComposeContent);
                            page.Footer().Element(ComposeFooter);
                        });
                    }).GeneratePdf(sfd.FileName);

                    MessageBox.Show("El archivo PDF se exportó exitosamente.", "Exportación Completa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar a PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ComposeHeader(IContainer container)
        {
            var empresa = _contabilidadService.ObtenerEmpresaActiva()?.NombreEmpresa ?? "Empresa No Definida";

            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("ESTADO FINANCIERO").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                    column.Item().Text(empresa).FontSize(14).FontColor(Colors.Grey.Darken3);
                    column.Item().Text($"Del {FechaInicio:dd/MM/yyyy} al {FechaCorte:dd/MM/yyyy}").FontSize(12).FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(150).AlignRight().Column(column =>
                {
                    column.Item().Text("Sistema Contable").FontSize(10).FontColor(Colors.Grey.Medium);
                    column.Item().Text("Zulay Angola").FontSize(12).SemiBold().FontColor(Colors.Blue.Darken2);
                });
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(1, Unit.Centimetre).Column(column =>
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.ConstantColumn(120);
                    });

                    table.Header(header =>
                    {
                        var cellStyle = header.Cell().Background(Colors.Blue.Darken2).PaddingVertical(5).PaddingHorizontal(5);
                        cellStyle.Text("Cuenta").SemiBold().FontColor(Colors.White);
                        
                        string monedaSymbol = "Bs.";
                        if (MonedaSeleccionada != null)
                        {
                            if (MonedaSeleccionada.Contains("USD") || MonedaSeleccionada.Contains("Dólar")) monedaSymbol = "$";
                            else if (MonedaSeleccionada.Contains("EUR") || MonedaSeleccionada.Contains("Euro")) monedaSymbol = "€";
                            else if (MonedaSeleccionada.Contains("COP") || MonedaSeleccionada.Contains("Peso")) monedaSymbol = "COP";
                        }

                        var numStyle = header.Cell().Background(Colors.Blue.Darken2).PaddingVertical(5).PaddingHorizontal(5).AlignRight();
                        numStyle.Text($"Monto ({monedaSymbol})").SemiBold().FontColor(Colors.White);
                    });

                    int rowIndex = 0;

                    void AddNodeToTable(NodoEstadoFinanciero nodo, int nivel)
                    {
                        var cellPadding = 4f;
                        var indent = nivel * 15f;
                        var isGroupHeader = nodo.EsEncabezado;
                        
                        string bgColor = isGroupHeader ? Colors.Grey.Lighten3 : (rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4);

                        // Draw Account Name
                        var cellName = table.Cell().Background(bgColor).PaddingVertical(cellPadding).PaddingLeft(indent).PaddingRight(4).Text(nodo.Nombre).FontSize(10);
                        if (nodo.EsEncabezado || nodo.EsTotal) cellName.SemiBold();
                        if (isGroupHeader) cellName.FontColor(Colors.Blue.Darken3);
                        if (nodo.TieneHistorial) cellName.FontColor(Colors.Orange.Darken2);

                        // Draw Monto
                        if (nodo.Monto != 0 || nodo.EsTotal)
                        {
                            var cellMonto = table.Cell().Background(bgColor).PaddingVertical(cellPadding).PaddingHorizontal(4).AlignRight().Text(nodo.MontoFormateado).FontSize(10);
                            if (nodo.EsEncabezado || nodo.EsTotal) cellMonto.SemiBold();
                            if (isGroupHeader) cellMonto.FontColor(Colors.Blue.Darken3);
                        }
                        else
                        {
                            table.Cell().Background(bgColor).Text("");
                        }
                        
                        rowIndex++;

                        foreach (var subNodo in nodo.SubNodos)
                        {
                            AddNodeToTable(subNodo, nivel + 1);
                        }
                    }

                    foreach (var nodo in Nodos)
                    {
                        AddNodeToTable(nodo, 0);
                        
                        // Agregar línea separadora después de cada grupo principal
                        table.Cell().ColumnSpan(2).PaddingVertical(8).BorderBottom(1).BorderColor(Colors.Grey.Medium).Text("");
                    }
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Text(x =>
            {
                x.Span("Página ");
                x.CurrentPageNumber();
                x.Span(" de ");
                x.TotalPages();
            });
        }
    }
}
