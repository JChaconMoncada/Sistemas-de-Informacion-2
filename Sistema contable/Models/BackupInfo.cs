using System;

namespace Sistema_contable.Models
{
    public class BackupInfo
    {
        public DateTime Fecha { get; set; }
        public string NombreArchivo { get; set; }
        public string RutaCompleta { get; set; }
        public string Tamaño { get; set; }
    }
}
