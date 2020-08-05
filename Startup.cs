using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Shop.Data;

namespace Shop
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			// inicializa as configurações
			Configuration = configuration;
		}

		// tem acesso as configurações no appsettings
		public IConfiguration Configuration { get; }

		// quais serviços do aspnet core ire usar
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCors();
			// comprimir o Json antes de mandar pra tela
			services.AddResponseCompression(options =>
			{
				options.Providers.Add<GzipCompressionProvider>();
				// comprimir tudo para application/json
				options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
			});

			//services.AddResponseCaching();

			services.AddControllers();

			// gerar chave simetrica, formato de bytes
			var key = Encoding.ASCII.GetBytes(Settings.Secret);

			services.AddAuthentication(x =>
			{
				x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(x =>
			{
				x.RequireHttpsMetadata = false;
				x.SaveToken = true;
				x.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					// se eu tivesse  usando identity server
					ValidateIssuer = false,
					ValidateAudience = false
				};
			});

			// services.AddDbContext<DataContext>(opt => opt.UseInMemoryDatabase("Database"));
			services.AddDbContext<DataContext>(opt => opt.UseSqlServer(Configuration.GetConnectionString("connectionString")));
			
			// O AddDbContext já faz a função o AddScoped
			// services.AddScoped<DataContext, DataContext>();

			services.AddSwaggerGen(c =>
						{
							c.SwaggerDoc("v1", new OpenApiInfo { Title = "Shop Api", Version = "v1" });
						});
		}

		// como ou quais desses serviços iremos utilizar
		public void Configure(
			// tudo o que quiser da minha aplicação
			IApplicationBuilder app,
			// tudo que eu quiser sobre o ambiente de desenvolvimento
			IWebHostEnvironment env)
		{
			// if (env.IsDevelopment())
			// {
				// detalher do erro
				app.UseDeveloperExceptionPage();
			//}

			// forçar a api responde somente a https
			app.UseHttpsRedirection();

			// Enable middleware to serve generated Swagger as a JSON endpoint.
			app.UseSwagger();

			// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
			// specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shop API V1");
			});

			// usar padrões de rotas do mvc
			app.UseRouting();

			// antes do authentication e authorization
			// politica global de cors
			app.UseCors(x => x
				.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader()
				);

			// autorização e autenticação com roles
			app.UseAuthentication();
			app.UseAuthorization();


			// mapeamento dos endpoints
			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
