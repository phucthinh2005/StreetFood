#pragma warning disable CA1416

using MauiApp1.Models;
using MauiApp1.ViewModels;
using Microsoft.Maui.Media;


namespace MauiApp1.Views;

[QueryProperty(nameof(SelectedPOI), "SelectedPOI")]
public partial class POIDetailPage : ContentPage
{
    private readonly POIDetailViewModel _viewModel;

    public POI? SelectedPOI
    {
        set => _viewModel.SelectedPOI = value;
    }

    public POIDetailPage()
    {
        InitializeComponent();        // Load XAML
        _viewModel = new POIDetailViewModel();
        BindingContext = _viewModel;  // Gắn ViewModel
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.Stop();            // Dừng audio khi rời trang
    }
}