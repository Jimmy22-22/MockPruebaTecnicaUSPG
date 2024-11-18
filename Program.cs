using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MockPruebaTecnica.Data;
using System;
using System.Text;

namespace MockPruebaTecnica
{
	public class Program
	{
		public static void Main(string[] args)
		{
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContextExcel>(options =>
                options.UseSqlServer(connectionString));


            // Add services to the container.
            builder.Services.AddControllersWithViews();

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}
	}
}
