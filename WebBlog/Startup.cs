using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using WebBlog.Models;

namespace WebBlog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure DbContext to use SQL Server
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole<int>>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false; // Change as needed
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Define custom policies for admin and authenticated user
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("AuthenticatedUser", policy => policy.RequireRole("AuthenticatedUser"));
            });

            services.AddSession(options =>
            {
       
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true; 
            });

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.LoginPath = "/Account/Login";
                    options.AccessDeniedPath = "/Home/AccessDenied";
                    options.SlidingExpiration = true;
                });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RoleManager<IdentityRole<int>> roleManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();

            app.UseAuthentication(); // Add authentication middleware
            app.UseAuthorization();

            // Create roles during application startup
            CreateRoles(roleManager).Wait();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");


                endpoints.MapControllerRoute(
                    name: "editPost",
                    pattern: "Admin/EditPost/{id}",
                    defaults: new { controller = "Admin", action = "EditPost" }
                );
            });
        }

        // Method to create roles
        private async Task CreateRoles(RoleManager<IdentityRole<int>> roleManager)
        {
            // Ensure that "Admin" role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                var adminRole = new IdentityRole<int>("Admin");
                await roleManager.CreateAsync(adminRole);
            }

            // Ensure that "AuthenticatedUser" role exists
            if (!await roleManager.RoleExistsAsync("AuthenticatedUser"))
            {
                var authenticatedUserRole = new IdentityRole<int>("AuthenticatedUser");
                await roleManager.CreateAsync(authenticatedUserRole);
            }
        }
    }
}
