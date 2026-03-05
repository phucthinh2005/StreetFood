using System;
using System.Collections.Generic;
using System.Text;
using MauiApp1.Models;
using System.Linq;

using MauiApp1.ViewModels;

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
    private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is POI selectedPOI)
        {
            await Shell.Current.GoToAsync(nameof(POIDetailPage), new Dictionary<string, object>
        {
            { "SelectedPOI", selectedPOI }
        });

            ((CollectionView)sender).SelectedItem = null; // bỏ chọn để không bị giữ màu
        }
    }
}