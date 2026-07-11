# Análisis del proyecto — Sistema Contable WPF/C#

## 1. Tecnología y arquitectura actual

- **Tecnología**: aplicación de escritorio WPF/C# con .NET 10 (`net10.0-windows`), patrón MVVM.
- **Persistencia**: NO hay base de datos. El servicio singleton `ContabilidadService` guarda todo en archivos XML en `Datos/` (`comprobantes.xml`, `cuentas.xml`, `empresas.xml`, `periodos.xml`, `facturas.xml`). Véase `Sistema contable/Services/ContabilidadService.cs`.
- **UI**: 11 pestañas principales y varias ventanas de diálogo en `Views/`.
- **Modelos**: hay dos namespaces separados (`SistemaContableZulay.UI.Domain` y `Sistema_contable.Models`) que aún no están unificados.
- **Librerías extra**: `UglyToad.PdfPig` para extraer texto de PDFs bancarios y `Microsoft.Web.WebView2` para previsualizarlos.
- **Autenticación / roles**: no existe.
- **Reportes**: las vistas tienen botones, pero la generación real de PDF/Excel/impresión está en placeholders.

---

## 2. Funciones que YA tiene el proyecto

| Módulo | Funcionalidad implementada | Archivos relevantes |
|---|---|---|
| **Empresas** | CRUD de empresas, validación de RIF único, teléfono, selección de empresa activa. | `EmpresasViewModel.cs`, `GestionEmpresasWindow.xaml`, `ContabilidadService.Guardar/EliminarEmpresa` |
| **Plan de Cuentas** | Jerarquía de cuentas contables, seed de cuentas por defecto, agregar/editar/eliminar. | `PlanCuentasViewModel.cs`, `ContabilidadService.Guardar/EliminarCuenta` |
| **Comprobantes** | Asientos manuales con validación de partida doble, totales Debe/Haber, indicador de cuadre, guardado. | `ComprobantesViewModel.cs`, `AsientoViewModel.cs`, `ContabilidadService.GuardarComprobante` |
| **Libro Diario / Mayor** | Visualización de movimientos, filtro por cuenta, saldo acumulado. | `LibroDiarioViewModel.cs`, `LibroMayorViewModel.cs` |
| **Cobranza** | Facturas de clientes, estados (Pendiente/Vencida/Pagada/Anulada), marcar pagada, anular, genera asientos contables automáticos, filtros. | `CobranzaViewModel.cs`, `FacturaCobranza.cs`, `ContabilidadService.Guardar/Marcar/AnularFactura` |
| **Movimientos rápidos** | Registro simplificado de ingresos/egresos con contrapartida automática en Caja General. | `MovimientosViewModel.cs` |
| **Bancos** | Importación de PDF bancario, extracción de texto, parseo de transacciones, mapeo automático a cuentas, importación como comprobantes. | `BancosViewModel.cs`, `PdfParserHelper.cs`, `Bancos.xaml.cs` |
| **Reexpresión monetaria** | Consulta de IPC (con fallback e interpolación), cálculo de factor, ajuste por inflación y generación de asiento REI. | `ReexpresionViewModel.cs`, `IpcService.cs`, `PartidaReexpresion.cs` |
| **Cierre de ejercicio** | Cálculo de resultado, asiento de cierre automático y bloqueo del período. | `CerrarEjercicio.xaml.cs`, `ContabilidadService.CerrarEjercicio` |
| **Reversión de comprobantes** | Selección de comprobante, vista previa del contra-asiento y reversión. | `ReversarComprobantes.xaml.cs`, `ContabilidadService.ReversarComprobante` |
| **Actualizar comprobantes** | Búsqueda por fecha/tipo y cambio masivo de estado (validado, etc.). | `ActualizarComprobantes.xaml.cs`, `ContabilidadService.ActualizarEstadoComprobante` |
| **Dashboard** | KPIs de ingresos/gastos/saldo, alertas de comprobantes descuadrados y ejercicios abiertos, movimientos recientes. | `DashboardViewModel.cs` |
| **Backup** | Respaldo en ZIP de la carpeta `Datos/`. | `ContabilidadService.EjecutarRespaldoAutomatico` |

