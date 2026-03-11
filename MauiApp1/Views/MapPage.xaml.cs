#pragma warning disable CA1416
using MauiApp1.Models;
using MauiApp1.ViewModels;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using System.Linq;

namespace MauiApp1.Views;

[QueryProperty(nameof(SelectedPOI), "SelectedPOI")]
public partial class MapPage : ContentPage
{
    MapViewModel vm;

    bool isFirstLoad = true;

    Dictionary<POI, Circle> poiCircles = new();
    POI? currentSelectedPOI;

    public POI SelectedPOI
    {
        set
        {
            if (value != null)
                ShowSelectedPOI(value);
        }
    }

    public MapPage()
    {
        InitializeComponent();

        vm = new MapViewModel();
        BindingContext = vm;

        vm.LocationUpdated += OnLocationUpdated;
        vm.POIsLoaded += LoadPOIs;
        vm.POIStateChanged += OnPOIStateChanged;

        // ===== Zoom map lần đầu =====
        //MainThread.BeginInvokeOnMainThread(() =>
        //{
        //    map.MoveToRegion(MapSpan.FromCenterAndRadius(
        //        new Location(10.761536, 106.702303),
        //        Distance.FromMeters(200)
        //    ));
        //});

        Init();
    }

    async void Init()
    {
        await vm.InitializeAsync();
    }

    private async void OnListTabTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ListPage),
            new Dictionary<string, object>
            {
                { "ViewModel", vm }
            });
    }

    private async void OnSettingClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SettingsPage));
    }

    // ===== Load POI =====
#pragma warning disable CA1416
    void LoadPOIs()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            map.Pins.Clear();
            map.MapElements.Clear();
            poiCircles.Clear();

            foreach (var poi in vm.POIs)
            {
                var location = new Location(poi.Latitude, poi.Longitude);

                var circle = new Circle
                {
                    Center = location,
                    Radius = Distance.FromMeters(poi.Radius),
                    StrokeWidth = 2
                };

#pragma warning disable CA1416
                circle.StrokeColor = Colors.Blue;
                circle.FillColor = Colors.Blue.WithAlpha(0.2f);
#pragma warning restore CA1416

                poiCircles[poi] = circle;
                map.MapElements.Add(circle);
            }

            foreach (var poi in vm.POIs)
            {
                var pin = new Pin
                {
                    Label = poi.Name,
                    Address = poi.Content,
                    Type = PinType.Place,
                    Location = new Location(poi.Latitude, poi.Longitude)
                };

                pin.MarkerClicked += (s, e) =>
                {
                    e.HideInfoWindow = false;
                };

                pin.InfoWindowClicked += async (s, e) =>
                {
                    await Shell.Current.GoToAsync(nameof(POIDetailPage),
                        new Dictionary<string, object>
                        {
                            { "SelectedPOI", poi }
                        });
                };

                map.Pins.Add(pin);
            }
        });
    }

    // ===== GPS update =====

    private void OnLocationUpdated(Location location)
    {
        if (!isFirstLoad) return;

        isFirstLoad = false;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var mapSpan = MapSpan.FromCenterAndRadius(
                new Location(location.Latitude, location.Longitude),
                Distance.FromMeters(200)
            );

            map.MoveToRegion(mapSpan);
        });
    }

    // ===== đổi màu vùng =====

    void OnPOIStateChanged(string name, bool isInside)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var poi = vm.POIs.FirstOrDefault(p => p.Name == name);

            if (poi == null) return;
            if (!poiCircles.ContainsKey(poi)) return;

            var circle = poiCircles[poi];

#pragma warning disable CA1416
            if (isInside)
            {
                circle.FillColor = Colors.Red.WithAlpha(0.3f);
                circle.StrokeColor = Colors.Red;
            }
            else
            {
                circle.FillColor = Colors.Blue.WithAlpha(0.2f);
                circle.StrokeColor = Colors.Blue;
            }
#pragma warning restore CA1416
        });
    }

    // ===== Hiện POI khi chọn từ List =====

    void ShowSelectedPOI(POI poi)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var location = new Location(poi.Latitude, poi.Longitude);

            map.MoveToRegion(MapSpan.FromCenterAndRadius(
                location,
                Distance.FromMeters(40)
            ));

            if (currentSelectedPOI != null && poiCircles.ContainsKey(currentSelectedPOI))
            {
                var oldCircle = poiCircles[currentSelectedPOI];

#pragma warning disable CA1416
                oldCircle.FillColor = Colors.Blue.WithAlpha(0.2f);
                oldCircle.StrokeColor = Colors.Blue;
#pragma warning restore CA1416
            }

            if (poiCircles.ContainsKey(poi))
            {
                var newCircle = poiCircles[poi];

#pragma warning disable CA1416
                newCircle.FillColor = Colors.Orange.WithAlpha(0.4f);
                newCircle.StrokeColor = Colors.Orange;
#pragma warning restore CA1416
            }

            currentSelectedPOI = poi;
        });
    }
}