using SQLite;                   // thư viện SQLite
using MauiApp1.Models;          // chứa class POI
using System.Text.Json;         // dùng để đọc JSON

namespace MauiApp1.Database
{
    public class DatabaseService
    {
        // biến kết nối database SQLite
        private SQLiteAsyncConnection _database;

        // ===== Constructor =====
        public DatabaseService()
        {
            // tạo đường dẫn file database
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "poi.db");

            // nếu database đã tồn tại thì xóa
            if (File.Exists(dbPath))
                File.Delete(dbPath);   // xóa DB cũ

            // tạo kết nối SQLite
            _database = new SQLiteAsyncConnection(dbPath);

            // tạo bảng POI trong database
            _database.CreateTableAsync<POI>().Wait();
        }

        // ===== Lấy toàn bộ POI =====
        public Task<List<POI>> GetPOIsAsync()
        {
            // SELECT * FROM POI
            return _database.Table<POI>().ToListAsync();
        }

        // ===== Thêm POI vào database =====
        public Task<int> AddPOIAsync(POI poi)
        {
            // INSERT INTO POI
            return _database.InsertAsync(poi);
        }

        // ===== Import dữ liệu từ JSON =====
        public async Task ImportFromJson()
        {
            // lấy danh sách POI trong database
            var list = await GetPOIsAsync();

            // nếu database đã có dữ liệu thì không import nữa
            if (list.Count > 0)
                return;

            // mở file JSON trong Resources/Raw
            using var stream = await FileSystem.OpenAppPackageFileAsync("poi.json");

            // đọc file
            using var reader = new StreamReader(stream);

            // chuyển thành string
            var json = await reader.ReadToEndAsync();

            // chuyển JSON -> List<POI>
            var pois = JsonSerializer.Deserialize<List<POI>>(json);

            // in ra số lượng POI trong JSON
            Console.WriteLine($"JSON count: {pois?.Count}");

            // nếu JSON lỗi thì dừng
            if (pois == null)
                return;

            // thêm từng POI vào database
            foreach (var poi in pois)
            {
                await AddPOIAsync(poi);
            }
        }
    }
}