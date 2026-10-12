using System;
using Microsoft.Extensions.DependencyInjection;

namespace DotNative.Toast;

public static class ToastServiceProviderExtensions
{
#if NET10_0_OR_GREATER
    extension(IServiceProvider services)
    {
        /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
        public IToast Toast => services.GetRequiredService<IToast>();
    }
#else
    /// <summary>Resolves the registered plugin using the provider's DI lifetime.</summary>
    public static IToast Toast(this IServiceProvider services) =>
        services.GetRequiredService<IToast>();
#endif
}
