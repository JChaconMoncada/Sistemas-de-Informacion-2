using System.Collections.ObjectModel;
using SistemaContableZulay.UI.Services;
using System.Windows.Input;
using SistemaContableZulay.UI.Domain;
using System.Linq;
using System.Collections.Generic;


namespace Sistema_contable.ViewModels
{
    public class PlanCuentasViewModel : ViewModelBase
    {
        private CuentaContable _cuentaSeleccionada;
        private readonly ContabilidadService _contabilidadService;

        public ObservableCollection<CuentaContable> Cuentas { get; set; }
        public List<string> TiposDeCuenta { get; } = new List<string> { "Activo", "Pasivo", "Patrimonio", "Ingreso", "Egreso", "Costo" };

        public CuentaContable CuentaSeleccionada
        {
            get => _cuentaSeleccionada;
            set
            {
                if (_cuentaSeleccionada != null)
                {
                    _cuentaSeleccionada.PropertyChanged -= CuentaSeleccionada_PropertyChanged;
                }
                SetProperty(ref _cuentaSeleccionada, value);
                if (_cuentaSeleccionada != null)
                {
                    _cuentaSeleccionada.PropertyChanged += CuentaSeleccionada_PropertyChanged;
                }
                OnPropertyChanged(nameof(CuentasPadreDisponibles));
            }
        }

