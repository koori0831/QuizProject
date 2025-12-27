using System.Timers;
using QuizApp.Models;

namespace QuizApp.Services
{
    public enum GameStatus
    {
        Idle,       // 대기 중
        Playing,    // 게임 진행 중
        Success,    // 성공
        Fail        // 실패 (시간 초과)
    }

    public class GameService
    {
        private System.Timers.Timer _timer;
        private List<Question> _currentQuestions;
        private int _currentQuestionIndex;
        private int _timeRemaining;
        
        // 이벤트
        public event Action<int>? TimeUpdated;
        public event Action<GameStatus>? GameStatusChanged;
        public event Action<Question?>? QuestionChanged;

        public GameStatus CurrentStatus { get; private set; } = GameStatus.Idle;
        public Question? CurrentQuestion => 
            (CurrentStatus == GameStatus.Playing && _currentQuestions != null && _currentQuestionIndex < _currentQuestions.Count) 
            ? _currentQuestions[_currentQuestionIndex] 
            : null;

        public int TimeRemaining => _timeRemaining;

        public GameService()
        {
            _timer = new System.Timers.Timer(1000); // 1초마다 틱
            _timer.Elapsed += OnTimerElapsed;
            _currentQuestions = new List<Question>();
        }

        public void StartGame(Theme theme, int timeLimitSeconds = 60)
        {
            if (theme.Questions.Count < 3)
            {
                throw new InvalidOperationException("문제가 3개 미만인 테마로는 게임을 시작할 수 없습니다.");
            }

            // 랜덤하게 3문제 추출
            var random = new Random();
            _currentQuestions = theme.Questions.OrderBy(x => random.Next()).Take(3).ToList();
            
            _currentQuestionIndex = 0;
            _timeRemaining = timeLimitSeconds;
            
            CurrentStatus = GameStatus.Playing;
            GameStatusChanged?.Invoke(CurrentStatus);
            QuestionChanged?.Invoke(CurrentQuestion);
            TimeUpdated?.Invoke(_timeRemaining);
            
            _timer.Start();
        }

        public void StopGame()
        {
            _timer.Stop();
            CurrentStatus = GameStatus.Idle;
            GameStatusChanged?.Invoke(CurrentStatus);
            QuestionChanged?.Invoke(null);
        }

        public void MarkCorrect()
        {
            if (CurrentStatus != GameStatus.Playing) return;

            _currentQuestionIndex++;

            if (_currentQuestionIndex >= _currentQuestions.Count)
            {
                // 모든 문제 해결 -> 성공
                EndGame(GameStatus.Success);
            }
            else
            {
                // 다음 문제
                QuestionChanged?.Invoke(CurrentQuestion);
            }
        }

        public void MarkFail()
        {
            EndGame(GameStatus.Fail);
        }

        private void EndGame(GameStatus status)
        {
            _timer.Stop();
            CurrentStatus = status;
            GameStatusChanged?.Invoke(CurrentStatus);
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            _timeRemaining--;
            TimeUpdated?.Invoke(_timeRemaining);

            if (_timeRemaining <= 0)
            {
                EndGame(GameStatus.Fail);
            }
        }
    }
}