> **Módulos que solo tienen UI sin lógica (placeholders):** `Informes`, `Documentos`, `Configuración`, `Planillas`, `ImportarExcel`.

---

## 3. Qué le falta para ser un sistema contable completo

Para dejar de ser una aplicación de “UI + XML local” y convertirse en un sistema contable robusto, faltan principalmente:

1. **Base de datos relacional** (no XML).
3. **Motor de reportes reales** (Balance General, Estado de Resultados, Balance de Comprobación, Libros, etc.).
4. **Exportación/impresión real** de PDF, Excel y papel.
5. **Cuentas por pagar completo** (proveedores, órdenes, pagos, retenciones).
6. **Cuentas por cobrar avanzado** (clientes, estados de cuenta, recordatorios, notas de crédito).
7. **Conciliación bancaria**.
8. **Activos fijos y depreciación**.
9. **Inventario / costos**.
10. **Nómina e impuestos (IVA, ISLR, SSO, RPPE, etc.)**.
11. **Multi-moneda y tasas de cambio**.
12. **Auditoría y trazabilidad** de cambios.
13. **Gestión documental real** (adjuntar archivos).
14. **Importación/exportación Excel real** (plan de cuentas, asientos, facturas).
15. **Notificaciones por correo/recordatorios**.

A continuación se dividen las funciones faltantes en tres equipos y los pasos para implementarlas.

---

## 4. Funciones faltantes por equipo

> **Aclaración**: como el proyecto es WPF de escritorio, se entiende **Backend** como la capa de servicios/lógica de negocio (y opcionalmente una API futura), **Frontend** como las vistas XAML + ViewModels, y **Base de datos** como el motor de persistencia.

---

### Equipo 1 — Base de Datos

Responsabilidad: diseñar el esquema relacional, migrar los datos XML, implementar el acceso a datos con EF Core, seed, índices y procedimientos de respaldo.

| Función faltante | Pasos para implementar |
|---|---|
| **Esquema relacional** | 1. Definir entidades: `Empresa`, `Usuario`, `Rol`, `Permiso`, `CuentaContable`, `Comprobante`, `AsientoLinea`, `Factura`, `Pago`, `Cliente`, `Proveedor`, `Banco`, `TransaccionBancaria`, `Conciliacion`, `PeriodoFiscal`, `Ipc`, `Documento`, `Configuracion`, `Auditoria`, `ActivoFijo`, `Inventario`, `Nomina`, `Impuesto`. <br>2. Crear diagrama E-R con claves foráneas. <br>3. Implementar `DbContext` con EF Core. <br>4. Configurar `OnModelCreating` para relaciones, restricciones y seed. |
| **Migración de XML a SQL** | 1. Leer `Datos/*.xml` con `ContabilidadService.CargarDatos`. <br>2. Mapear registros a entidades EF Core. <br>3. Ejecutar un script/migración de carga inicial. <br>4. Validar integridad (totales Debe/Haber, códigos de cuenta). |
| **Seed de datos** | 1. Insertar plan de cuentas base (1 Activo, 2 Pasivo, etc.). <br>2. Crear usuario `admin` por defecto. <br>3. Cargar tabla `Ipc` con valores históricos. <br>4. Configurar parámetros fiscales iniciales (IVA, ISLR, moneda). |
| **Restricciones e índices** | 1. Índices en `CuentaContable.Codigo`, `Comprobante.Fecha`, `Factura.NumeroFactura`, `AsientoLinea.CodigoCuenta`. <br>2. Constraint `CHECK` para que `Debe` y `Haber` no sean negativos. <br>3. Trigger o interceptor de auditoría en tablas contables. |
| **Backup y restore** | 1. Crear stored procedure / script `backup-db.sql`. <br>2. En SQLite: copiar archivo `.db` y comprimir. <br>3. En SQL Server: usar `BACKUP DATABASE`. <br>4. Guardar histórico de backups en tabla `BackupLog`. |

