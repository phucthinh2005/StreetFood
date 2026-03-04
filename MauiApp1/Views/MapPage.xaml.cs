using System;
using System.Linq;
using System.Text.Json;
using MauiApp1.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;

namespace MauiApp1.Views
{
    public partial class MapPage : ContentPage
    {
        int currentZoom = 17;
        MapViewModel vm;

        public MapPage()
        {
            InitializeComponent();

            vm = new MapViewModel();
            BindingContext = vm;

            vm.LocationUpdated += OnLocationUpdated;
            vm.POIStateChanged += OnPOIStateChanged;

            mapWebView.Navigating += MapWebView_Navigating;

            LoadMapHtml();
        }
        private async void OnListTabTapped(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(ListPage),
                new Dictionary<string, object>
                {
            { "ViewModel", vm }
                });
        }

        // ================================
        // GPS cập nhật vị trí user
        // ================================
        private void OnLocationUpdated(Location location)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await mapWebView.EvaluateJavaScriptAsync(
                    $"updateUserLocation({location.Latitude}, {location.Longitude})");
            });
        }

        // ================================
        // Đổi màu circle khi vào/ra vùng
        // ================================
        private void OnPOIStateChanged(string poiName, bool isInside)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                string color = isInside ? "green" : "red";

                await mapWebView.EvaluateJavaScriptAsync(
                    $"setPoiColor('{poiName}', '{color}')");
            });
        }

        // ================================
        // Bắt click từ Leaflet marker
        // ================================
        private async void MapWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("poi://"))
            {
                e.Cancel = true;

                var poiName = Uri.UnescapeDataString(
                    e.Url.Replace("poi://", "")
                );

                var selectedPoi = vm.POIs
                    .FirstOrDefault(p => p.Name == poiName);

                if (selectedPoi != null)
                {
                    await Shell.Current.GoToAsync(nameof(POIDetailPage),
                         new Dictionary<string, object>
                         {
                            { "SelectedPOI", selectedPoi }
                         });

                }
            }
        }

        // ================================
        // Load Leaflet Map
        // ================================
        void LoadMapHtml()
        {
            var poiJson = JsonSerializer.Serialize(vm.POIs);

            //_ = vm.SimulateMovement(); // test GPS

            var html = $@"<!DOCTYPE html>
<html>
<head>
<meta name='viewport' content='width=device-width, initial-scale=1.0' />
<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'/>
<style>
html,body,#map{{height:100%;margin:0;padding:0}}
</style>
</head>
<body>
<div id='map'></div>

<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
<script>

var map = L.map('map');
map.setView([10.768900, 106.690500], 18);

L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
maxZoom:19
}}).addTo(map);

var userMarker = null;

function updateUserLocation(lat,lng){{
    if(userMarker==null){{
        userMarker=L.marker([lat,lng]).addTo(map);
    }}else{{
        userMarker.setLatLng([lat,lng]);
    }}
}}

function setZoom(z){{
    map.setZoom(z);
}}

var poiData = {poiJson};
var poiCircles = {{}};

poiData.forEach(function(poi){{

    var marker = L.marker([poi.Latitude, poi.Longitude])
        .addTo(map)
        .bindPopup(
            ""<div style='min-width:160px'>""
            + ""<b>"" + poi.Name + ""</b><br/>""
            + ""<span style='font-size:13px'>"" + poi.Content + ""</span>""
            + ""</div>""
        );

    // 👉 Khi click marker
    marker.on('click', function () {{
        window.location.href = 'poi://' + encodeURIComponent(poi.Name);
    }});

    var circle = L.circle([poi.Latitude, poi.Longitude], {{
        radius: poi.Radius,
        color: 'red',
        fillOpacity: 0.2
    }}).addTo(map);

    poiCircles[poi.Name] = circle;
}});

function setPoiColor(name, color){{
    if(poiCircles[name]){{
        poiCircles[name].setStyle({{ color: color }});
    }}
}}

</script>
</body>
</html>";

            mapWebView.Source = new HtmlWebViewSource { Html = html };
        }

        // ================================
        // Zoom
        // ================================
        private async void ZoomIn_Clicked(object sender, EventArgs e)
        {
            currentZoom = Math.Min(19, currentZoom + 1);
            await mapWebView.EvaluateJavaScriptAsync($"setZoom({currentZoom})");
        }

        private async void ZoomOut_Clicked(object sender, EventArgs e)
        {
            currentZoom = Math.Max(1, currentZoom - 1);
            await mapWebView.EvaluateJavaScriptAsync($"setZoom({currentZoom})");
        }
    }
}