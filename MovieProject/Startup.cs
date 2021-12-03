using Business.Abstract;
using Business.Concrete;
using Business.Models;
using DataAccess.Abstract;
using DataAccess.Concrete;
using DataAccess.Concrete.Context;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MovieProject.Caching;
using MovieProject.Extensions;
using NLog;
using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace MovieProject
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


            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = "localhost:6379";
            });


            //get all validater
            services.AddMvc()
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Startup>());


            services.AddDbContext<MovieStoreContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DevConnection")));

            //appsettings tan�mlama 1.y�ntem.
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);


            //appsetting tan�mlama 2. y�ntem.
            AppSettings appSettings = new AppSettings();
            Configuration.GetSection("AppSettings").Bind(appSettings);

            //token i�indeki secret kullanmak i�in 
            TokenSettings tokenSettings1 = new TokenSettings();
            Configuration.GetSection("TokenSettings").Bind(tokenSettings1);

            //Create singleton from instance
            services.AddSingleton<AppSettings>(appSettings);


            //for dependency injection
            //Bu yap�y� auto fact gibi bir yap�ya ta��yarak kullanmam�zda fayda vard�r. Bussiness katman� i�inde kullan�labilir.
            //Auto fact yap�s� aop'yi destekledi�inden dolay� bu yap�y� auto factte ta��n�r.
            services.AddTransient<IJwtAuthenticationService, JwtAuthenticationManager>();
            services.AddTransient<IAuthenticationService, AuthenticationManager>();
            services.AddTransient<IMovieService, MovieManager>();
            services.AddTransient<ICastService, CastManager>();
            services.AddTransient<IUserService, UserManager>();
            services.AddTransient<IMenuService, MenuManager>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IMenuRepository, MenuRepository>();
            services.AddTransient<ICacheService, CacheService>();




            //JWT token settings
            var tokenSettingsSection = Configuration.GetSection("TokenSettings");
            services.Configure<TokenSettings>(tokenSettingsSection);

            //JWT authentication settings
            var tokenSettings = tokenSettingsSection.Get<TokenSettings>();
            var key = Encoding.ASCII.GetBytes(tokenSettings.Secret);
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(x =>
                {
                    x.RequireHttpsMetadata = false;
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false
                    };
                });



            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowOrigin",
                    builder => builder.WithOrigins("http://localhost:4200"));
            });
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MovieProject", Version = "v1" });


                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

            });
            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration["CacheConnection"];
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {

            //update database ile zaten de�i�iklik oldu�unda kendisi de�i�ikli�i alg�l�yor ve her defas�nda update database yapm�yor.
            //Package managerda update-database gerek kalmaz.
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                try
                {
                    serviceScope.ServiceProvider.GetService<MovieStoreContext>().Database.Migrate();
                }
                catch (Exception ex)
                {

                    var logger = LogManager.GetCurrentClassLogger();
                    logger.Error("Veritaban� g�ncellenirken hata olu�tu.Detay:" + ex);
                }
            }

            if (env.IsDevelopment())
            {

                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieProject v1"));
            }

            app.UseHttpsRedirection();


            app.UseRouting();

            app.UseCors(x => x
            .AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(origin => true)
            .AllowCredentials());

            app.UseAuthentication(); //user login

            app.UseAuthorization();

            app.UseLogging();

            //app.UseMiddleware(typeof(ExceptionHandlingMiddleware));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