---

### Equipo 2 — Backend

Responsabilidad: implementar lógica de negocio, reportes, exportaciones, validaciones, seguridad y módulos nuevos.

| Función faltante | Pasos para implementar |
|---|---|
| **Reportes financieros** | 1. Implementar `ReportesService` con métodos: `BalanceGeneral`, `EstadoResultados`, `BalanceComprobacion`, `LibroDiario`, `LibroMayor`, `FlujoEfectivo`. <br>2. Calcular saldos por cuenta considerando tipo (Activo/Egreso = Debe - Haber; Pasivo/Patrimonio/Ingreso = Haber - Debe). <br>3. Generar DTOs de reporte. |
| **Exportación/impresión real** | 1. Para Excel: usar `ClosedXML` o `EPPlus`. <br>2. Para PDF: `QuestPDF`, `iText` o `PdfSharp`. <br>3. Para impresión: `PrintDocument` en WPF. <br>4. Agregar `SaveFileDialog` en frontend. <br>5. Implementar `ExportarService.ExportarLibroDiarioAsync(formato)`. |
| **Conciliación bancaria** | 1. Crear `ConciliacionService`. <br>2. Comparar transacciones importadas de banco vs. movimientos contables por fecha/referencia/monto. <br>3. Permitir marcar transacciones como “conciliadas”. <br>4. Generar reporte de diferencias. |
| **Cuentas por pagar** | 1. Crear `Proveedor` y `CuentaPorPagar` entidades. <br>2. CRUD de proveedores. <br>3. Registro de facturas de compra, vencimientos y pagos. <br>4. Generación de retenciones (ISLR, IVA). <br>5. Asiento contable automático al registrar pago. |
| **Cuentas por cobrar avanzado** | 1. CRUD de `Cliente`. <br>2. Estados de cuenta por cliente. <br>3. Generación de recordatorios de pago. <br>4. Notas de crédito / débito. <br>5. Envío de correo con `MailKit`/`System.Net.Mail`. |
| **Activos fijos** | 1. Crear `ActivoFijo` con datos de adquisición, vida útil y método de depreciación. <br>2. Calcular depreciación mensual/anual. <br>3. Generar asiento de depreciación. |
| **Inventario** | 1. Crear `Producto`, `EntradaInventario`, `SalidaInventario`. <br>2. Métodos de costo: PEPS, UEPS, promedio. <br>3. Asiento de costo de ventas. <br>4. Kardex de inventario. |
| **Nómina e impuestos** | 1. Crear `Empleado`, `Nomina`, `NominaDetalle`. <br>2. Calcular sueldos, deducciones legales (SSO, RPPE, FAOV). <br>3. Generar asiento de nómina. <br>4. Módulo de impuestos: IVA, ISLR, retenciones. <br>5. Generar declaraciones/txt para SENIAT. |
| **Multi-moneda** | 1. Crear `Moneda` y `TasaCambio` con historial. <br>2. Convertir montos a moneda funcional. <br>3. Revaluación de saldos en moneda extranjera. |
| **Importación/exportación Excel** | 1. Usar `ClosedXML` para leer/escribir `.xlsx`. <br>2. Plantillas de importación para plan de cuentas, comprobantes, clientes, proveedores. <br>3. Validar filas y reportar errores. |
| **Auditoría** | 1. Crear `AuditoriaService`. <br>2. Registrar INSERT/UPDATE/DELETE en entidades críticas. <br>3. Mostrar trazabilidad en UI. |
fallo de importación de banco a movimientos

---

### Equipo 3 — Frontend

Responsabilidad: construir/terminar las pantallas XAML, ViewModels, bindings, gráficos, visores de reportes y flujos de usuario.

