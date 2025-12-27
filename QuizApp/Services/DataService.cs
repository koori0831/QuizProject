using System.IO;
using System.Text.Json;
using QuizApp.Models;

namespace QuizApp.Services
{
    public class DataService
    {
        private const string DataFileName = "quiz_data.json";
        private string _dataFilePath;

        public DataService()
        {
            // 실행 파일 위치를 기준으로 데이터 폴더 설정 (Git 배포 용이)
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string dataFolder = Path.Combine(baseDir, "Data");
            
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }
            _dataFilePath = Path.Combine(dataFolder, DataFileName);
        }

        public async Task SaveDataAsync(QuizData data)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(data, options);
            await File.WriteAllTextAsync(_dataFilePath, jsonString);
        }

        public async Task<QuizData> LoadDataAsync()
        {
            if (!File.Exists(_dataFilePath))
            {
                return new QuizData();
            }

            string jsonString = await File.ReadAllTextAsync(_dataFilePath);
            try
            {
                return JsonSerializer.Deserialize<QuizData>(jsonString) ?? new QuizData();
            }
            catch
            {
                return new QuizData();
            }
        }
    }
}