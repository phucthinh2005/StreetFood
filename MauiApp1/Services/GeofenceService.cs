using MauiApp1.Models;
using Microsoft.Maui.Devices.Sensors;

namespace MauiApp1.Services
{
    public class GeofenceService
    {
        private List<POI> _pois;

        private const int AudioCooldownSeconds = 60;
        private const int NextAudioDelaySeconds = 3;

        private bool _isPlaying = false;

        public event Action<GeofenceEvent>? GeofenceTriggered;

        public GeofenceService(List<POI> pois)
        {
            _pois = pois ?? new List<POI>();
        }

        public async Task ProcessLocation(Location location)
        {
            List<POI> insidePOIs = new();

            foreach (var poi in _pois)
            {
                double distanceMeters =
                    Location.CalculateDistance(
                        location,
                        new Location(poi.Latitude, poi.Longitude),
                        DistanceUnits.Kilometers) * 1000;

                bool wasInside = poi.IsInside;
                bool isInside = distanceMeters <= poi.Radius;

                // ENTER
                if (!wasInside && isInside)
                {
                    poi.IsInside = true;

                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.Enter
                    });
                }

                // EXIT
                if (wasInside && !isInside)
                {
                    poi.IsInside = false;

                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.Exit
                    });

                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.StopAudio
                    });
                }

                if (isInside)
                    insidePOIs.Add(poi);
            }

            if (!insidePOIs.Any())
                return;

            if (!_isPlaying)
            {
                _isPlaying = true;
                await PlayInsidePOIs(insidePOIs);
                _isPlaying = false;
            }
        }

        private async Task PlayInsidePOIs(List<POI> insidePOIs)
        {
            var sorted = insidePOIs
                .OrderByDescending(p => p.Priority) //priority cao trước
                .ToList();

            foreach (var poi in sorted)
            {
                if (!poi.IsInside)
                    continue;

                if (!CanPlayAudio(poi))
                    continue;

                poi.LastTriggered = DateTime.Now;

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

                await Task.Delay(NextAudioDelaySeconds * 1000); //delay giữa các poi

                
            }
        }

        private bool CanPlayAudio(POI poi)
        {
            return (DateTime.Now - poi.LastTriggered)
                > TimeSpan.FromSeconds(AudioCooldownSeconds);
        }
    }
}