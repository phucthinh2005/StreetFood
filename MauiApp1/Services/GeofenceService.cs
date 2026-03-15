using MauiApp1.Models;
using Microsoft.Maui.Devices.Sensors;

namespace MauiApp1.Services
{
    // service xu ly geofence (kiem tra vao/ra khu vuc POI)
    public class GeofenceService
    {
        // danh sach POI
        private List<POI> _pois;

        // thoi gian cooldown truoc khi audio cua 1 POI duoc phat lai
        private const int AudioCooldownSeconds = 60;

        // delay giua cac audio POI
        private const int NextAudioDelaySeconds = 3;

        // trang thai dang phat audio
        private bool _isPlaying = false;

        // su kien gui ve MapViewModel khi co thay doi geofence
        public event Action<GeofenceEvent>? GeofenceTriggered;

        public GeofenceService(List<POI> pois)
        {
            _pois = pois ?? new List<POI>();
        }

        // xu ly khi GPS cap nhat vi tri
        public async Task ProcessLocation(Location location)
        {
            // danh sach POI ma user dang o trong
            List<POI> insidePOIs = new();

            foreach (var poi in _pois)
            {
                // tinh khoang cach tu user den POI (met)
                double distanceMeters =
                    Location.CalculateDistance(
                        location,
                        new Location(poi.Latitude, poi.Longitude),
                        DistanceUnits.Kilometers) * 1000;

                bool wasInside = poi.IsInside; // trang thai truoc do
                bool isInside = distanceMeters <= poi.Radius; // trang thai hien tai

                // ENTER (lan dau vao khu vuc POI)
                if (!wasInside && isInside)
                {
                    poi.IsInside = true;

                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.Enter
                    });
                }

                // EXIT (roi khoi khu vuc POI)
                if (wasInside && !isInside)
                {
                    poi.IsInside = false;

                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.Exit
                    });

                    // dung audio cua POI
                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.StopAudio
                    });
                }

                // neu user dang o trong POI
                if (isInside)
                    insidePOIs.Add(poi);
            }

            // neu khong o trong POI nao thi ket thuc
            if (!insidePOIs.Any())
                return;

            // neu chua co audio dang phat
            if (!_isPlaying)
            {
                _isPlaying = true;

                // phat audio cac POI dang o trong
                await PlayInsidePOIs(insidePOIs);

                _isPlaying = false;
            }
        }

        // phat audio cac POI ma user dang o trong
        private async Task PlayInsidePOIs(List<POI> insidePOIs)
        {
            // sap xep POI theo priority (cao doc truoc)
            var sorted = insidePOIs
                .OrderByDescending(p => p.Priority) //priority cao trước
                .ToList();

            foreach (var poi in sorted)
            {
                // neu user da roi khoi POI thi bo qua
                if (!poi.IsInside)
                    continue;

                // neu chua du cooldown thi bo qua
                if (!CanPlayAudio(poi))
                    continue;

                // cap nhat lan trigger cuoi
                poi.LastTriggered = DateTime.Now;

                // gui su kien phat audio
                GeofenceTriggered?.Invoke(new GeofenceEvent
                {
                    POI = poi,
                    EventType = GeofenceEventType.Audio
                });

                //đợi audio đọc xong trước khi đọc POI tiếp
                while (AudioService.Instance.IsPlaying)
                {
                    await Task.Delay(300);
                }

                // delay truoc khi doc POI tiep theo
                await Task.Delay(NextAudioDelaySeconds * 1000); //delay giữa các poi
            }
        }

        // kiem tra POI co du dieu kien phat audio hay khong
        private bool CanPlayAudio(POI poi)
        {
            return (DateTime.Now - poi.LastTriggered)
                > TimeSpan.FromSeconds(AudioCooldownSeconds);
        }
    }
}