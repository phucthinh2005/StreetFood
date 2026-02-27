using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.ObjectModel;

namespace MauiApp1.ViewModels
{
    public class MapViewModel
    {
        private readonly GPSService _gpsService;
        private readonly GeofenceService _geofenceService;

        public ObservableCollection<POI> POIs { get; set; }

        public event Action<Location>? LocationUpdated;
        public event Action<string, bool>? POIStateChanged;

        //test
        public void SimulateLocation(double lat, double lng)
        {
            var fakeLocation = new Location(lat, lng);

            LocationUpdated?.Invoke(fakeLocation);
            _geofenceService.ProcessLocation(fakeLocation);
        }
        //
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
                    NearRadius = 40,
                    Priority = 2,
                    Content = "Sản phẩm khuyến mãi mới tại gian hàng A"
                },
                new POI
                {
                    Name = "Gian hàng B",
                    Latitude = 21.028500,
                    Longitude = 105.854200,
                    Radius = 20,
                    NearRadius = 40,
                    Priority = 3,
                    Content = "Sản phẩm giảm giá tại gian hàng B"
                },
                new POI
                {
                    Name = "Gian hàng C",
                    Latitude = 21.027500,
                    Longitude = 105.854500,
                    Radius = 20,
                    NearRadius = 60,
                    Priority = 1,
                    Content = "Sản phẩm mới tại gian hàng C"
                }
            };

            _geofenceService = new GeofenceService(POIs.ToList());
            _geofenceService.GeofenceTriggered += OnGeofenceTriggered;

            _gpsService.LocationChanged += OnLocationChanged;

            //_ = StartGPS();
        }

        private async Task StartGPS()
        {
            await _gpsService.StartListeningAsync();
        }

        private void OnLocationChanged(Location location)
        {
            LocationUpdated?.Invoke(location);

            _geofenceService.ProcessLocation(location);
        }

        //private async void OnGeofenceTriggered(GeofenceEvent e)
        //{
        //    switch (e.EventType)
        //    {
        //        case GeofenceEventType.Enter:
        //            await TextToSpeech.Default.SpeakAsync(
        //                $"Bạn đã vào {e.POI.Name}. {e.POI.Content}");
        //            POIStateChanged?.Invoke(e.POI.Name, true);
        //            break;

        //        case GeofenceEventType.Near:
        //            await TextToSpeech.Default.SpeakAsync(
        //                $"Bạn đang đến gần {e.POI.Name}");
        //            break;

        //        case GeofenceEventType.Exit:
        //            POIStateChanged?.Invoke(e.POI.Name, false);
        //            break;
        //    }
        //}
        private async void OnGeofenceTriggered(GeofenceEvent e)
        {
            switch (e.EventType)
            {
                case GeofenceEventType.Enter:
                    POIStateChanged?.Invoke(e.POI.Name, true);
                    break;

                case GeofenceEventType.Exit:
                    POIStateChanged?.Invoke(e.POI.Name, false);
                    break;

                case GeofenceEventType.Near:
                    await TextToSpeech.Default.SpeakAsync(
                        $"Bạn đang đến gần {e.POI.Name}");
                    break;

                case GeofenceEventType.Audio:
                    await TextToSpeech.Default.SpeakAsync(
                        $"Bạn đã vào {e.POI.Name}. {e.POI.Content}");
                    break;
            }
        }
        //test
        public async Task SimulateMovement()
        {
            var path = new List<Location>
    {
        // Bắt đầu ngoài vùng tất cả
        new Location(21.029500,105.853000),

        // Tiến gần A
        new Location(21.029100,105.853700),

        // Vào A
        new Location(21.029000,105.853800),

        // Rời A -> qua B
        new Location(21.028700,105.854000),

        // Vào B
        new Location(21.028500,105.854200),

        // Rời B -> qua C
        new Location(21.027900,105.854400),

        // Vào C
        new Location(21.027500,105.854500),

        // Ra khỏi C
        new Location(21.027000,105.854800)
    };

            foreach (var loc in path)
            {
                SimulateLocation(loc.Latitude, loc.Longitude);
                await Task.Delay(6000); // 4 giây mỗi bước
            }
        }

    }
}