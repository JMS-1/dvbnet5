
using Microsoft.Extensions.DependencyInjection;

namespace JMS.DVB.NET.Recording;

public class Lazy<T>(IServiceProvider services) where T : notnull
{
    public T Service => services.GetRequiredService<T>();
}
