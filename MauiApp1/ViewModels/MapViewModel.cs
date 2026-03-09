using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.Database;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.ObjectModel;

namespace MauiApp1.ViewModels
{
    public class MapViewModel
    {
        // ===== Service =====

        private readonly GPSService _gpsService;
        private GeofenceService? _geofenceService;
        private readonly DatabaseService _database;

        // ===== Event =====

        public event Action? POIsLoaded;

        public ObservableCollection<POI> POIs { get; set; }

        public event Action<Location>? LocationUpdated;

        public event Action<string, bool>? POIStateChanged;

        // ===== Constructor =====

        public MapViewModel()
        {
            _gpsService = new GPSService();
            _database = new DatabaseService();

            POIs = new ObservableCollection<POI>();

            // khi GPS đổi vị trí
            _gpsService.LocationChanged += OnLocationChanged;

            // chạy GPS
            _ = StartGPS();
        }

        // ===== Khởi tạo dữ liệu =====

        public async Task InitializeAsync()
        {
            await LoadPOIs();
        }

        // ===== Load POI từ SQLite =====

        private async Task LoadPOIs()
        {
            await _database.ImportFromJson();

            var list = await _database.GetPOIsAsync();

            POIs.Clear();

            foreach (var poi in list)
                POIs.Add(poi);

            POIsLoaded?.Invoke();

            // tạo GeofenceService
            _geofenceService = new GeofenceService(POIs.ToList());

            _geofenceService.GeofenceTriggered += OnGeofenceTriggered;
        }

        // ===== Bắt đầu GPS =====

        private async Task StartGPS()
        {
            await _gpsService.StartListeningAsync();
        }

        // ===== Khi GPS thay đổi =====

        private void OnLocationChanged(Location location)
        {
            // gửi vị trí cho UI
            LocationUpdated?.Invoke(location);

            // xử lý geofence
            _geofenceService?.ProcessLocation(location);
        }

        // ===== Khi Geofence kích hoạt =====

        private CancellationTokenSource? _ttsToken;

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

                case GeofenceEventType.Audio:

                    _ttsToken = new CancellationTokenSource();

                    await TextToSpeech.Default.SpeakAsync(
                        $"Bạn đã vào {e.POI.Name}. {e.POI.Content}",
                        cancelToken: _ttsToken.Token);

                    break;

                case GeofenceEventType.StopAudio:

                    _ttsToken?.Cancel();

                    break;
            }
        }
    }
}