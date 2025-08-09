using FaceId.Services;
using Microsoft.Extensions.Logging;
using Plugin.Maui.Biometric;

namespace FaceId;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
           // .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        });
        builder.Services.AddMauiBlazorWebView();

        // Use with Dependency Injection
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<IBiometric>(BiometricAuthenticationService.Default);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}