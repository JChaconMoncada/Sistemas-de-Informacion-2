# Análisis del Proyecto — Sistema Contable

> Documento generado a partir del análisis del código fuente (Julio 2026).

---

## 1. Descripción General

Aplicación de escritorio **WPF (.NET 10, Windows)** para la gestión contable multi-empresa. Permite llevar la contabilidad de varias empresas cliente: comprobantes contables con partida doble, libros diario y mayor, estados financieros, cobranza (facturación), bancos, reexpresión por inflación (IPC), cierre de ejercicio, backups y sincronización en la nube con **Supabase**.

**Patrón de arquitectura:** MVVM (Model–View–ViewModel).

**Tecnologías / paquetes:**

| Paquete | Uso |
|---|---|
| Entity Framework Core 10 + SQLite | Persistencia principal (comprobantes, facturas, períodos) |
| EPPlus / ClosedXML | Exportar e importar Excel |
| QuestPDF | Generación de reportes PDF |
| UglyToad.PdfPig | Lectura de PDFs (importación de documentos) |
| Microsoft.Web.WebView2 | Contenido web embebido (ayuda/dashboard) |
| Supabase (REST) | Sincronización en la nube |

---

## 2. Estructura del Proyecto

```
Sistema contable/
├── App.xaml / App.xaml.cs        → Punto de entrada de la aplicación
├── Data/
│   └── ContabilidadDbContext.cs  → DbContext EF Core (SQLite)
├── Migrations/                   → Migraciones EF Core
├── Domain/                       → Entidades de negocio
├── Models/                       → Modelos auxiliares (config, backup, reexpresión)
├── Services/                     → Lógica de negocio y servicios
├── ViewModels/                   → Lógica de presentación (MVVM)
├── Views/                        → Pantallas XAML (23 vistas)
├── Converters/                   → Convertidores XAML
├── Infrastructure/               → WpfMessageBoxService
└── Themes/Styles.xaml            → Estilos visuales
```

---

## 3. Persistencia de Datos (Híbrida)

El sistema usa **dos mecanismos de almacenamiento** en paralelo:

### 3.1 SQLite (EF Core) — `%LocalAppData%\SistemaContable\contable.db`
Entidades gestionadas por `ContabilidadDbContext`:
- `ComprobantesContables` (PK: `IdComprobante`)
- `LineasAsiento` (relación 1-N con comprobante, borrado en cascada)
- `FacturasCobranza`
- `PeriodosFiscales` (PK compuesta: Año + Mes)
- `ColaSincronizacion` (cola offline para Supabase)

### 3.2 Archivos XML — carpeta `Datos/` junto al ejecutable
Serialización XML para:
- `empresas.xml` — empresas cliente
- `cuentas.xml` — plan de cuentas
- `documentos.xml` — documentos recibidos
- `configuracion.xml` — configuración del sistema
- `historial_reexpresiones.xml` — historial de reexpresión
- `ipc_historico.xml` — valores de IPC

### 3.3 Supabase (nube)
`SupabaseSyncService` hace *upsert* vía REST (`on_conflict=sync_id`) a tablas: `facturas_cobranza`, `comprobantes_contables`, `asiento_lineas`. Si falla (sin internet), el payload se guarda en la tabla local `ColaSincronizacion` para reintento (timer en `ContabilidadService`).

---

## 4. Dominio (Entidades)

| Entidad | Descripción |
|---|---|
| `EmpresaCliente` | Empresa gestionada: nombre, RIF, razón social, dirección, contacto |
| `ComprobanteContable` | Comprobante con estado (Pendiente → Validado → Registrado), tipo (Ingreso/Egreso/Diario), moneda, líneas de asiento; calcula `TotalDebe`/`TotalHaber` |
| `AsientoLinea` | Línea de partida doble: código de cuenta, descripción, Debe, Haber |
| `CuentaContable` | Cuenta del plan de cuentas |
| `FacturaCobranza` | Factura por cobrar: cliente, montos, vencimiento, estado, vínculos a comprobantes de emisión/pago/reversión |
| `PeriodoFiscal` | Período (año/mes) con bandera de cerrado |
| `ResumenEjercicio` | Totales de ingresos/gastos/resultado para cierre |
| `ColaSincronizacion` | Payload JSON pendiente de sincronizar |
| `IpcRecord` | Registro histórico de IPC (fecha, valor) |

