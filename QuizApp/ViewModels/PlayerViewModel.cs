using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using QuizApp.Core;
using QuizApp.Models;
using QuizApp.Services;

namespace QuizApp.ViewModels
{
    public class PlayerViewModel : ObservableObject
    {
        private readonly GameService _gameService;
        private readonly DataService _dataService;
        private string _timerText;
        private string _currentQuestionImage;
        private string _resultMessage;
        private string _resultColor;
        private bool _isIdle;
        private bool _isPlaying;
        private bool _isResult;

        public ObservableCollection<Theme> Themes { get; private set; }

        public string TimerText
        {
            get => _timerText;
            set => SetProperty(ref _timerText, value);
        }

        public string CurrentQuestionImage
        {
            get => _currentQuestionImage;
            set => SetProperty(ref _currentQuestionImage, value);
        }

        public string ResultMessage
        {
            get => _resultMessage;
            set => SetProperty(ref _resultMessage, value);
        }

        public string ResultColor
        {
            get => _resultColor;
            set => SetProperty(ref _resultColor, value);
        }

        // 화면 상태 제어용 속성
        public bool IsIdle
        {
            get => _isIdle;
            set => SetProperty(ref _isIdle, value);
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        public bool IsResult
        {
            get => _isResult;
            set => SetProperty(ref _isResult, value);
        }

        public ICommand SelectThemeCommand { get; }
        public ICommand BackToMainCommand { get; }
        public ICommand OpenEditorCommand { get; }
        public ICommand ExitApplicationCommand { get; }

        public PlayerViewModel(GameService gameService)
        {
            _gameService = gameService;
            _dataService = new DataService();
            Themes = new ObservableCollection<Theme>();

            _timerText = "00:00";
            _currentQuestionImage = "";
            _resultMessage = "";
            _resultColor = "Black";
            
            // 초기 상태
            IsIdle = true;
            IsPlaying = false;
            IsResult = false;

            SelectThemeCommand = new RelayCommand(SelectTheme);
            BackToMainCommand = new RelayCommand(_ => ReturnToMain());
            OpenEditorCommand = new RelayCommand(OpenEditor);
            ExitApplicationCommand = new RelayCommand(_ => Application.Current.Shutdown());

            // 서비스 이벤트 구독
            _gameService.GameStatusChanged += OnGameStatusChanged;
            _gameService.TimeUpdated += OnTimeUpdated;
            _gameService.QuestionChanged += OnQuestionChanged;

            LoadThemes();
        }

        private void OpenEditor(object? parameter)
        {
            var editorWindow = new Views.EditorWindow();
            // 에디터 창이 닫힐 때 테마 목록 갱신
            editorWindow.Closed += (s, e) => ReloadThemes();
            editorWindow.ShowDialog();
        }

        private async void LoadThemes()
        {
            var data = await _dataService.LoadDataAsync();
            Themes.Clear();
            foreach (var theme in data.Themes)
            {
                Themes.Add(theme);
            }
        }

        private void SelectTheme(object? parameter)
        {
            if (parameter is Theme theme)
            {
                try
                {
                    _gameService.StartGame(theme);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "오류");
                }
            }
        }

        private void ReturnToMain()
        {
            _gameService.StopGame(); // 혹시 실행 중이라면 중지
            // 강제로 상태를 Idle로 변경 (서비스 이벤트가 발생하겠지만 확실히 하기 위해)
        }

        private void OnGameStatusChanged(GameStatus status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IsIdle = status == GameStatus.Idle;
                IsPlaying = status == GameStatus.Playing;
                IsResult = status == GameStatus.Success || status == GameStatus.Fail;

                if (IsResult)
                {
                    if (status == GameStatus.Success)
                    {
                        ResultMessage = "성공!";
                        ResultColor = "Green";
                    }
                    else
                    {
                        ResultMessage = "실패...";
                        ResultColor = "Red";
                    }
                }
            });
        }

        private void OnTimeUpdated(int timeRemaining)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TimerText = $"{timeRemaining / 60:00}:{timeRemaining % 60:00}";
            });
        }

        private void OnQuestionChanged(Question? question)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (question != null)
                {
                    CurrentQuestionImage = question.ImagePath;
                }
            });
        }
        
        // 데이터 리로드 메서드 (에디터 저장 후 갱신용)
        public void ReloadThemes()
        {
             LoadThemes();
        }
    }
}