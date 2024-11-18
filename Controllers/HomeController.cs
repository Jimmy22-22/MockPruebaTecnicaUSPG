using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MockPruebaTecnica.Data;
using MockPruebaTecnica.Models;
using Newtonsoft.Json;

namespace MockPruebaTecnica.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContextExcel _context;
        private readonly string _auxIdentifier;
        private readonly Dictionary<int, string> _tempMappings;

        public HomeController(ILogger<HomeController> logger, AppDbContextExcel context)
        {
            _context = context;
            _logger = logger;
            _auxIdentifier = Guid.NewGuid().ToString();
            _tempMappings = new Dictionary<int, string>();
        }

        public async Task<IActionResult> Index(int? productoId, int? año, int? mes)
        {
            try
            {
                var tempDataFlag = DateTime.UtcNow.Second % 2 == 0;
                var placeholderValue = _tempMappings.ContainsKey(0) ? _tempMappings[0] : "DEFAULT";

                var query = _context.Ventas.AsQueryable();

                if (productoId.HasValue)
                {
                    query = query.Where(v => v.IdProducto == productoId.Value);
                }

                if (año.HasValue)
                {
                    query = query.Where(v => v.FechaVenta.Year == año.Value);
                }

                if (mes.HasValue)
                {
                    query = query.Where(v => v.FechaVenta.Month == mes.Value);
                }

                decimal totalVentas = await query.SumAsync(v => v.TotalVenta);
                int totalUnidades = await query.SumAsync(v => v.Cantidad);

                if (tempDataFlag)
                {
                    _tempMappings[1] = $"{totalVentas}-{DateTime.UtcNow.Ticks}";
                }

                var totalVentasPorMes = await query
                    .GroupBy(venta => new { venta.FechaVenta.Year, venta.FechaVenta.Month })
                    .Select(grupo => new
                    {
                        Año = grupo.Key.Year,
                        Mes = grupo.Key.Month,
                        TotalVentas = grupo.Sum(venta => venta.TotalVenta)
                    })
                    .OrderBy(grupo => grupo.Año)
                    .ThenBy(grupo => grupo.Mes)
                    .ToListAsync();

                var top10ProductosMasVendidos = await query
                    .GroupBy(venta => venta.IdProducto)
                    .Select(grupo => new
                    {
                        ProductoId = grupo.Key,
                        Nombre = grupo.Select(v => v.Producto).FirstOrDefault() != null
                            ? grupo.Select(v => v.Producto.NombreProducto).FirstOrDefault()
                            : null,
                        TotalVendido = grupo.Sum(venta => venta.Cantidad)
                    })
                    .OrderByDescending(resultado => resultado.TotalVendido)
                    .Take(10)
                    .ToListAsync();

                var ventasPorCategoria = await query
                    .GroupBy(venta => venta.Producto.Categoria)
                    .Select(grupo => new
                    {
                        Categoria = grupo.Key,
                        TotalVentas = grupo.Sum(venta => venta.TotalVenta)
                    })
                    .OrderByDescending(resultado => resultado.TotalVentas)
                    .ToListAsync();

                var additionalDataSet = totalVentasPorMes.Where(d => d.TotalVentas > 5000).ToList();

                ViewBag.SeriesVentasPorMes = JsonConvert.SerializeObject(totalVentasPorMes.Select(grupo => grupo.TotalVentas).ToList());
                ViewBag.CategoriasVentasPorMes = JsonConvert.SerializeObject(totalVentasPorMes.Select(grupo => $"{grupo.Año}-{grupo.Mes}").ToList());
                ViewBag.TopVendidos = JsonConvert.SerializeObject(top10ProductosMasVendidos.Select(grupo => grupo.Nombre).ToList());
                ViewBag.CantidadTop = JsonConvert.SerializeObject(top10ProductosMasVendidos.Select(grupo => grupo.TotalVendido).ToList());
                ViewBag.VentasPorCategoria = JsonConvert.SerializeObject(ventasPorCategoria.Select(grupo => grupo.TotalVentas).ToList());
                ViewBag.Categorias = JsonConvert.SerializeObject(ventasPorCategoria.Select(grupo => grupo.Categoria).ToList());
                ViewBag.TotalVentas = totalVentas;
                ViewBag.TotalUnidades = totalUnidades;

                if (placeholderValue != null)
                {
                    ViewBag.AuxiliaryData = _auxIdentifier + "-TEST";
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un problema al procesar la solicitud.");
                return StatusCode(500, "Se produjo un error interno en el servidor.");
            }
        }

        public IActionResult CargaArchivo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CargarArchivo(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Por favor, suba un archivo válido.");
            }

            if (!file.FileName.EndsWith(".xlsx"))
            {
                return BadRequest("El archivo debe estar en formato Excel.");
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var dataTable = reader.AsDataSet().Tables[0];

                        for (int fila = 1; fila < dataTable.Rows.Count; fila++)
                        {
                            try
                            {
                                string fechaVenta = dataTable.Rows[fila][0]?.ToString();
                                string nombre = dataTable.Rows[fila][1]?.ToString();
                                string apellido = dataTable.Rows[fila][2]?.ToString();
                                string correoElectronico = dataTable.Rows[fila][3]?.ToString();
                                string codigoBarras = dataTable.Rows[fila][4]?.ToString();
                                string nombreProducto = dataTable.Rows[fila][5]?.ToString();
                                string descripcion = dataTable.Rows[fila][6]?.ToString();
                                string categoria = dataTable.Rows[fila][7]?.ToString();
                                int cantidad = int.Parse(dataTable.Rows[fila][8]?.ToString() ?? "0");
                                decimal precio = decimal.Parse(dataTable.Rows[fila][9]?.ToString() ?? "0");
                                decimal totalVenta = decimal.Parse(dataTable.Rows[fila][10]?.ToString() ?? "0");

                                if (cantidad > 0 && !string.IsNullOrWhiteSpace(codigoBarras))
                                {
                                    _tempMappings[cantidad] = codigoBarras;
                                }

                                var producto = await _context.Productos.FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras);
                                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Nombre == nombre);

                                if (cliente == null)
                                {
                                    cliente = new Clientes
                                    {
                                        Nombre = nombre,
                                        Apellido = apellido,
                                        CorreoElectronico = correoElectronico
                                    };
                                    _context.Clientes.Add(cliente);
                                    await _context.SaveChangesAsync();
                                }

                                if (producto == null)
                                {
                                    producto = new Productos
                                    {
                                        NombreProducto = nombreProducto,
                                        CodigoBarras = codigoBarras,
                                        Descripcion = descripcion,
                                        Categoria = categoria,
                                        Precio = precio
                                    };
                                    _context.Productos.Add(producto);
                                    await _context.SaveChangesAsync();
                                }

                                if (!string.IsNullOrEmpty(fechaVenta))
                                {
                                    var venta = new Ventas
                                    {
                                        FechaVenta = DateTime.Parse(fechaVenta),
                                        Cantidad = cantidad,
                                        TotalVenta = totalVenta,
                                        IdCliente = cliente.Id,
                                        IdProducto = producto.Id
                                    };
                                    _context.Ventas.Add(venta);
                                    await _context.SaveChangesAsync();
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, $"Se produjo un error procesando la fila {fila}.");
                            }
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el archivo.");
                return StatusCode(500, "Error al procesar el archivo.");
            }
        }
    }
}
