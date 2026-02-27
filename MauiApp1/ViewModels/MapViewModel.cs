using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.ObjectModel;

namespace MauiApp1.ViewModels
{
    public class MapViewModel
    {
        private readonly GPSService _gpsService;

        public ObservableCollection<POI> POIs { get; set; }

        public event Action<Location>? LocationUpdated;
        public event Action<string, bool>? POIStateChanged;

        public MapViewModel()
        {
            _gpsService = new GPSService();

            POIs = new ObservableCollection<POI>
            {
                new POI
                {
                    Name = "Gian hàng A",
                    Latitude = 21.029000,
                    Longitude = 105.853800,
                    Radius = 20,
                    IsInside = false
                },
                new POI
                {
                    Name = "Gian hàng B",
                    Latitude = 21.028500,
                    Longitude = 105.854200,
                    Radius = 20,
                    IsInside = false
                },
                new POI
                {
                    Name = "Gian hàng C",
                    Latitude = 21.027500,
                    Longitude = 105.854500,
                    Radius = 20,
                    IsInside = false
                }

            };

            _gpsService.LocationChanged += OnLocationChanged;
            //_ = StartGPS(); tắt gps thật khi test
        }

        private async Task StartGPS()
        {
            await _gpsService.StartListeningAsync();
        }

        private void OnLocationChanged(Location location)
        {
            LocationUpdated?.Invoke(location);

            foreach (var poi in POIs)
            {
                double distanceKm = Location.CalculateDistance(
                    location.Latitude,
                    location.Longitude,
                    poi.Latitude,
                    poi.Longitude,
                    DistanceUnits.Kilometers);

                double distanceMeters = distanceKm * 1000;

                if (distanceMeters <= poi.Radius && !poi.IsInside)
                {
                    poi.IsInside = true;
                    POIStateChanged?.Invoke(poi.Name, true);
                }
                else if (distanceMeters > poi.Radius && poi.IsInside)
                {
                    poi.IsInside = false;
                    POIStateChanged?.Invoke(poi.Name, false);
                }
            }
        }
    //    public async Task SimulateMovement()
    //    {
    //        var testPath = new List<Location>
    //{
    //    new Location(21.028000,105.853000), // Ngoài tất cả

    //    new Location(21.029000,105.853800), // Trung tâm Gian hàng A

    //    new Location(21.028500,105.854200), // Trung tâm Gian hàng B

    //    new Location(21.027500,105.854500)  // Trung tâm Gian hàng C
    //};

    //        foreach (var location in testPath)
    //        {
    //            OnLocationChanged(location);
    //            await Task.Delay(3000); // 3 giây đổi vị trí
    //        }
    //    }
    }
}