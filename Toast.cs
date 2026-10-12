using DotNative;
using DotNative.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNative.Toast;

public interface IToast
{
    Task ShowAsync(string message, CancellationToken cancellationToken = default);
}

public static class ToastServices
{
    public static IServiceCollection AddToast(this IServiceCollection services)
    {
        services.TryAddSingleton<IToast, RoutingToast>();
        return services;
    }
}

internal sealed class RoutingToast(
    IPlatformChannels channels,
    PresentationTarget target,
    IOverlayPresenter overlays
) : IToast
{
    public Task ShowAsync(string message, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (message.Length > 2_000)
            throw new ArgumentOutOfRangeException(nameof(message));
        return target.Platform is NativePlatform.Windows or NativePlatform.Linux
            ? new DesktopToast(overlays).ShowAsync(message, cancellationToken)
            : new ChannelToast(channels).ShowAsync(message, cancellationToken);
    }
}

internal sealed class ChannelToast(IPlatformChannels channels)
{
    public Task ShowAsync(string message, CancellationToken cancellationToken) =>
        channels
            .Get("dotnative.toast")
            .InvokeAsync(
                "show",
                new Dictionary<string, object?> { ["message"] = message },
                cancellationToken
            );
}

internal sealed class DesktopToast(IOverlayPresenter overlays)
{
    public async Task ShowAsync(string message, CancellationToken cancellationToken)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeout.Token
        );
        try
        {
            await overlays.ShowAsync<bool>(
                dismiss => new ToastBanner(message, () => dismiss(true)),
                linked.Token
            );
        }
        catch (OperationCanceledException)
            when (timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        { }
    }
}

internal sealed class ToastBanner(string message, Action dismiss) : Component
{
    public override Element Build() =>
        new VStack(
            new Button(message, dismiss)
                .Padding(16, 10)
                .FontSize(16)
                .ForegroundColor(new Color(128, 128, 128))
                .BackgroundColor(new Color(246, 246, 246))
                .CornerRadius(10)
        )
            .MainAlignment(MainAxisAlignment.End)
            .CrossAlignment(CrossAxisAlignment.Center)
            .Padding(24, 24, 24, 48)
            .WidthPercent(100)
            .HeightPercent(100);
}
