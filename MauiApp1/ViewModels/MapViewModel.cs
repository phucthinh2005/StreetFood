using MauiApp1.Models;          // chứa class POI
using MauiApp1.Services;        // chứa GPSService và GeofenceService
using MauiApp1.Database;        // chứa DatabaseService (SQLite)
using Microsoft.Maui.Devices.Sensors; // dùng Location (GPS)
using System.Collections.ObjectModel; // dùng ObservableCollection

namespace MauiApp1.ViewModels
{
    public class MapViewModel
    {
        // ===== Service =====

        // service lấy vị trí GPS
        private readonly GPSService _gpsService;

        // service xử lý Geofence (vùng địa lý)
        private GeofenceService? _geofenceService;

        // service làm việc với SQLite database
        private readonly DatabaseService _database;

        // ===== Event =====

        // event báo cho UI biết POI đã load xong
        public event Action? POIsLoaded;

        // danh sách POI hiển thị trên Map
        public ObservableCollection<POI> POIs { get; set; }

        // event gửi vị trí GPS mới cho UI
        public event Action<Location>? LocationUpdated;

        // event báo trạng thái vào / ra POI
        public event Action<string, bool>? POIStateChanged;

        // ===== Constructor =====
        public MapViewModel()
        {
            // tạo GPS service
            _gpsService = new GPSService();

            // tạo database service
            _database = new DatabaseService();

            // khởi tạo danh sách POI
            POIs = new ObservableCollection<POI>();

            // khi GPS thay đổi vị trí -> gọi hàm OnLocationChanged
            _gpsService.LocationChanged += OnLocationChanged;

            // LoadPOIs();  (đã chuyển sang InitializeAsync)

            // chạy GPS
            _ = StartGPS();
        }

        // ===== Hàm khởi tạo dữ liệu =====
        public async Task InitializeAsync()
        {
            await LoadPOIs();
        }

        // ===== Load POI từ database =====
        private async Task LoadPOIs()
        {
            // import dữ liệu từ file JSON vào SQLite (chỉ khi DB chưa có)
            await _database.ImportFromJson();

            // lấy danh sách POI từ SQLite
            var list = await _database.GetPOIsAsync();

            // xóa danh sách POI cũ
            POIs.Clear();

            // thêm từng POI vào ObservableCollection
            foreach (var poi in list)
                POIs.Add(poi);

            // báo cho UI biết đã load xong POI
            POIsLoaded?.Invoke();

            // tạo GeofenceService với danh sách POI
            _geofenceService = new GeofenceService(POIs.ToList());

            // khi Geofence kích hoạt -> gọi hàm OnGeofenceTriggered
            _geofenceService.GeofenceTriggered += OnGeofenceTriggered;
        }

        // ===== Bắt đầu GPS =====
        private async Task StartGPS()
        {
            await _gpsService.StartListeningAsync();
        }

        // ===== Khi vị trí GPS thay đổi =====
        private void OnLocationChanged(Location location)
        {
            // gửi vị trí mới cho UI (MapPage)
            LocationUpdated?.Invoke(location);

            // xử lý Geofence nếu service đã được tạo
            // dấu ? để tránh lỗi null
            _geofenceService?.ProcessLocation(location);
        }

        // ===== Khi Geofence được kích hoạt =====
        private async void OnGeofenceTriggered(GeofenceEvent e)
        {
            switch (e.EventType)
            {
                // ===== vào khu vực POI =====
                case GeofenceEventType.Enter:
                    POIStateChanged?.Invoke(e.POI.Name, true);
                    break;

                // ===== ra khỏi khu vực POI =====
                case GeofenceEventType.Exit:
                    POIStateChanged?.Invoke(e.POI.Name, false);
                    break;

                // ===== đến gần POI =====
                case GeofenceEventType.Near:
                    await TextToSpeech.Default.SpeakAsync(
                        $"Bạn đang đến gần {e.POI.Name}");
                    break;

                // ===== vào vùng audio của POI =====
                case GeofenceEventType.Audio:
                    await TextToSpeech.Default.SpeakAsync(
                        $"Bạn đã vào {e.POI.Name}. {e.POI.Content}");
                    break;
            }
        }
    }
}