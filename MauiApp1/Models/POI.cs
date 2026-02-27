using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp1.Models
{
    public class POI
    {
        public string Name { get; set; } = "";

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Radius { get; set; } = 20; // bán kính kích hoạt (mét)
        public double NearRadius { get; set; } = 40;  // Vùng "đến gần"
        public int Priority { get; set; } = 1;        // Mức ưu tiên
        
        public bool IsInside { get; set; }
        public string Content { get; set; } = "";   // Nội dung tự động phát

        public DateTime LastTriggered { get; set; } = DateTime.MinValue;

        public bool IsNear { get; set; }
    }
}
