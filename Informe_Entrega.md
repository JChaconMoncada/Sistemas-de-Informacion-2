# Informe de Primera Entrega - Sistema Contable

A continuación se presenta el informe correspondiente a la primera entrega del sistema contable en desarrollo, detallando las limitaciones encontradas, el glosario de términos del sistema y las indicaciones sobre la actualización de diagramas.

## 1. Limitaciones y Restricciones durante el Desarrollo

Durante el desarrollo de esta primera fase, nos encontramos con ciertas limitaciones que nos obligaron a adaptar el alcance inicial:

*   **Implementación del Patrón Arquitectónico (MVVM vs MVC):** Si en el levantamiento inicial se planteó un modelo MVC tradicional, al utilizar **WPF** (Windows Presentation Foundation) para el desarrollo de escritorio, la restricción del framework nos obligó a adoptar el patrón **MVVM (Model-View-ViewModel)**. Esto requirió un esfuerzo adicional para crear clases base (`ViewModelBase`, `RelayCommand`) y realizar el "Data Binding" para sincronizar la vista y los datos, modificando la estructura de clases inicialmente pensada.
*   **Complejidad en el Módulo de Reexpresión:** El cálculo de la reexpresión de estados financieros por inflación requiere el manejo estricto de índices (IPC) históricos. La restricción técnica de no contar con una API externa automática para el Índice de Precios al Consumidor (IPC) nos limitó a requerir la carga manual o local de estos registros (`IpcRecord` y `PartidaReexpresion`), lo cual no estaba contemplado inicialmente como un proceso manual.
*   **Generación de Reportes y Exportación:** La creación de reportes complejos (como el Libro Mayor o Libro Diario) en formatos estándar (PDF/Excel) de forma nativa en WPF es limitada sin el uso de librerías de terceros (muchas de ellas de pago). Por ende, la funcionalidad se restringió en esta fase a la visualización robusta en pantalla a través de DataGrids (`LibroDiario.xaml`, `LibroMayor.xaml`), postergando la exportación física.
*   **Manejo de Asincronía en Operaciones de Base de Datos:** Para mantener la interfaz fluida (UI thread) al cargar listas extensas (como el Plan de Cuentas o Movimientos), fue necesario implementar operaciones asíncronas en los ViewModels. Esta limitante de rendimiento del hilo principal no fue contemplada en el diseño inicial y requirió refactorización en la lógica de acceso a datos.

## 2. Glosario de Términos (Palabras Clave)

Este glosario define los términos fundamentales tanto del negocio (contabilidad) como del sistema desarrollado:

*   **Asiento Contable (`Asiento` / `AsientoLinea`):** Registro formal de una transacción financiera en el libro diario. Todo asiento debe cumplir con el principio de partida doble (el total de débitos debe ser igual al total de créditos).
*   **Comprobante Contable (`ComprobanteContable`):** Documento físico o digital que respalda y justifica las operaciones y asientos contables realizados en el sistema.
*   **Cuenta Contable (`CuentaContable`):** Instrumento de registro donde se detallan los aumentos y disminuciones de un activo, pasivo, capital, ingreso o egreso.
*   **Plan de Cuentas:** Listado estructurado y codificado que presenta todas las cuentas necesarias para registrar los hechos contables de la empresa.
*   **Libro Diario:** Registro contable principal cronológico en el cual se anotan todas las operaciones (asientos) de la empresa día a día.
*   **Libro Mayor:** Resumen o agrupación de todas las cuentas contables con sus respectivos movimientos (débitos y créditos) y saldos actualizados.
*   **Reexpresión (Ajuste por Inflación):** Proceso contable, reflejado en el módulo `Reexpresion.xaml`, utilizado para actualizar los valores históricos de las partidas no monetarias basándose en la inflación.
*   **IPC (Índice de Precios al Consumidor):** Indicador estadístico fundamental cargado en el sistema (`IpcRecord`) que sirve como base de cálculo para el módulo de reexpresión.
*   **MVVM (Model-View-ViewModel):** Patrón de arquitectura de software utilizado en el proyecto para separar completamente la lógica de negocio (Models/Domain) de la interfaz gráfica (Views), utilizando intermediarios (ViewModels).
*   **Data Binding (Enlace de Datos):** Mecanismo de WPF que permite que las interfaces gráficas se actualicen automáticamente cuando cambian los datos subyacentes en el ViewModel.

## 3. Actualización de Diagramas

De acuerdo a la evolución del código actual, se deben aplicar los siguientes cambios a la documentación inicial:

*   **Diagrama de Clases:** 
    *   *Actualización requerida:* Se deben incluir las nuevas entidades de dominio detectadas en el desarrollo: `EmpresaCliente`, `IpcRecord`, `Moneda`, `PartidaReexpresion` y `PeriodoFiscal`.
    *   *Estructura MVVM:* Si el diagrama inicial no lo contemplaba, se deben añadir las relaciones entre las Vistas (ej. `Bancos.xaml`, `LibroDiario.xaml`) y sus respectivos ViewModels (ej. `BancosViewModel`, `LibroDiarioViewModel`), las cuales heredan de `ViewModelBase`.
*   **Casos de Uso:** 
    *   *Actualización requerida:* Añadir o refinar el caso de uso de "Configurar Reexpresión" o "Ajuste por Inflación", ya que se ha evidenciado como un módulo completo (`Reexpresion.xaml` y `ReexpresionViewModel`).
*   **Diagrama de Secuencia:**
    *   *Actualización requerida:* Adaptar el flujo de guardado de un "Asiento Contable". El actor interactúa con la Vista, esta envía un `RelayCommand` al ViewModel, el ViewModel valida los datos usando los Modelos (`Asiento` y `AsientoLinea`), y luego invoca al Servicio de base de datos (Infrastructure).
*   **Diagrama de Colaboración:**
    *   *Actualización requerida:* Reflejar la misma comunicación mencionada en el diagrama de secuencia, destacando cómo el `DashboardViewModel` consolida información de múltiples servicios para mostrarla en la pantalla principal.

## 4. Enlace del Video (Presentación del Sistema)

*(Nota para el estudiante: Graba un video de 3 a 5 minutos navegando por las pantallas principales que ya tengan interfaz en WPF, como el Dashboard, Plan de Cuentas, Libro Diario o Reexpresión. Sube el video a YouTube (Oculto) o Google Drive y pega el enlace a continuación).*

**Enlace del video:** `[PEGAR_ENLACE_DEL_VIDEO_AQUI]`

> **Aprobación del Requisito:** En el video se evidencia al menos el 30% de las funcionalidades planteadas (ej. Navegación, carga de interfaz, módulos de Plan de Cuentas, Libro Diario y Reexpresión, demostrando el uso del patrón MVVM).
