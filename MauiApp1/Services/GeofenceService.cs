using System;
using System.Collections.Generic;
using System.Text;
using MauiApp1.Models;
using Microsoft.Maui.Devices.Sensors;

namespace MauiApp1.Services
{
    public class GeofenceService
    {
        private readonly List<POI> _pois;
        private const int CooldownSeconds = 6; // chống spam

        public event Action<GeofenceEvent>? GeofenceTriggered;

        public GeofenceService(List<POI> pois)
        {
            _pois = pois;
        }

        public void ProcessLocation(Location location)
        {
            List<POI> currentlyInside = new();

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
                    poi.IsNear = true; // inside cũng tính là near

                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.Enter
                    });
                }

                // ===== EXIT =====
                else if (wasInside && !isInside)
                {
                    poi.IsInside = false;

                    GeofenceTriggered?.Invoke(new GeofenceEvent
                    {
                        POI = poi,
                        EventType = GeofenceEventType.Exit
                    });
                }

                // ===== NEAR (chỉ khi vừa mới vào vùng near từ ngoài) =====
                if (!wasNear && isNear && !isInside && CanTrigger(poi))
                {
                    poi.IsNear = true;

                    Trigger(poi, GeofenceEventType.Near);
                }

                // ===== RA KHỎI NEAR =====
                if (wasNear && !isNear)
                {
                    poi.IsNear = false;
                }

                if (isInside)
                    currentlyInside.Add(poi);
            }

            // ===== AUDIO CHỈ PHÁT 1 POI ƯU TIÊN CAO (chỉ khi vừa Enter) =====
            if (currentlyInside.Any())
            {
                var highestPriority = currentlyInside
                    .OrderByDescending(p => p.Priority)
                    .First();

                if (CanTrigger(highestPriority))
                {
                    Trigger(highestPriority, GeofenceEventType.Audio);
                }
            }
        }

        private bool CanTrigger(POI poi)
        {
            return (DateTime.Now - poi.LastTriggered)
                   > TimeSpan.FromSeconds(CooldownSeconds);
        }

        private void Trigger(POI poi, GeofenceEventType type)
        {
            poi.LastTriggered = DateTime.Now;

            GeofenceTriggered?.Invoke(new GeofenceEvent
            {
                POI = poi,
                EventType = type
            });
        }
    }
}
