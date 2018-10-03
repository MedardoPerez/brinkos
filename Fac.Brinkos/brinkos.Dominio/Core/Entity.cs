using System;

namespace brinkos.Dominio.Core
{
    public abstract class Entity
    {
        public string ModificadoPor { get; set; }
        public DateTime FechaTransaccion { get; set; }
        public string DescripcionTransaccion { get; set; }
        public byte[] RowVersion { get; set; }
        public Guid TransaccionUId { get; set; }
        public string TipoTransaccion { get; set; }
    }
}