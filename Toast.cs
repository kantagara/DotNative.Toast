using DotNative;
using DotNative.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNative.Toast;

// ─────────────────────────────────────────────────────────────
// Public API
// ─────────────────────────────────────────────────────────────

public interface IToast
{
    Task ShowAsync(
        string message,
        CancellationToken cancellationToken = default);
}

// ─────────────────────────────────────────────────────────────
// Dependency Injection
// ─────────────────────────────────────────────────────────────

public static class ToastServices
{
    public static IServiceCollection AddToast(
        this IServiceCollection services)
    {
        services.TryAddSingleton<IToast, ToastService>();
        return services;
    }
}

// ─────────────────────────────────────────────────────────────
// Implementation
// ─────────────────────────────────────────────────────────────

internal sealed class ToastService(
    IPlatformChannels channels,
    PresentationTarget target,
    IOverlayPresenter overlays
) : IToast
{
    private const string ChannelName = "dotnative.toast";

    private bool IsDesktop =>
        target.Platform is NativePlatform.Windows or NativePlatform.Linux;

    public Task ShowAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        if (message.Length > 2_000)
            throw new ArgumentOutOfRangeException(nameof(message));

        return IsDesktop
            ? new DesktopToast(overlays).ShowAsync(message, cancellationToken)
            : channels.Get(ChannelName).InvokeAsync(
                "show",
                new Dictionary<string, object?> { ["message"] = message },
                cancellationToken);
    }
}

internal sealed class DesktopToast(IOverlayPresenter overlays)
{
    public async Task ShowAsync(
        string message,
        CancellationToken cancellationToken)
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
