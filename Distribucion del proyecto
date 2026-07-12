# Sistema Contable Zulay - Análisis y Distribución del Proyecto

## 1. Arquitectura y Decisiones Técnicas

* **Tecnología:** Aplicación de escritorio WPF/C# con .NET 10 (Patrón MVVM).
* **Base de Datos:** SQLite local (implementado con Entity Framework Core).
* **Control de Versiones:** Git (Exclusión estricta de archivos `.db`, `.db-shm`, y `.db-wal` en el `.gitignore`).

## 2. Estrategia de Respaldo y Recuperación (Supabase)

Para garantizar la integridad de los datos y ofrecer una recuperación rápida ante fallos de hardware del cliente, se utilizará **Supabase** como infraestructura en la nube:

* **Almacenamiento Automatizado:** Los archivos de respaldo de la base de datos SQLite se subirán automáticamente a un *bucket* de Supabase utilizando su API REST directamente desde la aplicación.
* **Recuperación ante Desastres:** Si la computadora del cliente sufre daños irreversibles, solo necesitará instalar el Sistema Contable en un equipo nuevo. La aplicación se conectará a la API de Supabase para descargar el último respaldo disponible y restaurar la operatividad de inmediato.
* **Prohibición en Git:** El archivo crudo de la base de datos local nunca debe subirse al repositorio de código fuente para evitar conflictos de versiones.

---

## 3. Distribución del Equipo: Núcleo Crítico

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

## 4. Distribución del Equipo: Interfaz y Flujos Operativos

### Francisco Villasmil — Experiencia de Usuario y Parámetros
* **Módulos asignados:** Dashboard, Configuración y Documentos.
* **Responsabilidad:** Integrar librerías de gráficos visuales en el panel de inicio, vincular la configuración fiscal a la base de datos y culminar el visor de archivos adjuntos mediante tecnología WebView2.

### Isaac Diaz — Estructura Base y Operaciones Rápidas
* **Módulos asignados:** Plan de Cuentas y Movimientos.
* **Responsabilidad:** Desarrollar las vistas y el CRUD completo para gestionar el catálogo contable respetando su jerarquía financiera, e implementar la interfaz de registro rápido de ingresos y egresos.

### Miguel Urbina — Gestión de Terceros
* **Módulos asignados:** Cobranza y Cuentas por Pagar.
* **Responsabilidad:** Maquetar y programar los flujos de facturación, estados de cuenta de clientes, registro de pagos y preparar las pantallas estructurales para la futura gestión de proveedores y compras.
