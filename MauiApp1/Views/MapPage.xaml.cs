using System;
using MauiApp1.Services;
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

// ====== INIT MAP (Center vào giữa 3 POI) ======
var map = L.map('map');

// Trung tâm giữa 3 gian hàng
map.setView([21.028800,105.854000], 18);

L.tileLayer('https://{{s}}.tile.openstreetmap.org/{{z}}/{{x}}/{{y}}.png', {{
maxZoom:19
}}).addTo(map);

// ====== USER MARKER ======
var userMarker = null;

function updateUserLocation(lat,lng){{

    if(userMarker==null){{
        userMarker=L.marker([lat,lng]).addTo(map);
    }}else{{
        userMarker.setLatLng([lat,lng]);
    }}

    // Không auto focus lại → tránh nhảy map
}}

function setZoom(z){{
    map.setZoom(z);
}}

// ====== POI MARKERS ======
// ====== POI + VÙNG BÁN KÍNH ======

var poiData = [
    {{ name: ""Gian hàng A"", lat: 21.029000, lng: 105.853800 }},
    {{ name: ""Gian hàng B"", lat: 21.028500, lng: 105.854200 }},
    {{ name: ""Gian hàng C"", lat: 21.027500, lng: 105.854500 }}
];

poiData.forEach(function(poi){{

    // Marker
    L.marker([poi.lat, poi.lng])
        .addTo(map)
        .bindPopup(poi.name);

    // Circle bán kính 20m
    L.circle([poi.lat, poi.lng], {{
        radius: 20,       // 20 mét
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