**Modelos auxiliares (`Models/`):** `ConfiguracionSistema`, `BackupInfo`, `Documento`, `HistorialReexpresion`, `PartidaReexpresion`.

---

## 5. Servicios

### 5.1 `ContabilidadService` (singleton, ~1045 líneas — el núcleo)
- **Empresas:** CRUD, selección de empresa activa (todo el sistema filtra por `EmpresaActivaId`).
- **Plan de cuentas:** CRUD + siembra de cuentas por defecto.
- **Comprobantes:** guardar, actualizar estado, eliminar, **reversar** (genera contra-asiento).
- **Cobranza:** guardar factura (genera comprobante de emisión), marcar pagada (comprobante de pago), anular (comprobante de reversión).
- **Cierre de ejercicio:** resumen de ingresos/gastos, cierre por año, verificación de período cerrado.
- **Saldos:** saldo de cuenta a fecha / entre fechas.
- **Backups:** crear ZIP manual/automático, historial, restaurar.
- **Sincronización:** envío a Supabase con cola offline y timer de reintento.
- **Reexpresión:** historial de reexpresiones por cuenta.
- **Documentos y configuración:** CRUD sobre XML.

### 5.2 `ExportacionService`
Exporta a **PDF (QuestPDF)** y **Excel (EPPlus)**:
- Libro Diario, Libro Mayor (con saldo acumulado), Informes/Estados financieros (con notas explicativas y conclusiones).

### 5.3 `IpcService` (singleton)
Obtiene el IPC para reexpresión por inflación con estrategia *offline-first*:
1. Busca en histórico local (`ipc_historico.xml`).
2. Intenta API externa (**URL de ejemplo, no real**: `api.example.bcv.org.ve`).
3. Fallback: interpolación entre valores cercanos, o valor base 1000.

### 5.4 `SupabaseSyncService`
Cliente REST hacia Supabase con API key desde `ConfiguracionApp` (lee `appsettings.json`). Timeout de 8 s; en fallo devuelve `false` y el llamador encola.

### 5.5 `IMessageBoxService` / `WpfMessageBoxService`
Abstracción de MessageBox para desacoplar los ViewModels de WPF (uso parcial: varios ViewModels aún llaman `MessageBox` directamente).

---

## 6. Pantallas (Views) y su Función

| Vista | Función |
|---|---|
| `MainWindow` | Ventana principal: navegación, selector de empresa activa, estado de backup |
| `Dashboard` | Panel de indicadores/resumen |
| `Empresas` / `GestionEmpresasWindow` | CRUD de empresas cliente |
| `PlanCuentas` | Gestión del plan de cuentas |
| `Comprobantes` | Registro de comprobantes con partida doble |
| `ActualizarComprobantes` | Edición/validación de comprobantes |
| `ReversarComprobantes` | Reversión (contra-asientos) |
| `Movimientos` | Consulta de movimientos contables |
| `LibroDiario` / `LibroMayor` | Libros contables con exportación PDF/Excel |
| `EstadoFinanciero` | Balance general / Estado de resultados |
| `Informes` | Informes con notas y conclusiones, exportables |
| `Cobranza` | Facturación y cuentas por cobrar |
| `Bancos` | Transacciones bancarias |
| `Planillas` | Planillas (nómina/formatos) |
| `Reexpresion` | Reexpresión por inflación usando IPC |
| `CerrarEjercicio` | Cierre del ejercicio fiscal |
| `Documentos` | Registro de documentos recibidos |
| `ImportarExcel` | Importación de datos desde Excel |
| `HistorialCuentaView` | Historial de movimientos de una cuenta |
| `Configuracion` | Configuración del sistema |
| `Ayuda` | Pantalla de ayuda |

Cada vista tiene su ViewModel correspondiente en `ViewModels/`, todos heredando de `ViewModelBase` (INotifyPropertyChanged) y usando `RelayCommand`.

