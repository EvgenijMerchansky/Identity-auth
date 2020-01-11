using AutoMapper;
using Identity_auth.Configurations;
using Identity_auth.Configurations.MapperProfiles;
using Identity_auth.Data;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Identity_auth.Extensions
{
    public static class ConfigurationExtension
    {
        public static void RegisterDbContext(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            serviceCollection.AddDbContext<AppDbContext>(config =>
            {
                config.UseSqlServer(connectionString);
            });
        }

        public static void RegisterIdentity(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddIdentity<IdentityUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false;
                config.Password.RequireNonAlphanumeric = false;
                config.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<Data.AppDbContext>()
                .AddDefaultTokenProviders();
        }

        public static void RegisterIdentityServer(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection");

            string assembly = typeof(Startup).Assembly.GetName().Name;

            serviceCollection.AddIdentityServer()
                .AddAspNetIdentity<IdentityUser>()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(assembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = b => b.UseSqlServer(connectionString,
                        sql => sql.MigrationsAssembly(assembly));

                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30;
                })
                .AddDeveloperSigningCredential();
        }

        public static void RegisterMappers(this IServiceCollection serviceCollection)
        {
            MapperConfiguration mapperConfiguration = new MapperConfiguration(c =>
            {
                c.AddProfile<AccountControllerProfile>();
            });

            serviceCollection.AddSingleton(s => mapperConfiguration.CreateMapper());
        }

        public static void RegisterAuthentication(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options => {
                    options.Authority = "https://localhost:44352";
                    options.ApiName = "ApiOne";
                    options.SaveToken = true;
                });
        }

        public static void RegisterCors(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());
            });
        }

        public static void RegisterCookie(this IServiceCollection serviceCollection)
        {
            serviceCollection.ConfigureApplicationCookie(config =>
            {
                config.Cookie.Name = "IdentityServer.Cookie";
                config.LoginPath = "/account/login";
            });
        }

        public static void RegisterIdentityServerValuesConfigurations(
            this IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            IdentityServerValuesConfiguration identityServerValuesConfiguration = new IdentityServerValuesConfiguration();
            configuration.Bind("IdentityServerValuesConfiguration", identityServerValuesConfiguration);
            serviceCollection.AddSingleton(identityServerValuesConfiguration);
        }
    }
}