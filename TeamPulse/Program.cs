using Microsoft.EntityFrameworkCore;
using TeamPulse.Models;

namespace TeamPulse
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // ==========================================
            // 1. ADD SESSION SERVICE (Login yaad rakhne ke liye)
            // ==========================================
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 minute baad logout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddDbContext<TeamPulseDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // ==========================================
            // 2. STATIC FILES (AdminLTE CSS/JS ke liye zaroori)
            // ==========================================
            app.UseStaticFiles();

            app.UseRouting();

            // ==========================================
            // 3. ENABLE SESSION (Routing ke baad lagana zaroori hai)
            // ==========================================
            app.UseSession();

            app.UseAuthorization();

            // ==========================================
            // 4. UPDATE DEFAULT ROUTE (Start page = Login)
            // ==========================================
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}"); // Pehle Login Page khulega

            app.Run();
        }
    }
}