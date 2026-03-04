using System;
using System.Collections.Generic;
using System.Text;

using System;

namespace MauiApp1.Models
{
    public class POI
    {
        // ===== Thông tin cơ bản =====
        public string Name { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // ===== Thông tin hiển thị giao diện =====
        public string ImageUrl { get; set; } = "";
        public string Description { get; set; } = "";
        public string Detail { get; set; } = "";
        public string Language { get; set; } = "";

        // ===== Geofence =====
        public double Radius { get; set; } = 20;
        public double NearRadius { get; set; } = 40;
        public int Priority { get; set; } = 1;

        public bool IsInside { get; set; }
        public bool IsNear { get; set; }

        public string Content { get; set; } = ""; // nội dung tự động phát
        public DateTime LastTriggered { get; set; } = DateTime.MinValue;
    }
}

