using System.Windows;
using QuizApp.Services;
using QuizApp.ViewModels;
using QuizApp.Views;

namespace QuizApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private GameService _gameService;
    private HostViewModel _hostViewModel;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. 데이터 로드
        var dataService = new DataService();
        var data = await dataService.LoadDataAsync();
        
        // 데이터 없으면 더미 생성 (테스트용)
        if (data.Themes.Count == 0)
        {
            var dummyTheme = new Models.Theme { Title = "테스트 테마" };
            // 테스트를 위해 문제 3개 생성
            for (int i = 1; i <= 3; i++)
            {
                dummyTheme.Questions.Add(new Models.Question { ImagePath = "", Answer = $"정답{i}" });
            }
            data.Themes.Add(dummyTheme);
            await dataService.SaveDataAsync(data);
        }

        // 2. 서비스 초기화
        _gameService = new GameService();

        // 3. 진행자용 윈도우 생성 및 표시
        _hostViewModel = new HostViewModel(_gameService);
        var hostWindow = new HostWindow
        {
            DataContext = _hostViewModel
        };
        hostWindow.Show();

        // 4. 참가자용 윈도우 생성 및 표시
        var playerViewModel = new PlayerViewModel(_gameService);
        var playerWindow = new PlayerWindow
        {
            DataContext = playerViewModel
        };
        playerWindow.Show();

        // 5. 에디터 윈도우 생성 및 표시 (메인 화면에서 에디터 버튼 추가 예정, 지금은 테스트용으로 바로 띄움)
        // var editorWindow = new EditorWindow();
        // editorWindow.Show();
    }
}