        private void CuentaSeleccionada_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CuentaContable.Tipo))
            {
                OnPropertyChanged(nameof(CuentasPadreDisponibles));
            }
        }

        public ICommand NuevaCuentaCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand EliminarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand ExpandirTodoCommand { get; }
        public ICommand ContraerTodoCommand { get; }
        public ICommand ExportarCommand { get; }
        public ICommand AplicarFiltroCommand { get; }
        private string _tipoSeleccionado = "Todos los tipos";
        public string TipoSeleccionado
        {
            get => _tipoSeleccionado;
            set { _tipoSeleccionado = value; OnPropertyChanged(nameof(TipoSeleccionado)); }
        }

        private string _nivelSeleccionado = "Todos niveles";
        public string NivelSeleccionado
        {
            get => _nivelSeleccionado;
            set { _nivelSeleccionado = value; OnPropertyChanged(nameof(NivelSeleccionado)); }
        }

        private string _textoBusqueda;
        public string TextoBusqueda
        {
            get => _textoBusqueda;
            set
            {
                _textoBusqueda = value;
                OnPropertyChanged(nameof(TextoBusqueda));

                FiltrarPlanCuentas();
            }
        }
        public ObservableCollection<CuentaContable> TodasLasCuentasPlanas { get; set; } = new ObservableCollection<CuentaContable>();

        public IEnumerable<CuentaContable> CuentasPadreDisponibles
        {
            get
            {
                var query = TodasLasCuentasPlanas.Where(c => !c.AceptaMovimiento);
                if (CuentaSeleccionada != null && !string.IsNullOrEmpty(CuentaSeleccionada.Tipo))
                {
                    query = query.Where(c => c.Tipo == CuentaSeleccionada.Tipo);
                }
                return query.ToList();
            }
        }

        public void ActualizarListaPlana()
        {
            TodasLasCuentasPlanas.Clear();
            // Empezamos a recorrer desde la raíz del árbol ('Cuentas' es tu lista jerárquica)
            if (Cuentas != null)
            {
                AplanarCuentas(Cuentas);
            }
            OnPropertyChanged(nameof(CuentasPadreDisponibles));
        }

        private void AplanarCuentas(System.Collections.Generic.IEnumerable<CuentaContable> listaJerarquica)
        {
            foreach (var cuenta in listaJerarquica)
            {
                // Añadimos la cuenta actual a la lista del ComboBox
                TodasLasCuentasPlanas.Add(cuenta);

                // Si esta cuenta tiene hijos, bajamos un nivel a revisarlos también
                if (cuenta.Hijos != null && cuenta.Hijos.Count > 0)
                {
                    AplanarCuentas(cuenta.Hijos);
                }
            }
        }
        public PlanCuentasViewModel()
        {
            _contabilidadService = ContabilidadService.Instance;
            Cuentas = new ObservableCollection<CuentaContable>();

            NuevaCuentaCommand = new RelayCommand(() => NuevaCuenta());
            GuardarCommand = new RelayCommand(() => GuardarCuenta(), () => CuentaSeleccionada != null);
            EliminarCommand = new RelayCommand(() => EliminarCuenta(), () => CuentaSeleccionada != null);
            CancelarCommand = new RelayCommand(() => { CuentaSeleccionada = null; CargarCuentas(); });
            AplicarFiltroCommand = new RelayCommand(FiltrarPlanCuentas);
            // Pon esto dentro del constructor de tu ViewModel
            ExpandirTodoCommand = new RelayCommand(ExpandirTodo);
            ContraerTodoCommand = new RelayCommand(ContraerTodo);
            ExportarCommand = new RelayCommand(ExportarPlanCuentas);

            ActualizarListaPlana();
            CargarCuentas();
        }
        private void CargarCuentas()
        {
            // 1. Traemos todas las cuentas de la base de datos
            var cuentasServicio = _contabilidadService.ObtenerCuentasContables();

            if (cuentasServicio == null || cuentasServicio.Count == 0)
            {
                SembrarCuentasPorDefecto();
                cuentasServicio = _contabilidadService.ObtenerCuentasContables();
            }

            System.Console.WriteLine($"=== DIAGNÓSTICO: El servicio devolvió {cuentasServicio?.Count ?? 0} cuentas ===");

            // 2. Limpiamos la pantalla
            Cuentas.Clear();

            // [PASO CLAVE NUEVO]: Limpiamos los hijos de TODAS las cuentas antes de empezar a enlazar
            foreach (var cuenta in cuentasServicio)
            {
                cuenta.Hijos.Clear();
            }

            // 3. Diccionario para buscar cuentas rápido por su Id
            var diccionarioCuentas = cuentasServicio.ToDictionary(c => c.Codigo);

            // 4. Lista temporal para las cuentas principales (Nivel 1, las que no tienen papá)
            var cuentasPrincipales = new List<CuentaContable>();

            foreach (var cuenta in cuentasServicio)
            {
                // Si el código del padre es igual al de la propia cuenta, o está vacío, es raíz
                if (string.IsNullOrEmpty(cuenta.CuentaPadre) || cuenta.CuentaPadre == cuenta.Codigo)
                {
                    cuentasPrincipales.Add(cuenta);
                }
                else
                {
                    // Buscamos si su padre existe en el sistema
                    if (diccionarioCuentas.TryGetValue(cuenta.CuentaPadre, out var cuentaPadre))
                    {
                        cuentaPadre.Hijos.Add(cuenta);
                    }
                    else
                    {
                        // PLAN DE EMERGENCIA: Si tiene un "CuentaPadre" pero ese padre NO existe en la BD,
                        // significa que esta cuenta debe actuar como una raíz para que no se pierda.
                        cuentasPrincipales.Add(cuenta);
                    }
                }
            }

            // 5. Subimos al TreeView de la pantalla solo las cuentas raíz
            foreach (var cuentaRaiz in cuentasPrincipales)
            {
                Cuentas.Add(cuentaRaiz);
            }

            // 6. Llenamos la lista plana para el ComboBox de la interfaz
            TodasLasCuentasPlanas.Clear();
            foreach (var cuenta in cuentasServicio)
            {
                TodasLasCuentasPlanas.Add(cuenta);
            }
            ActualizarListaPlana();
        }

        private void NuevaCuenta()
        {
            // 1. Guardamos una referencia de la cuenta que estaba seleccionada en el árbol
            var cuentaPadreActual = CuentaSeleccionada;

            // 2. Instanciamos la nueva cuenta con valores base
            var nueva = new CuentaContable
            {
                Nombre = "Nueva Cuenta",
                Tipo = "Activo", // Un valor por defecto seguro para el ComboBox
                AceptaMovimiento = true,
                Activo = true
            };

            // 3. ¡Aquí está la magia! Evaluamos si va a ser una cuenta raíz o una subcuenta
            if (cuentaPadreActual != null && !string.IsNullOrWhiteSpace(cuentaPadreActual.Codigo))
            {
                // El usuario tenía seleccionada una cuenta, así que creamos una SUBCUENTA
                nueva.CuentaPadre = cuentaPadreActual.Codigo;
                nueva.Tipo = cuentaPadreActual.Tipo; // Hereda el mismo tipo (ej: si el padre es Activo, el hijo es Activo)
                nueva.Nivel = cuentaPadreActual.Nivel + 1; // Sube un nivel en la jerarquía
                
                nueva.Codigo = GenerarSiguienteCodigo(cuentaPadreActual);
            }
            else
            {
                // No había nada seleccionado, se crea una cuenta RAÍZ (Nivel 1)
                nueva.CuentaPadre = null;
                nueva.Nivel = 1;
                nueva.Codigo = GenerarSiguienteCodigo(null);
            }

            // 4. Pasamos la nueva cuenta al formulario para que el usuario termine de rellenar
            CuentaSeleccionada = nueva;
        }

        private string GenerarSiguienteCodigo(CuentaContable padre)
        {
            if (padre == null)
            {
                // Generar código para una nueva raíz
                int maxRoot = 0;
                foreach (var c in Cuentas)
                {
                    if (!string.IsNullOrEmpty(c.Codigo) && char.IsDigit(c.Codigo[0]))
                    {
                        int val = c.Codigo[0] - '0';
                        if (val > maxRoot) maxRoot = val;
                    }
                }
                return $"{maxRoot + 1}.0.00.00";
            }

            // Generar código para hijo
            if (padre.Hijos == null || padre.Hijos.Count == 0)
            {
                // No tiene hijos aún
                var parts = padre.Codigo.Split('.');
                if (parts.Length != 4) return padre.Codigo + ".01";
                
                if (padre.Nivel == 1) return $"{parts[0]}.1.00.00";
                if (padre.Nivel == 2) return $"{parts[0]}.{parts[1]}.01.00";
                if (padre.Nivel == 3) return $"{parts[0]}.{parts[1]}.{parts[2]}.01";
                return $"{padre.Codigo}.01";
            }
            else
            {
                // Buscar el hijo con el código mayor
                var maxHijo = padre.Hijos.OrderByDescending(h => h.Codigo).First();
                var parts = maxHijo.Codigo.Split('.');
                if (parts.Length == 4)
                {
                    if (padre.Nivel == 1)
                    {
                        int val = int.Parse(parts[1]);
                        return $"{parts[0]}.{val + 1}.00.00";
                    }
                    if (padre.Nivel == 2)
                    {
                        int val = int.Parse(parts[2]);
                        return $"{parts[0]}.{parts[1]}.{(val + 1):D2}.00";
                    }
                    if (padre.Nivel >= 3)
                    {
                        int val = int.Parse(parts[3]);
                        return $"{parts[0]}.{parts[1]}.{parts[2]}.{(val + 1):D2}";
                    }
                }
                return maxHijo.Codigo + "+1";
            }
        }

        private void GuardarCuenta()
        {
            if (CuentaSeleccionada == null) return;

            // 1. Validaciones de seguridad
            if (string.IsNullOrWhiteSpace(CuentaSeleccionada.Codigo) || string.IsNullOrWhiteSpace(CuentaSeleccionada.Nombre))
            {
                System.Windows.MessageBox.Show("El código y el nombre son campos obligatorios.", "Validación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // 2. Limpieza de espacios en blanco
            if (!string.IsNullOrEmpty(CuentaSeleccionada.Codigo)) CuentaSeleccionada.Codigo = CuentaSeleccionada.Codigo.Trim();
            if (!string.IsNullOrEmpty(CuentaSeleccionada.CuentaPadre)) CuentaSeleccionada.CuentaPadre = CuentaSeleccionada.CuentaPadre.Trim();

            // 3. Si es nivel 1, aseguramos que no tenga padre registrado
            if (CuentaSeleccionada.Nivel == 1 || string.IsNullOrWhiteSpace(CuentaSeleccionada.CuentaPadre))
            {
                CuentaSeleccionada.CuentaPadre = null;
            }

            try
            {
                // 4. Mandamos a guardar los datos en la persistencia real
                _contabilidadService.GuardarCuenta(CuentaSeleccionada);

                System.Windows.MessageBox.Show($"La cuenta '{CuentaSeleccionada.Nombre}' se guardó correctamente.", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                // 5. ¡AQUÍ ESTÁ LA CLAVE! 
                // Llamamos a cargar cuentas para que el sistema lea la base de datos armada 
                // y pinte el árbol y el combo perfectamente actualizados.
                CargarCuentas();

                // 6. Limpiamos la selección del formulario
                CuentaSeleccionada = null;
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al guardar en la base de datos: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void EliminarCuenta()
        {
            if (CuentaSeleccionada == null || string.IsNullOrEmpty(CuentaSeleccionada.Codigo)) return;

            // 1. BLINDAJE: Si la cuenta tiene hijos en la colección, se prohíbe la eliminación
            if (CuentaSeleccionada.Hijos != null && CuentaSeleccionada.Hijos.Count > 0)
            {
                System.Windows.MessageBox.Show(
                    $"No se puede eliminar la cuenta '{CuentaSeleccionada.Codigo} - {CuentaSeleccionada.Nombre}' porque contiene subcuentas dependientes.\n\nPara eliminarla, primero debes reubicar o eliminar sus cuentas hijas.",
                    "Operación No Permitida",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Stop);
                return;
            }

            // 2. Si no tiene hijos, ahí sí le pedimos confirmación para borrar
            var resultado = System.Windows.MessageBox.Show(
                $"¿Estás seguro de que deseas eliminar la cuenta '{CuentaSeleccionada.Codigo} - {CuentaSeleccionada.Nombre}'?\nEsta acción no se puede deshacer.",
                "Confirmar Eliminación",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (resultado == System.Windows.MessageBoxResult.No) return;

            try
            {
                // 3. Eliminamos de la Base de Datos ya que es una cuenta "hoja" (sin hijos)
                _contabilidadService.EliminarCuenta(CuentaSeleccionada.Codigo);

                System.Windows.MessageBox.Show("Cuenta eliminada con éxito.", "Sistema Contable", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                // 4. recargamos al data
                CargarCuentas();
                CuentaSeleccionada = null;
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al eliminar la cuenta: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void FiltrarPlanCuentas()
        {
            if (Cuentas == null) return;

            string criterioTexto = string.IsNullOrWhiteSpace(TextoBusqueda) ? "" : TextoBusqueda.Trim().ToLower();
            string criterioTipo = TipoSeleccionado;
            string criterioNivel = NivelSeleccionado;

            foreach (var cuenta in Cuentas)
            {
                // Iniciamos el parámetro padreCoincidePorTipo en false para la raíz
                EvaluarFiltro(cuenta, criterioTexto, criterioTipo, criterioNivel, false);
            }
        }

        private bool EvaluarFiltro(CuentaContable cuenta, string texto, string tipo, string nivel, bool padreCoincidePorTipo = false)
        {
            // 1. Limpieza de datos iniciales
            tipo = tipo?.ToLower().Trim() ?? "";
            nivel = nivel?.ToLower().Trim() ?? "";

            bool filtroTipoVacio = string.IsNullOrEmpty(tipo) || tipo.Contains("todo");
            bool filtroNivelVacio = string.IsNullOrEmpty(nivel) || nivel.Contains("todo");

            // Si no hay filtros activos, todo visible por defecto
            if (string.IsNullOrEmpty(texto) && filtroTipoVacio && filtroNivelVacio)
            {
                cuenta.IsVisible = true;
                cuenta.IsExpanded = false;
                if (cuenta.Hijos != null)
                {
                    foreach (var hijo in cuenta.Hijos)
                        EvaluarFiltro(hijo, texto, tipo, nivel, false);
                }
                return true;
            }

            // 2. Evaluar el filtro de Texto
            bool coincideTexto = string.IsNullOrEmpty(texto) ||
                                 (!string.IsNullOrEmpty(cuenta.Codigo) && cuenta.Codigo.ToLower().Contains(texto)) ||
                                 (!string.IsNullOrEmpty(cuenta.Nombre) && cuenta.Nombre.ToLower().Contains(texto));

            // 3. Evaluar el filtro de Tipo (Súper tolerante)
            // Si el padre ya coincidió por tipo, el hijo se arrastra como verdadero automáticamente
            bool coincideTipo = filtroTipoVacio || padreCoincidePorTipo ||
                                (!string.IsNullOrEmpty(cuenta.Tipo) &&
                                 (cuenta.Tipo.ToLower().Contains(tipo) || tipo.Contains(cuenta.Tipo.ToLower())));

            // 4. Evaluar el filtro de Nivel
            bool coincideNivel = filtroNivelVacio;
            if (!coincideNivel)
            {
                string numeroNivel = System.Text.RegularExpressions.Regex.Match(nivel, @"\d+").Value;
                if (int.TryParse(numeroNivel, out int nivelInt))
                {
                    coincideNivel = (cuenta.Nivel == nivelInt);
                }
            }

            // La cuenta coincide si cumple los criterios actuales
            bool coincideActual = coincideTexto && coincideTipo && coincideNivel;
            bool algunHijoCoincide = false;

            // Pasamos el estado de 'coincideTipo' a los hijos para que no se oculten si el padre es del tipo correcto
            if (cuenta.Hijos != null)
            {
                foreach (var hijo in cuenta.Hijos)
                {
                    if (EvaluarFiltro(hijo, texto, tipo, nivel, coincideTipo))
                    {
                        algunHijoCoincide = true;
                    }
                }
            }

            // Es visible si ella coincide o si uno de sus hijos profundos coincide
            bool debeSerVisible = coincideActual || algunHijoCoincide;
            cuenta.IsVisible = debeSerVisible;

            // Expandimos las carpetas solo si la coincidencia viene del fondo
            cuenta.IsExpanded = algunHijoCoincide && debeSerVisible;

            return debeSerVisible;

        }

        public void ExpandirTodo()
        {
            if (Cuentas == null) return;
            foreach (var cuenta in Cuentas) CambiarEstadoExpansion(cuenta, true);
        }

        // ==========================================
        // 2. BOTÓN: CONTRAER TODO
        // ==========================================
        public void ContraerTodo()
        {
            if (Cuentas == null) return;
            foreach (var cuenta in Cuentas) CambiarEstadoExpansion(cuenta, false);
        }

        private void CambiarEstadoExpansion(CuentaContable cuenta, bool expandir)
        {
            cuenta.IsExpanded = expandir;
            if (cuenta.Hijos != null)
            {
                foreach (var hijo in cuenta.Hijos) CambiarEstadoExpansion(hijo, expandir);
            }
        }

        // ==========================================
        // 3. BOTÓN: EXPORTAR A EXCEL (CSV Nativo)
        // ==========================================
        public void ExportarPlanCuentas()
        {
            if (Cuentas == null || !Cuentas.Any())
            {
                System.Windows.MessageBox.Show("No hay cuentas disponibles para exportar.", "Atención", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Abre el cuadro de diálogo nativo de Windows para guardar archivos
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Archivos de Excel (*.csv)|*.csv",
                FileName = "Plan_De_Cuentas_Contable_" + System.DateTime.Now.ToString("yyyyMMdd")
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var csvContent = new System.Text.StringBuilder();

                // Creamos los títulos de las columnas separados por punto y coma (Estándar de Excel)
                csvContent.AppendLine("Código;Nombre de la Cuenta;Tipo de Cuenta;Nivel");

                // Convertimos el árbol jerárquico en una lista plana secuencial
                var listaPlana = new List<CuentaContable>();
                foreach (var cuenta in Cuentas)
                {
                    AplanarCuentas(cuenta, listaPlana);
                }

                // Escribimos cada cuenta en el archivo entre comillas para evitar problemas con los espacios
                foreach (var c in listaPlana)
                {
                    csvContent.AppendLine($"\"{c.Codigo}\";\"{c.Nombre}\";\"{c.Tipo}\";{c.Nivel}");
                }

                // Guardamos el archivo con codificación UTF-8 para que los acentos y la 'Ñ' no salgan rotos en Excel
                System.IO.File.WriteAllText(saveFileDialog.FileName, csvContent.ToString(), System.Text.Encoding.UTF8);

                System.Windows.MessageBox.Show("Catálogo de cuentas exportado exitosamente", "Éxito al Exportar", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        // Función auxiliar recursiva para desenredar el árbol de cuentas
        private void AplanarCuentas(CuentaContable cuenta, List<CuentaContable> lista)
        {
            lista.Add(cuenta);
            if (cuenta.Hijos != null)
            {
                foreach (var hijo in cuenta.Hijos)
                {
                    AplanarCuentas(hijo, lista);
                }
            }
        }

        private void SembrarCuentasPorDefecto()
        {
            var defaultCuentas = new List<CuentaContable>
            {
                new CuentaContable { Codigo = "1.0.00.00", Nombre = "Activo", Tipo = "Activo", Nivel = 1, AceptaMovimiento = false },
                new CuentaContable { Codigo = "1.1.00.00", Nombre = "Activo Corriente", Tipo = "Activo", Nivel = 2, CuentaPadre = "1.0.00.00", AceptaMovimiento = false },
                new CuentaContable { Codigo = "1.1.01.00", Nombre = "Efectivo y Equivalentes", Tipo = "Activo", Nivel = 3, CuentaPadre = "1.1.00.00", AceptaMovimiento = false },
                new CuentaContable { Codigo = "1.1.01.01", Nombre = "Caja General", Tipo = "Activo", Nivel = 4, CuentaPadre = "1.1.01.00", AceptaMovimiento = true },
                new CuentaContable { Codigo = "1.1.01.02", Nombre = "Banco Mercantil 0105", Tipo = "Activo", Nivel = 4, CuentaPadre = "1.1.01.00", AceptaMovimiento = true },
                new CuentaContable { Codigo = "1.1.01.03", Nombre = "Banco Banesco 0134", Tipo = "Activo", Nivel = 4, CuentaPadre = "1.1.01.00", AceptaMovimiento = true },
                new CuentaContable { Codigo = "1.1.02.00", Nombre = "Cuentas por Cobrar", Tipo = "Activo", Nivel = 3, CuentaPadre = "1.1.00.00", AceptaMovimiento = false },
                new CuentaContable { Codigo = "1.1.02.01", Nombre = "Clientes Nacionales", Tipo = "Activo", Nivel = 4, CuentaPadre = "1.1.02.00", AceptaMovimiento = true },
                
                new CuentaContable { Codigo = "2.0.00.00", Nombre = "Pasivo", Tipo = "Pasivo", Nivel = 1, AceptaMovimiento = false },
                new CuentaContable { Codigo = "2.1.00.00", Nombre = "Pasivo Corriente", Tipo = "Pasivo", Nivel = 2, CuentaPadre = "2.0.00.00", AceptaMovimiento = false },
                new CuentaContable { Codigo = "2.1.01.00", Nombre = "Cuentas por Pagar Comerciales", Tipo = "Pasivo", Nivel = 3, CuentaPadre = "2.1.00.00", AceptaMovimiento = false },
                new CuentaContable { Codigo = "2.1.01.01", Nombre = "Proveedores Locales", Tipo = "Pasivo", Nivel = 4, CuentaPadre = "2.1.01.00", AceptaMovimiento = true },
                
                new CuentaContable { Codigo = "3.0.00.00", Nombre = "Patrimonio", Tipo = "Patrimonio", Nivel = 1, AceptaMovimiento = false },
                new CuentaContable { Codigo = "3.1.00.00", Nombre = "Capital Social", Tipo = "Patrimonio", Nivel = 2, CuentaPadre = "3.0.00.00", AceptaMovimiento = true },
                new CuentaContable { Codigo = "3.2.00.00", Nombre = "Resultados Acumulados", Tipo = "Patrimonio", Nivel = 2, CuentaPadre = "3.0.00.00", AceptaMovimiento = false },
                new CuentaContable { Codigo = "3.2.01.00", Nombre = "Resultado por Exposición a la Inflación (REI)", Tipo = "Patrimonio", Nivel = 3, CuentaPadre = "3.2.00.00", AceptaMovimiento = true },
                
                new CuentaContable { Codigo = "4.0.00.00", Nombre = "Ingresos", Tipo = "Ingreso", Nivel = 1, AceptaMovimiento = false },
                new CuentaContable { Codigo = "4.1.00.00", Nombre = "Ingresos Operacionales", Tipo = "Ingreso", Nivel = 2, CuentaPadre = "4.0.00.00", AceptaMovimiento = false },
                new CuentaContable { Codigo = "4.1.01.00", Nombre = "Ventas", Tipo = "Ingreso", Nivel = 3, CuentaPadre = "4.1.00.00", AceptaMovimiento = false },
                new CuentaContable { Codigo = "4.1.01.01", Nombre = "Ventas de Bienes / Servicios", Tipo = "Ingreso", Nivel = 4, CuentaPadre = "4.1.01.00", AceptaMovimiento = true },
                
                new CuentaContable { Codigo = "5.0.00.00", Nombre = "Egresos y Gastos", Tipo = "Egreso", Nivel = 1, AceptaMovimiento = false },
                new CuentaContable { Codigo = "5.1.00.00", Nombre = "Gastos de Operación", Tipo = "Egreso", Nivel = 2, CuentaPadre = "5.0.00.00", AceptaMovimiento = false },
                new CuentaContable { Codigo = "5.1.01.00", Nombre = "Gastos Administrativos", Tipo = "Egreso", Nivel = 3, CuentaPadre = "5.1.00.00", AceptaMovimiento = false },
                new CuentaContable { Codigo = "5.1.01.01", Nombre = "Gastos de Alquiler", Tipo = "Egreso", Nivel = 4, CuentaPadre = "5.1.01.00", AceptaMovimiento = true },
                new CuentaContable { Codigo = "5.1.01.02", Nombre = "Gastos de Servicios Públicos", Tipo = "Egreso", Nivel = 4, CuentaPadre = "5.1.01.00", AceptaMovimiento = true },
                new CuentaContable { Codigo = "5.1.01.03", Nombre = "Comisiones Bancarias", Tipo = "Egreso", Nivel = 4, CuentaPadre = "5.1.01.00", AceptaMovimiento = true },
                new CuentaContable { Codigo = "5.1.01.04", Nombre = "Gastos de Honorarios Profesionales", Tipo = "Egreso", Nivel = 4, CuentaPadre = "5.1.01.00", AceptaMovimiento = true }
            };

            foreach (var c in defaultCuentas)
            {
                _contabilidadService.GuardarCuenta(c);
            }
        }
    }
}
