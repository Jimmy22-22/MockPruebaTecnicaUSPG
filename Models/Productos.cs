using System.ComponentModel.DataAnnotations.Schema;

namespace MockPruebaTecnica.Models
{
    public class Productos
    {
        [Column("descripcion")]
        public string Descripcion { get; set; }

        [Column("categoria")]
        public string Categoria { get; set; }

        [Column("codigo_barras")]
        public string CodigoBarras { get; set; }

        [Column("id_producto")]
        public int Id { get; set; }

        [Column("nombre_producto")]
        public string NombreProducto { get; set; }

        [Column("precio")]
        public decimal Precio { get; set; } 

        public ICollection<Ventas> Ventas { get; set; }
    }
}
