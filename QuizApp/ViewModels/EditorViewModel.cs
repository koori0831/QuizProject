using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using QuizApp.Core;
using QuizApp.Models;
using QuizApp.Services;

namespace QuizApp.ViewModels
{
    public class EditorViewModel : ObservableObject
    {
        private readonly DataService _dataService;
        private QuizData _quizData;
        private Theme? _selectedTheme;
        private Question? _selectedQuestion;
        private string _newThemeTitle = string.Empty;

        public ObservableCollection<Theme> Themes { get; private set; }
        public ObservableCollection<Question> Questions { get; private set; }

        public string NewThemeTitle
        {
            get => _newThemeTitle;
            set
            {
                if (SetProperty(ref _newThemeTitle, value))
                {
                     ((RelayCommand)AddThemeCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public Theme? SelectedTheme
        {
            get => _selectedTheme;
            set
            {
                if (SetProperty(ref _selectedTheme, value))
                {
                    LoadQuestions();
                    ((RelayCommand)AddQuestionCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteThemeCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public Question? SelectedQuestion
        {
            get => _selectedQuestion;
            set
            {
                if (SetProperty(ref _selectedQuestion, value))
                {
                    ((RelayCommand)DeleteQuestionCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)UpdateImageCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ICommand AddThemeCommand { get; }
        public ICommand DeleteThemeCommand { get; }
        public ICommand AddQuestionCommand { get; }
        public ICommand DeleteQuestionCommand { get; }
        public ICommand UpdateImageCommand { get; }
        public ICommand SaveCommand { get; }

        public EditorViewModel()
        {
            _dataService = new DataService();
            _quizData = new QuizData();
            Themes = new ObservableCollection<Theme>();
            Questions = new ObservableCollection<Question>();

            AddThemeCommand = new RelayCommand(AddTheme, _ => !string.IsNullOrWhiteSpace(NewThemeTitle));
            DeleteThemeCommand = new RelayCommand(DeleteTheme, _ => SelectedTheme != null);
            AddQuestionCommand = new RelayCommand(AddQuestion, _ => SelectedTheme != null);
            DeleteQuestionCommand = new RelayCommand(DeleteQuestion, _ => SelectedQuestion != null);
            UpdateImageCommand = new RelayCommand(UpdateImage, _ => SelectedQuestion != null);
            SaveCommand = new RelayCommand(SaveData);

            LoadData();
        }

        private async void LoadData()
        {
            _quizData = await _dataService.LoadDataAsync();
            Themes.Clear();
            foreach (var theme in _quizData.Themes)
            {
                Themes.Add(theme);
            }
        }

        private void LoadQuestions()
        {
            Questions.Clear();
            if (SelectedTheme != null)
            {
                foreach (var question in SelectedTheme.Questions)
                {
                    Questions.Add(question);
                }
            }
        }

        private void AddTheme(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(NewThemeTitle)) return;

            var newTheme = new Theme { Title = NewThemeTitle };
            _quizData.Themes.Add(newTheme);
            Themes.Add(newTheme);
            NewThemeTitle = string.Empty;
        }

        private void DeleteTheme(object? parameter)
        {
            if (SelectedTheme == null) return;

            _quizData.Themes.Remove(SelectedTheme);
            Themes.Remove(SelectedTheme);
            SelectedTheme = null;
        }

        private void AddQuestion(object? parameter)
        {
            if (SelectedTheme == null) return;

            var newQuestion = new Question { Answer = "새 문제", ImagePath = "" };
            SelectedTheme.Questions.Add(newQuestion);
            Questions.Add(newQuestion);
            SelectedQuestion = newQuestion;
        }

        private void DeleteQuestion(object? parameter)
        {
            if (SelectedTheme == null || SelectedQuestion == null) return;

            SelectedTheme.Questions.Remove(SelectedQuestion);
            Questions.Remove(SelectedQuestion);
            SelectedQuestion = null;
        }

        private void UpdateImage(object? parameter)
        {
            if (SelectedQuestion == null) return;

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                // 실행 파일 위치 기준 Data/Images 폴더에 저장
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string imagesFolder = Path.Combine(baseDir, "Data", "Images");
                
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                string fileName = Path.GetFileName(openFileDialog.FileName);
                // 파일명 중복 방지를 위해 GUID 추가
                string uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
                string destPath = Path.Combine(imagesFolder, uniqueFileName);

                File.Copy(openFileDialog.FileName, destPath, true);

                // 상대 경로로 저장 (Data/Images/파일명)
                // 실행 시에는 Converter가 절대 경로로 변환하여 표시
                string relativePath = Path.Combine("Data", "Images", uniqueFileName);
                SelectedQuestion.ImagePath = relativePath;
                
                // UI 갱신을 위해 리스트 재할당
                int index = Questions.IndexOf(SelectedQuestion);
                Questions[index] = SelectedQuestion;
                SelectedQuestion = Questions[index];
            }
        }

        private async void SaveData(object? parameter)
        {
            await _dataService.SaveDataAsync(_quizData);
            MessageBox.Show("저장되었습니다.");
        }
        
        public void RaiseCanExecuteChangedForCommands() {
             ((RelayCommand)AddThemeCommand).RaiseCanExecuteChanged();
        }
    }
}