| Función faltante | Pasos para implementar |
|---|---|
| **Dashboard con gráficos reales** | 1. Integrar librería `LiveCharts` o `ScottPlot.WPF`. <br>2. Gráficos: ingresos vs gastos, evolución de saldos, cobranza por estado. <br>3. Bindings a `DashboardViewModel`. |
| **Visor de reportes** | 1. Terminar `Informes.xaml` con `DataGrid`/previsualización. <br>2. Agregar selección de tipo, fechas y formato. <br>3. Botones “Exportar PDF”, “Exportar Excel”, “Imprimir”. <br>4. Usar `WebView2` o `FlowDocument` para preview. |
| **Libro Diario / Mayor exportable** | 1. Reemplazar `MessageBox` placeholders en `LibroDiarioViewModel` y `LibroMayorViewModel` por llamadas reales a `ExportarService`. <br>2. Agregar filtros de fecha y cuenta. |
| **Conciliación bancaria UI** | 1. Crear vista `ConciliacionBancaria.xaml`. <br>2. DataGrid con movimientos del banco y movimientos contables. <br>3. Botón “Conciliar” y reporte de diferencias. |
| **Proveedores y Cuentas por pagar** | 1. Crear `ProveedoresView` y `CuentasPorPagarView`. <br>2. Formulario de factura de compra. <br>3. Pantalla de pagos y retenciones. |
| **Clientes y Cuentas por cobrar** | 1. Crear `ClientesView` con CRUD. <br>2. Estado de cuenta por cliente. <br>3. Botón “Enviar recordatorio por correo”. |
| **Activos fijos / Inventario / Nómina** | 1. Crear vistas `ActivosFijos`, `Inventario`, `Nomina`. <br>2. Formularios de alta, baja, depreciación, movimientos de inventario, cálculo de nómina. <br>3. Generación de asientos contables desde cada vista. |
| **Documentos adjuntos** | 1. Terminar `Documentos.xaml` y `DocumentosViewModel`. <br>2. `OpenFileDialog` para seleccionar archivos. <br>3. Guardar ruta/blob en base de datos. <br>4. Previsualización con `WebView2`. |
| **Configuración funcional** | 1. Terminar `Configuracion.xaml` con bindings reales. <br>2. Guardar parámetros fiscales (IVA, ISLR, moneda, ejercicio). <br>3. Pantalla de backup/restore. |
| **Importar Excel UI** | 1. Terminar `ImportarExcel.xaml` con mapeo de columnas. <br>2. Mostrar preview de filas antes de importar. <br>3. Reportar filas con error. |
| **Notificaciones** | 1. Mostrar alertas de vencimiento de facturas, ejercicios por cerrar, saldos negativos. <br>2. Badge de alertas en Dashboard y menú. |

---

## 5. Ruta crítica recomendada

1. **Base de datos** primero: sin un esquema relacional, todo lo demás se construye sobre XML frágil.
2. **Backend** de acceso a datos: repositorios + Unit of Work + `ContabilidadService` refactorizado.
4. **Reportes financieros**: es el corazón de un sistema contable.
5. **Exportación real**: los usuarios necesitan PDF/Excel de libros y reportes.
6. **Módulos operativos**: cobranza/pagos, bancos, activos fijos, inventario, nómina.
7. **Integraciones**: importación Excel, correos, SENIAT.
8. **Auditoría y backup**: trazabilidad y respaldo robusto.

---

## Resumen

El proyecto tiene una base visual sólida y varios módulos contables funcionando con XML (empresas, plan de cuentas, comprobantes, cobranza, bancos, reexpresión, cierre y reversión). Para ser un sistema contable completo, **le falta principalmente una base de datos real, motor de reportes, exportación/impresión real, conciliación bancaria, cuentas por pagar, activos fijos, inventario, nómina, impuestos y auditoría**. La división de trabajo sugerida es: **Base de datos** diseña el esquema y migraciones; **Backend** implementa servicios, reportes, validaciones y lógica de negocio; **Frontend** termina las vistas, gráficos, visores y flujos de usuario.
