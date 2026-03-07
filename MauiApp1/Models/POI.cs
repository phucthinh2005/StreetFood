using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace MauiApp1.Models
{
    public class POI
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // ===== Thông tin cơ bản =====
        public string Name { get; set; } = "";
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // ===== Thông tin hiển thị =====
        public string ImageUrl { get; set; } = "";
        public string Description { get; set; } = "";
        public string Detail { get; set; } = "";
        public string Language { get; set; } = "";

        // ===== Geofence config =====
        public double Radius { get; set; } = 20;
        public double NearRadius { get; set; } = 40;
        public int Priority { get; set; } = 1;

        public string Content { get; set; } = "";

        // ===== Runtime state (KHÔNG lưu DB) =====

        [Ignore]
        public bool IsInside { get; set; }

        [Ignore]
        public bool IsNear { get; set; }

        [Ignore]
        public DateTime LastTriggered { get; set; } = DateTime.MinValue;
    }
}

