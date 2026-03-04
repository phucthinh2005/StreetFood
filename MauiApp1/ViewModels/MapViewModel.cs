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
                     Name = "Bò Né 3 Ngon",
                     Latitude = 10.768900,
                     Longitude = 106.690500,
                     Radius = 20,
                     NearRadius = 40,
                     Priority = 2,
                     Content = "Bò né Sài Gòn ",
                     Description = "Gian hàng chuyên bán đồ ăn ",
                     Detail = "Bò Né 3 Ngon phục vụ món bò né truyền thống Sài Gòn với thịt bò mềm, trứng ốp la, pate gan và bánh mì nóng giòn. Sốt đặc biệt là bí quyết làm nên thương hiệu 20 năm.",
                     ImageUrl = "bun_cha.jpg",
                     Language = "Tiếng Việt"
                },
               new POI
                {
                     Name = "Chè Miền Tây",
                     Latitude = 10.768500,
                     Longitude = 106.690800,
                     Radius = 20,
                     NearRadius = 40,
                     Priority = 3,
                     Content = "Chè truyền thống miền Nam",
                     Description = "Gian hàng chuyên bán đồ ăn vặt",
                     Detail = "Chè Miền Tây với đa dạng các loại chè: chè thập cẩm, chè dừa dầm, chè bưởi, chè đậu đỏ. Tất cả đều được nấu thủ công mỗi ngày với nguyên liệu tươi ngon nhất.",
                     ImageUrl = "che.jpg",
                     Language = "Tiếng Việt"
                },

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
            var route = new List<Location>
    {
        // Bắt đầu xa khu vực
        new Location(10.769500, 106.689800),

        // Đi dọc đường tiến gần POI A
        new Location(10.769200, 106.690000),
        new Location(10.769000, 106.690200),

        // Gần POI A
        new Location(10.768950, 106.690350),

        // Vào POI A
        new Location(10.768900, 106.690500),

        // Rời POI A
        new Location(10.768850, 106.690650),

        // Tiến về POI B
        new Location(10.768700, 106.690750),

        // Vào POI B
        new Location(10.768500, 106.690800),

        // Rời POI B
        new Location(10.768300, 106.691000),

        // Đi xa khỏi toàn bộ khu vực
        new Location(10.767900, 106.691500)
    };

            foreach (var loc in route)
            {
                SimulateLocation(loc.Latitude, loc.Longitude);
                await Task.Delay(5000); // 5 giây mỗi bước (giống đi bộ)
            }
        }

    }
}