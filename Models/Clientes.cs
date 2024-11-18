using System.ComponentModel.DataAnnotations.Schema;

namespace MockPruebaTecnica.Models
{
    public class Clientes
    {
        [Column("apellido")]
        public string Apellido { get; set; }

        [Column("correo_electronico")]
        public string CorreoElectronico { get; set; }

        [Column("id_cliente")]
        public int Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; }

        public ICollection<Ventas> Ventas { get; set; }
    }
}
