using MauiApp1.Models;
using MauiApp1.ViewModels;
using System;
using System.Collections.Generic;

namespace MauiApp1.Views;

[QueryProperty(nameof(ViewModel), "ViewModel")]
public partial class ListPage : ContentPage
{
    public MapViewModel ViewModel
    {
        set => BindingContext = value;
    }

    public ListPage()
    {
        InitializeComponent();
    }

    private async void DetailButton_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.CommandParameter is POI selectedPOI)
        {
            await Shell.Current.GoToAsync(nameof(POIDetailPage), new Dictionary<string, object>
            {
                { "SelectedPOI", selectedPOI }
            });
        }
    }
}