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
        // service lay GPS
        private readonly GPSService _gpsService;

        // service xu ly geofence (vao/ra khu vuc)
        private GeofenceService? _geofenceService;

        // service database
        private readonly DatabaseService _database;

        // service audio doc noi dung
        private readonly AudioService _audioService;

        // danh sach POI hien thi tren map/list
        public ObservableCollection<POI> POIs { get; set; }

        // danh sach POI day du (de filter tim kiem)
        List<POI> _allPOIs = new();//them danh sách đầy đủ để filter

        // su kien khi POI load xong
        public event Action? POIsLoaded;

        // su kien khi GPS cap nhat vi tri
        public event Action<Location>? LocationUpdated;

        // su kien khi trang thai POI thay doi (vao/ra khu vuc)
        public event Action<string, bool>? POIStateChanged;


        // constructor ViewModel
        public MapViewModel()
        {
            _gpsService = new GPSService();
            _database = new DatabaseService();

            POIs = new ObservableCollection<POI>();

            // dang ky su kien GPS thay doi vi tri
            _gpsService.LocationChanged += OnLocationChanged;

            // dung AudioService chung
            _audioService = AudioService.Instance;

            // bat dau GPS
            _ = StartGPS();
        }

        // khoi tao du lieu
        public async Task InitializeAsync()
        {
            await LoadPOIs();
        }

        // load POI tu database
        private async Task LoadPOIs()
        {
            // import JSON vao database neu chua co
            await _database.ImportFromJson();

            // lay danh sach POI
            var list = await _database.GetPOIsAsync();

            // luu danh sach day du de dung cho filter
            _allPOIs = list.ToList();   // them vao danh sach day du de filter

            POIs.Clear();

            // dua POI vao ObservableCollection
            foreach (var poi in list)
                POIs.Add(poi);

            // thong bao POI da load xong
            POIsLoaded?.Invoke();

            // tao geofence service
            _geofenceService = new GeofenceService(POIs.ToList());

            // dang ky su kien geofence
            _geofenceService.GeofenceTriggered += OnGeofenceTriggered;
        }

        // bat dau nghe GPS
        private async Task StartGPS()
        {
            await _gpsService.StartListeningAsync();
        }

        // khi GPS thay doi vi tri
        private async void OnLocationChanged(Location location)
        {
            // gui vi tri moi len UI
            LocationUpdated?.Invoke(location);

            // xu ly geofence
            if (_geofenceService != null)
                await _geofenceService.ProcessLocation(location);
        }

        // khi co su kien geofence (vao/ra vung)
        private async void OnGeofenceTriggered(GeofenceEvent e)
        {
            switch (e.EventType)
            {
                case GeofenceEventType.Enter:

                    // vao khu vuc POI
                    POIStateChanged?.Invoke(e.POI.Name, true);
                    break;

                case GeofenceEventType.Exit:

                    // roi khoi khu vuc POI
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

                    // noi dung audio khi vao POI
                    var text =
                        $"{AppResources.EnterArea} {e.POI.Name}. {e.POI.Content}";

                    await _audioService.Speak(text);

                    break;
            }
        }

        // dung GPS
        public void Stop()
        {
            _gpsService.StopListening();
        }

        // ham filter POI theo tu khoa tim kiem
        public void FilterPois(string keyword) //thêm hàm filter
        {
            keyword = keyword?.ToLower() ?? "";

            // neu khong co keyword -> hien tat ca
            var filtered = string.IsNullOrWhiteSpace(keyword)
                ? _allPOIs
                : _allPOIs.Where(p =>
                    (p.Name?.ToLower().Contains(keyword) ?? false)); //|| (p.Description?.ToLower().Contains(keyword) ?? false));


            MainThread.BeginInvokeOnMainThread(() =>
            {
                POIs.Clear();

                // them lai POI sau khi filter
                foreach (var poi in filtered)
                    POIs.Add(poi);
            });
        }


    }
}