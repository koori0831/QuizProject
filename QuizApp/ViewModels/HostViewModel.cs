using System.Windows;
using System.Windows.Input;
using QuizApp.Core;
using QuizApp.Models;
using QuizApp.Services;

namespace QuizApp.ViewModels
{
    public class HostViewModel : ObservableObject
    {
        private readonly GameService _gameService;
        private string _currentStatusMessage;
        private string _timerText;
        private string _currentAnswer;
        private string _currentImage;
        private bool _canPass;

        public string CurrentStatusMessage
        {
            get => _currentStatusMessage;
            set => SetProperty(ref _currentStatusMessage, value);
        }

        public string TimerText
        {
            get => _timerText;
            set => SetProperty(ref _timerText, value);
        }

        public string CurrentAnswer
        {
            get => _currentAnswer;
            set => SetProperty(ref _currentAnswer, value);
        }

        public string CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        public bool CanPass
        {
            get => _canPass;
            set => SetProperty(ref _canPass, value);
        }

        public ICommand CorrectCommand { get; }
        public ICommand FailCommand { get; }
        public ICommand StartGameCommand { get; }
        public ICommand StopGameCommand { get; }
        public ICommand PassCommand { get; }

        // 테스트용: 게임 시작을 위한 테마 주입
        public Theme? TestTheme { get; set; }

        public HostViewModel(GameService gameService)
        {
            _gameService = gameService;
            
            _currentStatusMessage = "대기 중";
            _timerText = "00:00";
            _currentAnswer = "-";
            _currentImage = string.Empty;
            _canPass = false;

            _gameService.GameStatusChanged += OnGameStatusChanged;
            _gameService.TimeUpdated += OnTimeUpdated;
            _gameService.QuestionChanged += OnQuestionChanged;
            _gameService.PassStateChanged += OnPassStateChanged;

            CorrectCommand = new RelayCommand(_ => _gameService.MarkCorrect(), _ => _gameService.CurrentStatus == GameStatus.Playing);
            FailCommand = new RelayCommand(_ => _gameService.MarkFail(), _ => _gameService.CurrentStatus == GameStatus.Playing);
            PassCommand = new RelayCommand(_ => _gameService.PassCurrentQuestion(), _ => _gameService.CurrentStatus == GameStatus.Playing && !_gameService.IsPassUsed);
            
            // 테스트용: 임시 시작 커맨드 (실제로는 PlayerWindow에서 테마 선택 시 시작됨)
            StartGameCommand = new RelayCommand(_ => 
            {
                if (TestTheme != null) _gameService.StartGame(TestTheme);
            }, _ => TestTheme != null && _gameService.CurrentStatus == GameStatus.Idle);
            
            StopGameCommand = new RelayCommand(_ => _gameService.StopGame(), _ => _gameService.CurrentStatus == GameStatus.Playing);
        }

        private void OnPassStateChanged(bool isPassUsed)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((RelayCommand)PassCommand).RaiseCanExecuteChanged();
            });
        }

        private void OnGameStatusChanged(GameStatus status)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentStatusMessage = status switch
                {
                    GameStatus.Idle => "대기 중",
                    GameStatus.Playing => "게임 진행 중",
                    GameStatus.Success => "성공!",
                    GameStatus.Fail => "실패 (시간 초과)",
                    _ => ""
                };
                
                ((RelayCommand)CorrectCommand).RaiseCanExecuteChanged();
                ((RelayCommand)FailCommand).RaiseCanExecuteChanged();
                ((RelayCommand)PassCommand).RaiseCanExecuteChanged();
                ((RelayCommand)StartGameCommand).RaiseCanExecuteChanged();
                ((RelayCommand)StopGameCommand).RaiseCanExecuteChanged();
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
                    CurrentAnswer = question.Answer;
                    CurrentImage = question.ImagePath;
                }
                else
                {
                    CurrentAnswer = "-";
                    CurrentImage = string.Empty;
                }
            });
        }
    }
}