using System.Globalization;
using MauiApp1.Resources.Languages;
using MauiApp1.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MauiApp1
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            // ===== Load language từ Settings =====
            var lang = SettingsService.Language;

            var culture = new CultureInfo(lang);

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            AppResources.Culture = culture;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}