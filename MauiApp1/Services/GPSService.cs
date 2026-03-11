using Microsoft.Maui.Devices.Sensors;

namespace MauiApp1.Services
{
    public class GPSService
    {
        private CancellationTokenSource? _cts;
        private bool _isTracking;

        public event Action<Location>? LocationChanged;

        public async Task StartListeningAsync()
        {
            if (_isTracking)
                return;

            _isTracking = true;
            _cts = new CancellationTokenSource();

            _ = TrackLocationAsync(_cts.Token);
        }

        private async Task TrackLocationAsync(CancellationToken token)
        {
            Location? lastLocation = null;

            const double MIN_DISTANCE_METERS = 2;

            int delay = 2000;
            int idleCounter = 0;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var request = new GeolocationRequest(
                        GeolocationAccuracy.High,
                        TimeSpan.FromSeconds(4)
                    );

                    var location = await Geolocation.Default
                        .GetLocationAsync(request, token);

                    if (location != null)
                    {
                        if (location.Accuracy > 40)
                            continue;

                        if (lastLocation == null)
                        {
                            lastLocation = location;
                            LocationChanged?.Invoke(location);
                        }
                        else
                        {
                            double distance =
                                Location.CalculateDistance(
                                    lastLocation,
                                    location,
                                    DistanceUnits.Kilometers) * 1000;

                            if (distance >= MIN_DISTANCE_METERS)
                            {
                                lastLocation = location;
                                idleCounter = 0;
                                delay = 2000;

                                LocationChanged?.Invoke(location);
                            }
                            else
                            {
                                idleCounter++;

                                if (idleCounter >= 5)
                                {
                                    LocationChanged?.Invoke(location);
                                    delay = 6000;
                                }
                            }
                        }
                    }
                }
                catch
                {
                }

                await Task.Delay(delay, token);
            }
        }

        public void StopListening()
        {
            if (!_isTracking)
                return;

            _cts?.Cancel();
            _isTracking = false;
        }
    }
}