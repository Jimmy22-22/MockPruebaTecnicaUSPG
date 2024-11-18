using System.ComponentModel.DataAnnotations.Schema;

namespace MockPruebaTecnica.Models
{
    public class Ventas
    {
        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Column("id_venta")]
        public int Id { get; set; }

        [Column("id_producto")]
        public int IdProducto { get; set; }

        [ForeignKey("IdProducto")]
        public Productos Producto { get; set; }

        [Column("fecha_venta")]
        public DateTime FechaVenta { get; set; }

        [ForeignKey("IdCliente")]
        public Clientes Cliente { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("total_venta")]
        public decimal TotalVenta { get; set; } 
    }
}
