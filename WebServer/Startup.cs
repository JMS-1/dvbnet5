using Microsoft.AspNetCore.Mvc;
using JMS.DVB.NET.Recording;

namespace JMS.VCR.NET;

[ApiController]
[Route("api/it")]
public class TheController : ControllerBase
{
    [HttpGet]
    public string Info() => "Running";
}

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

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

        services.UseRecording();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();
        app.UseCors("AllowAny");
        app.UseAuthorization();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }

    private static void ConfigureSwagger(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}
