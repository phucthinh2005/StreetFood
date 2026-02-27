using System;
using MauiApp1.Services;
using MauiApp1.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using System.Text.Json;

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

            LoadMapHtml();
        }

        private void OnLocationUpdated(Location location)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await mapWebView.EvaluateJavaScriptAsync(
                    $"updateUserLocation({location.Latitude}, {location.Longitude})");
            });
        }

        void LoadMapHtml()
        {
            var poiJson = JsonSerializer.Serialize(vm.POIs);

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
map.setView([21.028800,105.854000], 18);

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

// ===== NHẬN POI TỪ C# =====
var poiData = {poiJson};

poiData.forEach(function(poi){{

    L.marker([poi.Latitude, poi.Longitude])
        .addTo(map)
        .bindPopup(poi.Name);

    L.circle([poi.Latitude, poi.Longitude], {{
        radius: poi.Radius,
        color: 'red',
        fillOpacity: 0.2
    }}).addTo(map);

}});

</script>
</body>
</html>";

            mapWebView.Source = new HtmlWebViewSource { Html = html };
        }

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