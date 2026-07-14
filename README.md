# Sistema Contable Zulay

Aplicación de escritorio para la **gestión contable multi-empresa**, desarrollada como proyecto de la asignatura *Sistemas de Información 2*. El proyecto parte de un sistema contable anticuado y lo moderniza con una arquitectura actual, persistencia relacional, respaldo en la nube y reportería profesional.

---

## 1. Contexto: Modernización de un Sistema Anticuado

El sistema original presentaba limitaciones típicas de software contable legado: interfaz obsoleta, datos sin estructura relacional, ausencia de respaldos, sin exportación de reportes y sin soporte para inflación. Las mejoras implementadas en esta versión son:

| Área | Sistema anticuado | Sistema modernizado |
|---|---|---|
| Interfaz | Pantallas obsoletas | WPF con patrón **MVVM** y estilos unificados |
| Persistencia | Archivos planos | **SQLite + Entity Framework Core** (con migraciones) |
| Respaldos | Manuales o inexistentes | Backups **ZIP** con historial y restauración integrada |
| Nube | Sin conectividad | Sincronización con **Supabase** (offline-first con cola de reintento) |
| Reportes | Solo en pantalla | Exportación real a **PDF (QuestPDF)** y **Excel (EPPlus)** |
| Inflación | No contemplada | **Reexpresión monetaria por IPC** con histórico local |
| Cierre contable | Manual | **Cierre de ejercicio** con bloqueo de períodos fiscales |
| Cobranza | No integrada | Facturación con **asientos contables automáticos** |

---

## 2. Funcionalidades Principales

* **Multi-empresa:** gestión de varias empresas cliente con selección de empresa activa.
* **Plan de cuentas:** catálogo contable jerárquico con cuentas por defecto.
* **Comprobantes contables:** registro con **partida doble** (Debe/Haber), flujo de estados (Pendiente → Validado → Registrado), actualización y **reversión con contra-asientos**.
* **Libros contables:** Libro Diario y Libro Mayor con saldo acumulado.
* **Estados financieros:** Balance General y Estado de Resultados, con notas explicativas y conclusiones.
* **Cobranza (CxC):** emisión, pago y anulación de facturas generando los comprobantes contables correspondientes de forma automática.
* **Bancos:** registro de transacciones bancarias e importación desde PDFs.
* **Reexpresión por inflación:** ajuste de saldos según IPC con historial auditable.
* **Cierre de ejercicio:** resumen de ingresos/gastos y cierre del período fiscal.
* **Documentos:** registro de documentos recibidos con visor integrado (WebView2).
* **Importación Excel:** carga masiva de datos.
* **Backups:** creación manual y automática de respaldos ZIP, historial y restauración.

---

## 3. Arquitectura y Decisiones Técnicas

* **Tecnología:** Aplicación de escritorio WPF/C# con .NET 10 (Patrón MVVM).
* **Base de Datos:** SQLite local (implementado con Entity Framework Core, con migraciones versionadas en `Migrations/`).
* **Control de Versiones:** Git (Exclusión estricta de archivos `.db`, `.db-shm`, y `.db-wal` en el `.gitignore`).

### Estructura del proyecto

```
Sistema contable/
├── Data/            → DbContext de EF Core (SQLite)
├── Migrations/      → Migraciones versionadas de la base de datos
├── Domain/          → Entidades de negocio (Comprobante, Factura, Cuenta...)
├── Models/          → Modelos auxiliares (Configuración, Backup, Reexpresión)
├── Services/        → Lógica de negocio (Contabilidad, Exportación, IPC, Sync)
├── ViewModels/      → Lógica de presentación (MVVM)
├── Views/           → 23 pantallas XAML
├── Converters/      → Convertidores de datos para XAML
├── Infrastructure/  → Abstracciones de servicios de UI
└── Themes/          → Estilos visuales centralizados
```

### Decisiones de diseño documentadas

* **Persistencia híbrida (SQLite + XML):** los datos **transaccionales** (comprobantes, líneas de asiento, facturas, períodos fiscales) residen en SQLite por su naturaleza relacional y volumen; los datos **maestros y de configuración** (empresas, plan de cuentas, configuración, histórico de IPC) se serializan a XML en la carpeta `Datos/` por simplicidad de despliegue y portabilidad. La migración total a SQLite está prevista como trabajo futuro.
* **IPC con estrategia offline-first:** dado que el BCV no expone una API pública estable, el servicio de IPC consulta primero el histórico local, intenta un endpoint remoto (actualmente simulado) y, en su defecto, interpola entre los valores conocidos más cercanos. Esto garantiza que la reexpresión funcione siempre, con o sin internet.
* **Sincronización con cola offline:** si el envío a Supabase falla (sin conexión, timeout), el registro se encola localmente en la tabla `ColaSincronizacion` y un temporizador reintenta el envío automáticamente.

### Tecnologías y librerías

| Paquete | Uso |
|---|---|
| Entity Framework Core 10 + SQLite | Persistencia principal |
| QuestPDF | Generación de reportes PDF |
| EPPlus / ClosedXML | Exportación e importación de Excel |
| UglyToad.PdfPig | Lectura de PDFs bancarios |
| Microsoft.Web.WebView2 | Visor de documentos embebido |
| Supabase (API REST) | Respaldo y sincronización en la nube |

