#pragma warning disable CA1416
using System;
using System.Globalization;
using System.Threading;
using MauiApp1.Resources.Languages;
using MauiApp1.Services;

namespace MauiApp1.Views;

// trang cai dat cua ung dung
public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        // lay gia tri da luu trong SettingsService
        volumeSlider.Value = SettingsService.Volume;
        speedSlider.Value = SettingsService.SpeechSpeed;

        gpsSwitch.IsToggled = SettingsService.GPSBackground;

        // cap nhat label ngay khi mo trang
        VolumeChanged(volumeSlider, new ValueChangedEventArgs(0, volumeSlider.Value));
        SpeedChanged(speedSlider, new ValueChangedEventArgs(0, speedSlider.Value));

        UpdateLanguageLabel(); // cap nhat text hien thi ngon ngu
    }

    // ===== Volume =====
    void VolumeChanged(object sender, ValueChangedEventArgs e)
    {
        int volume = (int)e.NewValue;

        // luu volume vao settings
        SettingsService.Volume = volume;

        // hien thi label: Volume: xx%
        volumeLabel.FormattedText = new FormattedString
        {
            Spans =
            {
                new Span { Text = AppResources.Volume },
                new Span { Text = ": " },
                new Span { Text = $"{volume}%" }
            }
        };
    }

    // ===== Speed =====
    void SpeedChanged(object sender, ValueChangedEventArgs e)
    {
        double speed = e.NewValue;

        // luu toc do doc audio
        SettingsService.SpeechSpeed = speed;

        // hien thi label: Speed: x.x
        speedLabel.FormattedText = new FormattedString
        {
            Spans =
            {
                new Span { Text = AppResources.SpeechSpeed },
                new Span { Text = ": " },
                new Span { Text = $"{speed:F1}x" }
            }
        };
    }

    // ===== Choose Language =====
    async void ChooseLanguage(object sender, EventArgs e)
    {
        // hien thi menu chon ngon ngu
        string result = await DisplayActionSheetAsync(
            AppResources.ChooseLanguage,
            AppResources.Cancel,
            null,
            "English",
            "Tiếng Việt",
            "日本語"
        );

        if (result == null || result == AppResources.Cancel)
            return;

        // doi ngon ngu
        if (result == "English")
            SetLanguage("en");

        else if (result == "Tiếng Việt")
            SetLanguage("vi");

        else if (result == "日本語")
            SetLanguage("ja");
    }

    // ===== Set Language =====
    void SetLanguage(string lang)
    {
        // luu ngon ngu vao settings
        SettingsService.Language = lang;

        // tao culture moi
        var culture = new CultureInfo(lang);

        // cap nhat culture cho toan app
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        // cap nhat resource language
        AppResources.Culture = culture;

        // reload lai AppShell de refresh UI
        var app = Application.Current;

        if (app != null && app.Windows.Count > 0)
        {
            app.Windows[0].Page = new AppShell();
        }
    }

    // ===== Update Label =====
    void UpdateLanguageLabel()
    {
        var lang = SettingsService.Language;

        // hien thi ten ngon ngu hien tai
        if (lang == "en")
            languageLabel.Text = "English";

        else if (lang == "vi")
            languageLabel.Text = "Tiếng Việt";

        else if (lang == "ja")
            languageLabel.Text = "日本語";
    }

    // nut reset tat ca cai dat
    async void ResetSettings(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlertAsync(
            AppResources.Reset,
            AppResources.ResetConfirm,
            AppResources.OK,
            AppResources.Cancel);

        if (!confirm) return;

        // reset settings
        SettingsService.Volume = 100;
        SettingsService.SpeechSpeed = 1.0;
        SettingsService.GPSBackground = true;

        // cap nhat UI
        volumeSlider.Value = 100;
        speedSlider.Value = 1.0;
        gpsSwitch.IsToggled = true;

        // bật lại GPS service
        BackgroundGpsManager.Start();

        // reset language
        SetLanguage("vi");
    }

    // ===== GPS Background =====
    void GpsToggled(object sender, ToggledEventArgs e)
    {
        bool enabled = e.Value;

        // luu trang thai GPS background
        SettingsService.GPSBackground = enabled;

        // bat / tat GPS background
        if (enabled)
        {
            BackgroundGpsManager.Start();
        }
        else
        {
            BackgroundGpsManager.Stop();
        }
    }
}