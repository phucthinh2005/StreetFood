#pragma warning disable CA1416
using System;
using System.Globalization;
using System.Threading;
using MauiApp1.Resources.Languages;

namespace MauiApp1.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();
        UpdateLanguageLabel();
    }

    void VolumeChanged(object sender, ValueChangedEventArgs e)
    {
        volumeLabel.FormattedText = new FormattedString
        {
            Spans =
            {
                new Span { Text = AppResources.Volume },
                new Span { Text = ": " },
                new Span { Text = $"{(int)e.NewValue}%" }
            }
        };
    }

    void SpeedChanged(object sender, ValueChangedEventArgs e)
    {
        speedLabel.FormattedText = new FormattedString
        {
            Spans =
            {
                new Span { Text = AppResources.SpeechSpeed },
                new Span { Text = ": " },
                new Span { Text = $"{e.NewValue:F1}x" }
            }
        };
    }

    async void ChooseLanguage(object sender, EventArgs e)
    {
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

        if (result == "English")
            SetLanguage("en");
        else if (result == "Tiếng Việt")
            SetLanguage("vi");
        else if (result == "日本語")
            SetLanguage("ja");
    }

    void SetLanguage(string lang)
    {
        var culture = new CultureInfo(lang);

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        AppResources.Culture = culture;

        var app = Application.Current;

        if (app != null && app.Windows.Count > 0)
        {
            app.Windows[0].Page = new AppShell();
        }
    }

    void UpdateLanguageLabel()
    {
        var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        if (lang == "en")
            languageLabel.Text = "English";
        else if (lang == "vi")
            languageLabel.Text = "Tiếng Việt";
        else if (lang == "ja")
            languageLabel.Text = "日本語";
    }
}