---

## 7. Flujo Contable Principal

1. Se registra/selecciona una **empresa activa** (`MainWindowViewModel`).
2. Se configura el **plan de cuentas** (o se usan las cuentas sembradas por defecto).
3. Se registran **comprobantes** con líneas Debe/Haber (validación de partida doble).
4. Los comprobantes alimentan **Libro Diario, Libro Mayor y Estados Financieros**.
5. **Cobranza** genera comprobantes automáticos al emitir/pagar/anular facturas.
6. Al final del período: **reexpresión por IPC** y **cierre de ejercicio**.
7. Todo se respalda en **ZIP** y se sincroniza con **Supabase** cuando hay internet.

---

## 8. Qué le Falta / Puntos de Mejora

### Críticos
- **Persistencia duplicada e inconsistente:** conviven XML y SQLite para datos relacionados (empresas/cuentas en XML, comprobantes en SQLite). Riesgo de inconsistencia y dificultad de mantenimiento. *Recomendación: migrar todo a SQLite.*
- **Sin autenticación ni usuarios:** cualquiera que abra la app accede a todo. Falta login, roles (contador/administrador) y auditoría de quién hizo cada operación.
- **API de IPC ficticia:** `IpcService` apunta a `api.example.bcv.org.ve` (no existe). Siempre cae al fallback. Falta integrar una fuente real de IPC o una pantalla de carga manual de índices.
- **API key de Supabase en `appsettings.json`:** la clave anónima se distribuye con la app; deben aplicarse políticas RLS estrictas en Supabase.

### Importantes
- **Sin pruebas automatizadas:** no existe ningún proyecto de tests. La lógica contable (partida doble, cierres, reversiones, saldos) es la más crítica para testear.
- **Sincronización unidireccional:** solo se sube a Supabase; no hay descarga/merge, por lo que no sirve para multi-equipo. Tampoco hay resolución de conflictos.
- **`ContabilidadService` monolítico (~1045 líneas):** mezcla persistencia, negocio, backups y sync. Convendría separar en repositorios/servicios (EmpresaService, ComprobanteService, BackupService...).
- **Manejo de errores silencioso:** varios `catch { }` vacíos (IpcService, carga de XML) ocultan fallos. Falta logging estructurado (Serilog/NLog).
- **ViewModels acoplados a WPF:** existe `IMessageBoxService` pero muchos ViewModels usan `MessageBox.Show` directo, dificultando pruebas.

### Deseables
- **Validaciones contables adicionales:** impedir asientos descuadrados a nivel de servicio (no solo UI), bloquear registros en períodos cerrados en todas las rutas.
- **Multimoneda real:** existe campo `Moneda` pero no hay tasas de cambio ni conversión.
- **Cuentas por pagar:** existe Cobranza (CxC) pero no el módulo espejo de proveedores (CxP).
- **Inventario / activos fijos y depreciación:** módulos típicos ausentes.
- **Reportes fiscales:** libros de IVA compras/ventas, retenciones (relevante en Venezuela por el RIF/BCV).
- **Instalador y actualizaciones:** no hay setup (MSIX/Inno) ni mecanismo de update.
- **Backup automático programado:** el respaldo automático existe pero solo se dispara en reexpresión; falta programación periódica.
- **README del repositorio:** documentación de instalación, requisitos y uso para nuevos desarrolladores.

---

## 9. Resumen

| Aspecto | Estado |
|---|---|
| Arquitectura MVVM | ✔ Implementada correctamente |
| Contabilidad de partida doble | ✔ Funcional (comprobantes, libros, estados) |
| Multi-empresa | ✔ Funcional |
| Cobranza con asientos automáticos | ✔ Funcional |
| Reexpresión por inflación | ◐ Funcional pero con API de IPC ficticia |
| Cierre de ejercicio | ✔ Funcional |
| Backups ZIP | ✔ Funcional |
| Sincronización nube | ◐ Solo subida, con cola offline |
| Seguridad / usuarios | ✘ No existe |
| Pruebas automatizadas | ✘ No existen |
| Persistencia unificada | ✘ Híbrida XML + SQLite |