---

## 4. Instalación y Ejecución

### Requisitos

* Windows 10/11
* [SDK de .NET 10](https://dotnet.microsoft.com/download) (con carga de trabajo de escritorio)

### Compilar y ejecutar

```powershell
git clone <url-del-repositorio>
cd "Sistemas-de-Informacion-2/Sistema contable"
dotnet build
dotnet run
```

### Datos generados en tiempo de ejecución

* **Base de datos:** `%LocalAppData%\SistemaContable\contable.db` (se crea automáticamente).
* **Archivos maestros:** carpeta `Datos/` junto al ejecutable.
* **Backups:** carpeta `Backups/` junto al ejecutable.
* **Configuración de nube:** credenciales de Supabase en `appsettings.json` (no incluir claves reales en el repositorio).

---

## 5. Estrategia de Respaldo y Recuperación (Supabase)

Para garantizar la integridad de los datos y ofrecer una recuperación rápida ante fallos de hardware del cliente, se utiliza **Supabase** como infraestructura en la nube:

* **Almacenamiento Automatizado:** Los archivos de respaldo de la base de datos SQLite se suben automáticamente a un *bucket* de Supabase utilizando su API REST directamente desde la aplicación.
* **Recuperación ante Desastres:** Si la computadora del cliente sufre daños irreversibles, solo necesitará instalar el Sistema Contable en un equipo nuevo. La aplicación se conectará a la API de Supabase para descargar el último respaldo disponible y restaurar la operatividad de inmediato.
* **Prohibición en Git:** El archivo crudo de la base de datos local nunca debe subirse al repositorio de código fuente para evitar conflictos de versiones.

---

## 6. Distribución del Equipo: Núcleo Crítico

### Jesús Chacón — Entradas Complejas, Seguridad y Nube
* **Módulos asignados:** Bancos, Reexpresión monetaria y Backup.
* **Responsabilidad:** Programar la importación y parseo de transacciones desde PDFs bancarios, implementar el cálculo del IPC, y desarrollar la conexión con la API de Supabase para la carga y descarga automatizada de los respaldos de la base de datos.

### Omar Angola — Base de Datos y Motor Contable
* **Módulos asignados:** Arquitectura de Base de Datos, Comprobantes y Cierre de Ejercicio.
* **Responsabilidad:** Diseñar el esquema relacional en SQLite utilizando EF Core, garantizar la validación matemática estricta de la partida doble en los asientos y programar el bloqueo operativo de los periodos fiscales cerrados.

### Carlos Ocariz — Reportes Financieros y Salidas
* **Módulos asignados:** Libros, Informes y Exportaciones reales.
* **Responsabilidad:** Construir la lógica de cálculo precisa para el Balance General, Estado de Resultados y Libro Mayor, además de integrar las librerías necesarias para exportar documentos funcionales y listos para imprimir en PDF y Excel.

---

## 7. Distribución del Equipo: Interfaz y Flujos Operativos

### Francisco Villasmil — Experiencia de Usuario y Parámetros
* **Módulos asignados:** Dashboard, Configuración y Documentos.
* **Responsabilidad:** Integrar librerías de gráficos visuales en el panel de inicio, vincular la configuración fiscal a la base de datos y culminar el visor de archivos adjuntos mediante tecnología WebView2.

### Isaac Diaz — Estructura Base y Operaciones Rápidas
* **Módulos asignados:** Plan de Cuentas y Movimientos.
* **Responsabilidad:** Desarrollar las vistas y el CRUD completo para gestionar el catálogo contable respetando su jerarquía financiera, e implementar la interfaz de registro rápido de ingresos y egresos.

### Miguel Urbina — Gestión de Terceros
* **Módulos asignados:** Cobranza y Cuentas por Pagar.
* **Responsabilidad:** Maquetar y programar los flujos de facturación, estados de cuenta de clientes, registro de pagos y preparar las pantallas estructurales para la futura gestión de proveedores y compras.

---

## 8. Trabajo Futuro

Mejoras identificadas y planificadas para versiones posteriores:

* **Autenticación y roles de usuario** (contador / administrador) con auditoría de operaciones.
* **Migración total de la persistencia a SQLite**, unificando los datos maestros hoy almacenados en XML.
* **Sincronización bidireccional** con Supabase (descarga y resolución de conflictos) para trabajo multi-equipo.
* **Cuentas por Pagar (CxP):** módulo espejo de Cobranza para proveedores y compras.
* **Integración de una fuente oficial de IPC** o pantalla de carga manual de índices certificados.
* **Suite de pruebas automatizadas** sobre la lógica contable crítica (partida doble, cierres, reversiones, saldos).
* **Reportes fiscales:** libros de IVA compras/ventas y retenciones.
* **Multimoneda** con tasas de cambio históricas.
* **Instalador (MSIX)** y mecanismo de actualización automática.

---

## 9. Documentación Adicional

* [`Sistema contable/ANALISIS_PROYECTO.md`](Sistema%20contable/ANALISIS_PROYECTO.md) — análisis técnico detallado del sistema: entidades, servicios, pantallas y evaluación de estado.
