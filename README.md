# DotNative.Toast

Shows a short native message over the current window.

```csharp
builder.Services.AddToast();
await services.Toast.ShowAsync("Lozinka je sačuvana");
```

| Platform | Status |
| --- | --- |
| Android | `android.widget.Toast`, centered |
| iOS | `UIView` over the key window |
| macOS | `NSView` over the presenter |
| Windows | DotNative banner above the application |
| Linux | DotNative banner above the application |

The call completes when the message is on screen. It disappears after about
two seconds. On iOS, macOS, Windows and Linux a tap dismisses it sooner.

Windows and Linux draw a `ToastBanner` through `IOverlayPresenter`, so the
message is a DotNative component above the application tree. Android, iOS and
macOS keep the native window toast.

## Service access

Import `DotNative.Toast` to access the plugin through `IServiceProvider`:

```csharp
using DotNative.Toast;

var plugin = services.Toast;
```

The getter calls `GetRequiredService<IToast>()` on every access, preserving
DI lifetimes and the usual missing-registration error. Register the plugin with
`AddToast()` before building the provider.

A `net10.0` application uses the property syntax with C# 14 or later. A
`net9.0` application uses only the method equivalent:

```csharp
var plugin = services.Toast();
```

The package contains separate `net9.0` and `net10.0` assemblies. NuGet selects
the assembly matching the application target framework. `NET10_0_OR_GREATER`
selects the property; the `#else` branch selects the method.

Build and pack both targets with .NET 10 SDK. A source build using .NET 9 SDK
builds only `net9.0`; it does not produce the .NET 10 assembly.
