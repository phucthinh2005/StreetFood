using MauiApp1.Models;
using Microsoft.Maui.Devices.Sensors;

namespace MauiApp1.Services
{
    public class GeofenceService
    {
        private List<POI> _pois;

        // 2 phút
        private const int AudioCooldownSeconds = 50;

        public event Action<GeofenceEvent>? GeofenceTriggered;

        public GeofenceService(List<POI> pois)
        {
            _pois = pois ?? new List<POI>();
        }

        public void UpdatePOIs(List<POI> pois)
        {
            _pois = pois ?? new List<POI>();
        }

        public void ProcessLocation(Location location)
        {
            List<POI> triggerAudio = new();

            foreach (var poi in _pois)
            {
                double distanceMeters =
                    Location.CalculateDistance(
                        location,
                        new Location(poi.Latitude, poi.Longitude),
                        DistanceUnits.Kilometers) * 1000;

                bool wasInside = poi.IsInside;
                bool wasNear = poi.IsNear;

                bool isInside = distanceMeters <= poi.Radius;
                bool isNear = distanceMeters <= poi.NearRadius;

                // ===== ENTER =====
                if (!wasInside && isInside)
                {
                    poi.IsInside = true;
                    poi.IsNear = true;

                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.Enter
                    });

                    triggerAudio.Add(poi);
                }

                // ===== EXIT =====
                if (wasInside && !isInside)
                {
                    poi.IsInside = false;
                    poi.IsNear = false;

                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.Exit
                    });
                }

                // ===== NEAR =====
                if (!wasNear && isNear && !isInside)
                {
                    poi.IsNear = true;

                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.Near
                    });
                }

                if (wasNear && !isNear)
                {
                    poi.IsNear = false;
                }

                // ===== ĐỨNG YÊN TRONG VÙNG -> 2 PHÚT PHÁT LẠI =====
                if (isInside && CanPlayAudio(poi))
                {
                    triggerAudio.Add(poi);
                }
            }

            // ===== CHỌN POI ƯU TIÊN CAO NHẤT =====
            if (triggerAudio.Any())
            {
                var highestPriority = triggerAudio
                    .OrderByDescending(p => p.Priority)
                    .First();

                highestPriority.LastTriggered = DateTime.Now;

                GeofenceTriggered?.Invoke(new GeofenceEvent
                {
                    POI = highestPriority,
                    EventType = GeofenceEventType.Audio
                });
            }
        }

        private bool CanPlayAudio(POI poi)
        {
            return (DateTime.Now - poi.LastTriggered)
                   > TimeSpan.FromSeconds(AudioCooldownSeconds);
        }
    }
}