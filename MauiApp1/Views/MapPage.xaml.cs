using MauiApp1.Models;
using MauiApp1.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;

namespace MauiApp1.Views;

public partial class MapPage : ContentPage
{
    MapViewModel vm;

    public MapPage()
    {
        InitializeComponent();

        vm = new MapViewModel();
        BindingContext = vm;

        vm.LocationUpdated += OnLocationUpdated;

        LoadPOIs();

        map.MoveToRegion(
            MapSpan.FromCenterAndRadius(
                new Location(10.761536, 106.702303),
                Distance.FromMeters(300)
            ));
    }

    private async void OnListTabTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ListPage),
            new Dictionary<string, object>
            {
                { "ViewModel", vm }
            });
    }

    void LoadPOIs()
    {
        foreach (var poi in vm.POIs)
        {
            var pin = new Pin
            {
                Label = poi.Name,
                Address = poi.Content,
                Type = PinType.Place,
                Location = new Location(poi.Latitude, poi.Longitude)
            };

            pin.MarkerClicked += async (s, e) =>
            {
                e.HideInfoWindow = true;

                await Shell.Current.GoToAsync(nameof(POIDetailPage),
                    new Dictionary<string, object>
                    {
                        { "SelectedPOI", poi }
                    });
            };

            map.Pins.Add(pin);
        }
    }

    private void OnLocationUpdated(Location location)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var mapSpan = MapSpan.FromCenterAndRadius(
                new Location(location.Latitude, location.Longitude),
                Distance.FromMeters(200)
            );

            map.MoveToRegion(mapSpan);
        });
    }
}