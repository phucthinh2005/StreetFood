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
            var lang = SettingsService.Language;// "en", "vi", ...

            var culture = new CultureInfo(lang);// tạo CultureInfo từ mã ngôn ngữ

            CultureInfo.DefaultThreadCurrentCulture = culture;// đặt culture cho thread hiện tại
            CultureInfo.DefaultThreadCurrentUICulture = culture;// đặt culture cho resource

            AppResources.Culture = culture;
        }

        protected override Window CreateWindow(IActivationState? activationState)// tạo cửa sổ chính của ứng dụng
        {
            return new Window(new AppShell());
        }

        //protected override void OnSleep() // 
        //{
        //    base.OnSleep();

        //    // dừng TTS khi nhấn HOME hoặc chuyển app khác
        //    AudioService.Instance.Stop();
        //}
    }
}