using Microsoft.Extensions.DependencyInjection;

namespace LightestNight.System.ServiceResolution
{
    public abstract class DelegateExposer
    {
        public abstract IServiceCollection ExposeDelegates(IServiceCollection services);
    }
}