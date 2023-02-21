using System;
using System.Reflection;
using System.IO;
using InventoryWebApplication.Attributes;
using InventoryWebApplication.DatabaseContexts;
using InventoryWebApplication.Models.Database;
using InventoryWebApplication.Services.Database;
using InventoryWebApplication.Services.Exporter;
using InventoryWebApplication.Services.Importer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace InventoryWebApplication
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
            services.AddCors();
            services.AddControllers();

            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(o =>
            {
                o.Cookie.Name = "CookieAuth";
                o.LoginPath = "/login";
                o.ExpireTimeSpan = TimeSpan.FromHours(24);
            });

            services.AddDbContext<DatabaseContext>(o =>
                o.UseSqlite(Configuration["SQLiteConnection:SQLiteConnectionString"]));


            services.AddTransient<DatabaseService<User>, UsersService>();
            services.AddTransient<DatabaseService<SaleInfo>, SalesService>();
            services.AddTransient<DatabaseService<Product>, ProductsService>();
            services.AddTransient<DatabaseService<PaymentMethod>, PaymentMethodsService>();

            services.AddTransient<UsersService>();
            services.AddTransient<SalesService>();
            services.AddTransient<ProductsService>();
            services.AddTransient<PaymentMethodsService>();
            
            services.AddTransient<ExporterFactory>();
            services.AddTransient<ImporterFactory>();

            services.AddMvc(o => { o.Filters.Add(new AutoLoggingAttribute()); });

            services.AddRazorPages();
            services.AddControllersWithViews();
      //      services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
           /* app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.)
           /* app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });*/
            app.UseRouting();

            app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                // endpoints.MapControllerRoute("auth", "{controller=Authentication}");

                // endpoints.MapControllerRoute(
                //     "login",
                //     "auth/{controller=Login}/{action=Login}/{id?}");
            });
        }
    }
}