using System.Collections.ObjectModel;
using MauiApp1.Database;
using MauiApp1.Models;
using MauiApp1.Resources.Languages;
using MauiApp1.Services;
using Microsoft.Maui.Devices.Sensors;

namespace MauiApp1.ViewModels
{
    public class MapViewModel
    {
        private readonly GPSService _gpsService;
        private GeofenceService? _geofenceService;
        private readonly DatabaseService _database;

        private readonly AudioService _audioService;

        public ObservableCollection<POI> POIs { get; set; }

        List<POI> _allPOIs = new();//them danh sách đầy đủ để filter

        public event Action? POIsLoaded;
        public event Action<Location>? LocationUpdated;
        public event Action<string, bool>? POIStateChanged;


        


        public MapViewModel()
        {
            _gpsService = new GPSService();
            _database = new DatabaseService();

            POIs = new ObservableCollection<POI>();

            _gpsService.LocationChanged += OnLocationChanged;

            // dung AudioService chung
            _audioService = AudioService.Instance;

            _ = StartGPS();
        }

        public async Task InitializeAsync()
        {
            await LoadPOIs();
        }

        private async Task LoadPOIs()
        {
            await _database.ImportFromJson();

            var list = await _database.GetPOIsAsync();

            _allPOIs = list.ToList();   // them vao danh sach day du de filter

            POIs.Clear();

            foreach (var poi in list)
                POIs.Add(poi);

            POIsLoaded?.Invoke();

            _geofenceService = new GeofenceService(POIs.ToList());
            _geofenceService.GeofenceTriggered += OnGeofenceTriggered;
        }

        private async Task StartGPS()
        {
            await _gpsService.StartListeningAsync();
        }

        private async void OnLocationChanged(Location location)
        {
            LocationUpdated?.Invoke(location);

            if (_geofenceService != null)
                await _geofenceService.ProcessLocation(location);
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

                // 🔴 thêm case này để dừng audio khi user rời khỏi khu vực
                //case GeofenceEventType.StopAudio:

                //    _audioService.Stop();
                //    break;

                case GeofenceEventType.Audio:

                    // nếu user đang nghe manual thì GPS không đọc
                    if (_audioService.IsManualMode)
                        return;

                    // nếu đang có audio khác
                    if (_audioService.IsPlaying)
                        return;

                    var text =
                        $"{AppResources.EnterArea} {e.POI.Name}. {e.POI.Content}";

                    await _audioService.Speak(text);

                    break;
            }
        }

        public void Stop()
        {
            _gpsService.StopListening();
        }

        public void FilterPois(string keyword) //thêm hàm filter
        {
            keyword = keyword?.ToLower() ?? "";

            var filtered = string.IsNullOrWhiteSpace(keyword)
                ? _allPOIs
                : _allPOIs.Where(p =>
                    (p.Name?.ToLower().Contains(keyword) ?? false)); //|| (p.Description?.ToLower().Contains(keyword) ?? false));


            MainThread.BeginInvokeOnMainThread(() =>
            {
                POIs.Clear();
               

                foreach (var poi in filtered)
                    POIs.Add(poi);
            });
        }


    }
}