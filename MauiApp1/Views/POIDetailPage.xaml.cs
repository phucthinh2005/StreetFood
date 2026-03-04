using MauiApp1.Models;
using Microsoft.Maui.Media;
using Microsoft.Maui.Media;
using System.Threading;

namespace MauiApp1.Views;

[QueryProperty(nameof(SelectedPOI), "SelectedPOI")]
public partial class POIDetailPage : ContentPage
{
    private POI? _selectedPOI;

    private CancellationTokenSource? _cts;
    private bool _isSpeaking = false;

    public POI? SelectedPOI
    {
        get => _selectedPOI;
        set
        {
            _selectedPOI = value;
            BindingContext = _selectedPOI;
        }
    }

    public POIDetailPage()
    {
        InitializeComponent();
    }

    // =========================
    // Nút đóng
    // =========================
    private async void OnCloseClicked(object sender, EventArgs e)
    {
        _cts?.Cancel(); // dừng nếu đang nói
        await Shell.Current.GoToAsync("..");
    }

    // =========================
    // Nút Nghe / Dừng
    // =========================
    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        if (BindingContext is not POI poi)
            return;

        // Nếu đang nói → bấm lại để dừng
        if (_isSpeaking)
        {
            _cts?.Cancel();
            _isSpeaking = false;
            SpeakButton.Text = "Nghe ngay";
            return;
        }

        if (string.IsNullOrEmpty(poi.Detail))
            return;

        _cts = new CancellationTokenSource();
        _isSpeaking = true;
        SpeakButton.Text = "Dừng";

        try
        {
            await TextToSpeech.SpeakAsync(
                poi.Detail,
                cancelToken: _cts.Token
            );
        }
        catch (OperationCanceledException)
        {
            // bị hủy giữa chừng → không làm gì
        }
        finally
        {
            _isSpeaking = false;
            SpeakButton.Text = "Nghe ngay";
        }
    }

    // =========================
    // Khi rời trang → tự động dừng
    // =========================
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cts?.Cancel();
    }
}