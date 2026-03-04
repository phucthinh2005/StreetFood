using MauiApp1.Models;
using Microsoft.Maui.Devices.Sensors;

namespace MauiApp1.Services
{
    public class GeofenceService
    {
        private List<POI> _pois;

        private const int AudioCooldownSeconds = 15;

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
            List<POI> justEntered = new();

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
                // ===== ENTER =====
                if (!wasInside && isInside)
                {
                    poi.IsInside = true;
                    poi.IsNear = true;

                    // 🟢 Đổi màu sang xanh
                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.Enter
                    });

                    justEntered.Add(poi);
                }

                // ===== EXIT (KHÔNG báo gì) =====

                // ===== EXIT =====
                if (wasInside && !isInside)
                {
                    poi.IsInside = false;
                    poi.IsNear = false;

                    // 🔘 Đổi lại màu mặc định
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
            }

            // ===== AUDIO CHỈ CHẠY KHI VỪA ENTER =====
            if (justEntered.Any())
            {
                var highestPriority = justEntered
                    .OrderByDescending(p => p.Priority)
                    .First();

                if (CanPlayAudio(highestPriority))
                {
                    highestPriority.LastTriggered = DateTime.Now;

                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = highestPriority,
                        EventType = GeofenceEventType.Audio
                    });
                }
            }
        }

        private bool CanPlayAudio(POI poi)
        {
            return (DateTime.Now - poi.LastTriggered)
                   > TimeSpan.FromSeconds(AudioCooldownSeconds);
        }
    }
}