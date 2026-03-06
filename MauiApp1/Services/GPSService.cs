using Microsoft.Maui.Devices.Sensors;
using System.Threading;

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

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var request = new GeolocationRequest(
                        GeolocationAccuracy.High,
                        TimeSpan.FromSeconds(3)   // ⬅ GPS timeout 3s
                    );

                    var location = await Geolocation.Default
                        .GetLocationAsync(request, token);

                    if (location != null)
                    {
                        if (lastLocation == null)
                        {
                            lastLocation = location;
                            LocationChanged?.Invoke(location);
                        }
                        else
                        {
                            double distanceInMeters =
                                Location.CalculateDistance(
                                    lastLocation,
                                    location,
                                    DistanceUnits.Kilometers) * 1000;

                            // Chỉ cập nhật ke ca dung yen
                            if (location != null)
                            {
                                lastLocation = location;
                                LocationChanged?.Invoke(location);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }

                await Task.Delay(3000, token); // ⬅ cập nhật mỗi 3s
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