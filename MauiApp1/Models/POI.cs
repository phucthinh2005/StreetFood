using System;
using System.Collections.Generic;
using System.Text;

namespace MauiApp1.Models
{
    public class POI
    {
        public string Name { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public double Radius { get; set; } = 20; // bán kính kích hoạt (mét)

        public string AudioFile { get; set; }

        public bool IsInside { get; set; }
    }
}
