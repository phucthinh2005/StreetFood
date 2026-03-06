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
        
        public MapViewModel()
        {
            _gpsService = new GPSService();

            
            POIs = new ObservableCollection<POI>
            {
               new POI
                {
                     Name = "Bò Né 3 Ngon",
                     Latitude = 10.761819,
                     Longitude = 106.702132,
                     Radius = 10,
                     NearRadius = 30,
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
                     Latitude = 10.761536,
                     Longitude = 106.702303,
                     Radius = 10,
                     NearRadius = 30,
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

            _ = StartGPS();
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
       
        

    }
}