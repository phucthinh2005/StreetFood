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

            // ===== Theme =====
            var theme = SettingsService.Theme;
            if (theme == "Dark")
                Current.UserAppTheme = AppTheme.Dark;
            else
                Current.UserAppTheme = AppTheme.Light;

            // ===== Language =====
            var lang = SettingsService.Language;
            var culture = new CultureInfo(lang);
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            AppResources.Culture = culture;

            // ===== Firebase / POI Repository =====
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "vinh_khanh.db3");
            PoiRepository.Init(dbPath);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }

        protected override void OnStart()
        {
            base.OnStart();
            _ = PoiRepository.Instance.InitializeAsync();

            // ── DEBUG TẠM THỜI — xóa sau khi fix ──
            _ = DebugFirebaseAsync();
        }

        private async Task DebugFirebaseAsync()
        {
            try
            {
                // Test 1: internet có hoạt động không?
                using var http = new System.Net.Http.HttpClient();
                http.Timeout = TimeSpan.FromSeconds(5);
                var ping = await http.GetAsync("https://www.google.com");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Internet: {ping.StatusCode}");

                // Test 2: Firestore URL có đúng không?
                var PROJECT_ID = "vinh-khanh-cms";
                var API_KEY = "AIzaSyDO7cvTxvx26Qu6Bo6Ts5ZT0cl8yBhcj5s";
                var url = $"https://firestore.googleapis.com/v1/projects/{PROJECT_ID}/databases/(default)/documents/pois?key={API_KEY}";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] URL: {url}");

                var resp = await http.GetAsync(url);
                var body = await resp.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] HTTP Status: {resp.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Response: {body.Substring(0, Math.Min(500, body.Length))}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Exception: {ex.GetType().Name}: {ex.Message}");
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Khi app từ background trở lại → sync lại để có data mới nhất từ CMS
            _ = PoiRepository.Instance.SyncFromFirebaseAsync();
        }

        //protected override void OnSleep()
        //{
        //    base.OnSleep();
        //    AudioService.Instance.Stop();
        //}
    }
}