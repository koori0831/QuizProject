using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace QuizApp.Converters
{
    public class PathToFullPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path && !string.IsNullOrEmpty(path))
            {
                try
                {
                    // 이미 절대 경로라면 그대로 반환
                    if (Path.IsPathRooted(path))
                    {
                        if (File.Exists(path)) return path;
                    }

                    // 상대 경로라면 실행 폴더 기준으로 변환
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    string fullPath = Path.GetFullPath(Path.Combine(baseDir, path));

                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
                catch
                {
                    // 경로 변환 중 오류 발생 시 무시
                }
            }
            return null; // 이미지가 없으면 null 반환
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}