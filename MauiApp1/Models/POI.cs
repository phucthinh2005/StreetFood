using System.Globalization;
using SQLite;

namespace MauiApp1.Models
{
    public class POI
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // ===== Name =====
        public string Name_vi { get; set; } = "";
        public string Name_en { get; set; } = "";

        // ===== Location =====
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string ImageUrl { get; set; } = "";

        // ===== Address =====
        public string Address_vi { get; set; } = "";
        public string Address_en { get; set; } = "";
        // ===== Description =====
        public string Description_vi { get; set; } = "";
        public string Description_en { get; set; } = "";

        // ===== Detail =====
        public string Detail_vi { get; set; } = "";
        public string Detail_en { get; set; } = "";

        // ===== Content =====
        public string Content_vi { get; set; } = "";
        public string Content_en { get; set; } = "";

        public double Radius { get; set; } = 10;
        public double NearRadius { get; set; } = 30;
        public int Priority { get; set; } = 1;

        // ===== Runtime =====
        [Ignore]
        public bool IsInside { get; set; }

        [Ignore]
        public DateTime LastTriggered { get; set; } = DateTime.MinValue;

        // ===== Property cho UI =====

        [Ignore]
        public string Name => GetName();

        [Ignore]
        public string Description => GetDescription();

        [Ignore]
        public string Content => GetContent();

        [Ignore]
        public string Detail => GetDetail();

        [Ignore]
        public string Address => GetAddress();

        // ===== Get Language =====

        public string GetName()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            return lang switch
            {
                "en" => Name_en,
                _ => Name_vi
            };
        }

        public string GetDescription()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            return lang switch
            {
                "en" => Description_en,
                _ => Description_vi
            };
        }

        public string GetContent()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            return lang switch
            {
                "en" => Content_en,
                _ => Content_vi
            };
        }

        public string GetDetail()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            return lang switch
            {
                "en" => Detail_en,
                _ => Detail_vi
            };
        }

        public string GetAddress()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            return lang switch
            {
                "en" => Address_en,
                _ => Address_vi
            };
        }

        [Ignore]
        public string Language
        {
            get
            {
                var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

                return lang switch
                {
                    "en" => "English",
                    "ja" => "日本語",
                    _ => "Tiếng Việt"
                };
            }
        }
    }
}