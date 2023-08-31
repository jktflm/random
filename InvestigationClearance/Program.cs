using InvestigationClearance.Data;
using Microsoft.EntityFrameworkCore;

namespace InvestigationClearance
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
                           options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnectionString")));

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
                pattern: "{controller=Clearance}/{action=Index}/{id?}");
            app.MapControllerRoute(
                name: "edit",
                pattern: "Clearance/Edit/{id:int}",
                defaults: new { controller = "Clearance", action = "Edit" }
            );

            app.MapControllerRoute(
                name: "delete",
                pattern: "Clearance/Delete/{id:int}",
                defaults: new { controller = "Clearance", action = "Delete" }
            );
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                name: "error",
                pattern: "Clearance/Error",
                defaults: new
                {
                    controller = "Clerance",
                    action = "Error"
                });

                endpoints.MapFallbackToController("Error", "Clearance");
                app.Run();
            });
        }
    }
}