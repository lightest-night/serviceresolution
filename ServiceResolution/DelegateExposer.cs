using Microsoft.Extensions.DependencyInjection;

namespace LightestNight.ServiceResolution
{
    public abstract class DelegateExposer
    {
        public abstract IServiceCollection ExposeDelegates(IServiceCollection services);
    }
}