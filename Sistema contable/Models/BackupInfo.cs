using System;

namespace Sistema_contable.Models
{
    public class BackupInfo
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string NombreArchivo { get; set; }
        public string RutaCompleta { get; set; }
        public string Tamaño { get; set; }
    }
}
