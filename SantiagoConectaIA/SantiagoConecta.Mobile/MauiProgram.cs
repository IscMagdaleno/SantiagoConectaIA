using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using SantiagoConecta.SharedUI.Data;

namespace SantiagoConecta.Mobile;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

#if DEBUG
#if ANDROID
        var apiBaseAddress = "https://10.0.2.2:7196"; // Local para emulador Android
#else
        var apiBaseAddress = "https://localhost:7196"; // Local para iOS/Windows
#endif
#else
        // Modo Release (producción/staging)
        var apiBaseAddress = "https://santiagoconecta-api-atejc4e2hjane8dq.centralus-01.azurewebsites.net";
#endif

#if ANDROID && DEBUG
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
        builder.Services.AddScoped(sp => new HttpClient(handler) { BaseAddress = new Uri(apiBaseAddress), Timeout = TimeSpan.FromMinutes(10) });
#else
		builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress), Timeout = TimeSpan.FromMinutes(10) });
#endif
		builder.Services.AddScoped<Data_Tramites>();
		builder.Services.AddScoped<Data_Noticias>();
		builder.Services.AddMudServices();
		builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

		return builder.Build();
	}
}
