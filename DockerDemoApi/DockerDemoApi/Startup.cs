using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;

using System;
using System.IO.Compression;
using System.Linq;

namespace DockerDemoApi
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public static void ConfigureServices(IServiceCollection services)
		{
			// This is required to enable controller parameter attributes
			services.AddControllers().AddNewtonsoftJson();

			ConfigureCors(services);
			//ConfigureJwt(services);

			// Add services
			//services.AddScoped<IAuthService, AuthService>();

			ConfigureResponseEncoding(services);
			//ConfigureLogging(services);
			//ConfigurePolicies(services);
			ConfigureSwagger(services);
			ConfigureVersioning(services);
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider apiVersionDescriptionProvider)
		{
			if (env.IsDevelopment() || env.IsStaging())
			{
				app.UseDeveloperExceptionPage();
				app.UseCors("AllowAll");

				// Configure OpenApi
				app.UseOpenApi(options =>
				{
					options.PostProcess = (document, x) =>
					{
						document.Schemes = new[] { OpenApiSchema.Https };
						document.Host = Environment.GetEnvironmentVariable("DOMAIN") ?? "";
					};
				});

				// Configure Swagger
				app.UseSwaggerUi3(options =>
				{
					options.Path = "/swagger";
					foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
					{
						string version = $"v{description.ApiVersion.MajorVersion}";
						options.SwaggerRoutes.Add(new SwaggerUi3Route($"{version}", $"/swagger/{version}/swagger.json"));
					}
				});
			}
			else
			{
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
				app.UseCors("AllowRootBeerProduction");
			}

			app.UseResponseCompression();
			app.UseHttpsRedirection();
			app.UseRouting();

			// NOTE: The order of these two calls is important!
			//app.UseAuthentication();
			//app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}

		private static void ConfigureResponseEncoding(IServiceCollection services)
		{
			services.AddResponseCompression(options =>
			{
				options.EnableForHttps = true;
				options.Providers.Add<BrotliCompressionProvider>();  // Brotli will be chosen first based upon order here
				options.Providers.Add<GzipCompressionProvider>();
			});

			services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });
			services.Configure<BrotliCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });
		}

		private static void ConfigureSwagger(IServiceCollection services)
		{
			services.AddSwaggerDocument(document =>
			{
				document.Version = "v1";
				document.DocumentName = "v1";
				document.Title = "DockerDemoApi";
				document.Description = "Change the dropdown in the upper right to select different versions.";
				document.DocumentProcessors.Add(new SecurityDefinitionAppender(
					"custom-auth", Enumerable.Empty<string>(),
					new OpenApiSecurityScheme
					{
						Type = OpenApiSecuritySchemeType.ApiKey,
						Name = "Authorization",
						Description = "Copy 'Bearer ' + token into field",
						In = OpenApiSecurityApiKeyLocation.Header
					}));
				document.ApiGroupNames = new string[] { "v1" };
			});
		}

		private static void ConfigureVersioning(IServiceCollection services)
		{
			services.AddApiVersioning(options =>
			{
				options.DefaultApiVersion = new ApiVersion(1, 0);
				options.AssumeDefaultVersionWhenUnspecified = true;
				options.ReportApiVersions = true;
			});

			// Changed in v3.x https://github.com/dotnet/aspnet-api-versioning/issues/330
			services.AddVersionedApiExplorer(options =>
			{
				options.GroupNameFormat = "'v'VVV";
				options.SubstituteApiVersionInUrl = true;
			});
		}

		private static void ConfigureCors(IServiceCollection services)
		{
			// Cross Domain configuration
			services.AddCors(options =>
			{
				options.AddPolicy("AllowAll",
					builder =>
					{
						// The token fingerprint and refresh token are passed as an http only cookie, the consuming client requests must
						// be initiated with the "withCredentials" property in order to pass the http only cookie back, but the
						// back end will only accept that cookie if CORS has been set up with specific origins.
						// "AllowAnyOrigin" blocks "AllowCredentials". Using a "SetIsOriginAllowed" hack for dev/staging environments.
						// https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS/Errors/CORSNotSupportingCredentials
						builder
							.SetIsOriginAllowed(origin => true)
							.AllowAnyMethod()
							.AllowAnyHeader()
							.AllowCredentials();
					});
				options.AddPolicy("AllowRootBeerProduction",
					builder =>
					{
						builder.WithOrigins(
							"https://rootbeer.everythingdisc.com",      // TODO: Remove when no longer used
							"https://catalyst.everythingdisc.com",
							"https://www.catalyst.everythingdisc.com")  // In case "www" prefix is ever used
						.AllowAnyMethod()
						.AllowAnyHeader()
						.AllowCredentials();
					});
			});
		}
	}
}
