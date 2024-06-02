using JMS.DVB.NET.Recording;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

namespace JMS.VCR.NET;

public class Startup(IConfiguration configuration)
{
    private readonly IConfiguration Configuration = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        ConfigureSwagger(services);

        services.AddCors(options =>
            options.AddPolicy("AllowAny", builder =>
                builder
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed((host) => true)
                    .AllowCredentials()
                ));

        services.UseRecording(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI(config => config.SwaggerEndpoint("/swagger/API/swagger.json", "VCR.NET"));

        app.UseRouting();
        app.UseCors("AllowAny");
        app.UseAuthorization();

        app.UseEndpoints(endpoints => endpoints.MapControllers());

        var clientPath = Configuration.GetSection("ClientPath").Get<List<string>>()!;

        clientPath.Insert(0, Path.GetDirectoryName(typeof(Startup).Assembly.Location)!);

        app.UseStaticFiles(new StaticFileOptions() { FileProvider = new PhysicalFileProvider(Path.Combine([.. clientPath])) });
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(config =>
            config.SwaggerDoc("API", new OpenApiInfo
            {
                Description = "VCR.NET Recording Server",
                Title = "VCR.NET",
                Version = "5.0"
            })
        );
    }
}
