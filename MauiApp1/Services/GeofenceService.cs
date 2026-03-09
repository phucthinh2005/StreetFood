using MauiApp1.Models;
using Microsoft.Maui.Devices.Sensors;

namespace MauiApp1.Services
{
    public class GeofenceService
    {
        private List<POI> _pois;

        private const int AudioCooldownSeconds = 50;
        private const int NextAudioDelaySeconds = 6;

        private Queue<POI> _audioQueue = new();
        private bool _isPlaying = false;

        public event Action<GeofenceEvent>? GeofenceTriggered;

        public GeofenceService(List<POI> pois)
        {
            _pois = pois ?? new List<POI>();
        }

        public void UpdatePOIs(List<POI> pois)
        {
            _pois = pois ?? new List<POI>();
        }

        public async void ProcessLocation(Location location)
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
            {
                _audioQueue.Clear();
                _isPlaying = false;
                return;
            }

            var sorted = insidePOIs
                .OrderByDescending(p => p.Priority)
                .ToList();

            _audioQueue = new Queue<POI>(sorted);

            if (!_isPlaying)
            {
                _isPlaying = true;
                await PlayQueue();
            }
        }

        private async Task PlayQueue()
        {
            while (_audioQueue.Any())
            {
                var poi = _audioQueue.Dequeue();

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

                await Task.Delay(NextAudioDelaySeconds * 1000);
            }

            _isPlaying = false;
        }

        private bool CanPlayAudio(POI poi)
        {
            return (DateTime.Now - poi.LastTriggered)
                   > TimeSpan.FromSeconds(AudioCooldownSeconds);
        }
    }
}