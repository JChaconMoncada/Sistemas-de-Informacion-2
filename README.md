# Sistema Contable - Oficina Contable Zulay Angola

Sistema contable profesional desarrollado en WPF/C# con patrón MVVM.

## Estructura del Proyecto

```
Sistema contable/
├── Views/                      # Vistas XAML (UI)
│   ├── MainWindow.xaml         # Ventana principal con navegación
│   ├── Dashboard.xaml          # Dashboard con tarjetas y alertas
│   ├── Empresas.xaml           # Gestión de empresas
│   ├── Comprobantes.xaml       # Asientos contables
│   ├── PlanCuentas.xaml        # Catálogo de cuentas
│   ├── LibroDiario.xaml        # Libro Diario
│   ├── LibroMayor.xaml         # Libro Mayor
│   ├── Informes.xaml           # Estados Financieros
│   ├── Reexpresion.xaml        # Ajuste por inflación
│   ├── Bancos.xaml             # Importación PDF bancario
│   ├── Cobranza.xaml           # Gestión de cobranza
│   ├── Documentos.xaml         # Control documental
│   └── Configuracion.xaml      # Backups y configuración
│
├── ViewModels/                 # ViewModels (MVVM)
│   ├── ViewModelBase.cs        # Clase base con INotifyPropertyChanged
│   ├── MainWindowViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── EmpresasViewModel.cs
│   ├── ComprobantesViewModel.cs
│   └── CobranzaViewModel.cs
│
├── Models/                     # Modelos de datos
│   ├── Empresa.cs
│   ├── CuentaContable.cs
│   ├── Asiento.cs
│   ├── FacturaInterna.cs
│   └── Documento.cs
│
├── Converters/                 # Value Converters para XAML
│   ├── BoolToVisibilityConverter.cs
│   ├── DecimalToCurrencyConverter.cs
│   └── NullToVisibilityConverter.cs
│
├── Themes/                     # Estilos y recursos
│   └── Styles.xaml             # Estilos globales (Excel-like)
│
├── App.xaml                    # Configuración de la aplicación
└── App.xaml.cs
```

## Características Implementadas (UI)

### Ventana Principal
- **Menú**: Archivo, Empresas, Contabilidad, Reportes, Herramientas, Ayuda
- **Toolbar**: Acceso rápido a módulos principales
- **Dock Empresas**: TreeView con lista de empresas
- **TabControl Central**: Navegación entre módulos
- **StatusBar**: Empresa activa y estado de backup

### Módulos

#### 1. Dashboard
- Tarjetas resumen (Saldo, Ingresos, Gastos, Alertas)
- Gráficos de ingresos vs gastos
- Lista de alertas recientes
- Acciones rápidas

#### 2. Empresas
- DataGrid con lista de empresas
- Formulario CRUD
- Filtros de búsqueda
- Botones de acción

#### 3. Comprobantes (Asientos Contables)
- Encabezado (Fecha, Número, Tipo, Descripción)
- DataGrid de detalle (Código, Cuenta, Debe, Haber)
- Totales y validación de cuadre
- Botones para agregar/eliminar líneas

#### 4. Plan de Cuentas
- TreeView con jerarquía de cuentas
- Panel de detalle con formulario
- Filtros por tipo y nivel
- Edición inline

#### 5. Libros (Diario y Mayor)
- Filtros por fecha y periodo
- DataGrid con transacciones
- Totales calculados
- Botones de exportación

#### 6. Informes
- Selector de tipo de reporte
- Parámetros (fechas, ejercicio, formato)
- Vista previa del reporte
- Tab de gráficos

#### 7. Cobranza
- Tarjetas resumen (Pendientes, Vencidas, Cobradas)
- DataGrid de facturas
- Filtros por estado y fecha
- Acciones (Marcar pagada, Enviar recordatorio)

#### 8. Documentos
- Lista de documentos
- Panel de detalle
- Filtros por estado y tipo
- Gestión de archivos adjuntos

#### 9. Reexpresión Monetaria
- Parámetros (Fechas, IPC)
- Cálculo de factor
- DataGrid de partidas a reexpresar
- Vista previa de ajustes

#### 10. Bancos
- Selector de archivo PDF
- Barra de progreso
- DataGrid con transacciones extraídas
- Mapeo a cuentas contables (ComboBox por fila)

#### 11. Configuración
- Gestión de backups
- Historial de backups
- Configuración fiscal (IVA, ISLR)
- Configuración general

## Estilo Visual

### Paleta de Colores
- **Acento Principal**: Verde Excel (#217346)
- **Fondo**: Blanco (#FFFFFF) y Gris claro (#F3F3F3)
- **Bordes**: Gris (#D1D1D1)
- **Texto**: Primario (#333333), Secundario (#666666)
- **Estados**: Success (#4CAF50), Warning (#FF9800), Danger (#F44336)

### Componentes Reutilizables
- **SummaryCard**: Tarjetas con sombra para resúmenes
- **StatusBadge**: Insignias de estado (Success, Warning, Danger)
- **DataGrid**: Estilo Excel-like con líneas de cuadrícula
- **Botones**: Estilo base y AccentButton (verde)
- **Formularios**: TextBox, ComboBox, DatePicker consistentes

## Patrón MVVM

### ViewModelBase
Clase base que implementa `INotifyPropertyChanged` con métodos helper:
- `OnPropertyChanged()`
- `SetProperty<T>()`

### Binding
Las vistas están preparadas para binding con ViewModels (actualmente sin implementar la lógica).

## Próximos Pasos (Implementación Lógica)

1. **Capa de Datos**
   - Implementar repositorios con Entity Framework o ADO.NET
   - Configurar SQLite como base de datos

2. **Lógica de Negocio**
   - Implementar servicios para cada módulo
   - Validaciones de negocio (partida doble, etc.)

3. **Commands**
   - Implementar RelayCommand/DelegateCommand
   - Conectar botones con acciones

4. **Navegación**
   - Implementar sistema de navegación entre vistas
   - Cargar Pages en los Frames del MainWindow

5. **Funcionalidades Avanzadas**
   - Importación de PDF (pdfplumber o similar)
   - Generación de reportes (PDF/Excel)
   - Gráficos (LiveCharts o similar)
   - Cálculos de reexpresión monetaria

## Requisitos

- .NET Framework 4.7.2+
- Visual Studio 2019 o superior
- Windows 10/11

## Compilación

```bash
# Abrir en Visual Studio
Sistema contable.sln

# O compilar desde línea de comandos
msbuild "Sistema contable.csproj" /p:Configuration=Release
```

## Notas

- Este proyecto contiene **solo la UI/XAML** según lo solicitado
- Los ViewModels son esqueletos sin lógica implementada
- Los datos mostrados son estáticos/de ejemplo
- La funcionalidad de negocio debe ser implementada por el equipo de desarrollo

## Autor

Adaptado del documento "Sistema Contable — Oficina Contable Zulay Angola" (Python/PySide6) a WPF/C#.
