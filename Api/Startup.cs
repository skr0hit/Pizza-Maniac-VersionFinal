using Api.Api.Extentions;
using Api.Api.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Api.Api
{
    public class Startup
    {

        private readonly IConfiguration _config;
        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Add Application Services (Extention method)
            services.AddApplicationServices(_config);

            //Register the Swagger Generator
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("PizzaManiacSpec",
                    new Microsoft.OpenApi.Models.OpenApiInfo()
                    {
                        Title = "PizzaManiac API",
                        Version = "Final",
                        Description = "API for PizzaManiac App.",
                        Contact = new Microsoft.OpenApi.Models.OpenApiContact()
                        {
                            Email = "Skr434546@gmail.com",
                            Name = "Sk Rohit",
                            Url = new Uri("https://github.com/itsRavaan")
                        }
                        
                    });

               

            });

            services.AddControllers();

            services.AddCors();

            //Add Identity services (Extention method)
            services.AddIdentityServices(_config);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            //Add our custom Error Middleware
            app.UseMiddleware<ExceptionMiddleware>();

            app.UseHttpsRedirection();

            //Adding Swashbuckle to the request pipeline
            app.UseSwagger();

            //Adding the Swagger UI for user friendlyness
            app.UseSwaggerUI(options =>
            {
                //Set endpoint where Swagger UI will be hosted
                options.SwaggerEndpoint("/swagger/PizzaManiacSpec/swagger.json", "Pizza API");

                //Add custom stylesheet to Swagger UI
                options.InjectStylesheet("/swagger-custom/swagger-custom-styles.css");

                //Inject favicons and Logos fot Swagger UI
                options.InjectJavascript("/swagger-custom/swagger-custom-script.js", "text/javascript");

                //Set as default page
                options.RoutePrefix = "";
            });

            //Used for serving static files so that we can add custom scripts and CSS files to Swagger
            app.UseStaticFiles();

            app.UseRouting();

            //Add Cors
            app.UseCors(x =>
                x.AllowAnyHeader().AllowAnyMethod()
                .WithOrigins("https://localhost:4200"));

            //Add Authentication
            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
