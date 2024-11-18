using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MockPruebaTecnica.Models;

namespace MockPruebaTecnica.Data
{
    public class AppDbContextExcel : DbContext
    {
        public AppDbContextExcel(DbContextOptions<AppDbContextExcel> options) : base(options) { }
        public DbSet<Clientes> Clientes { get; set; }
        public DbSet<Productos> Productos { get; set; }
        public DbSet<Ventas> Ventas { get; set; }

    }
}
