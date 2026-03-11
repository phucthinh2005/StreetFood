using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiApp1.Models;
using MauiApp1.Resources.Languages;
using MauiApp1.Services;

namespace MauiApp1.ViewModels
{
    public class POIDetailViewModel : INotifyPropertyChanged
    {
        private POI? _selectedPOI;
        private bool _isSpeaking;

        private readonly AudioService _audioService = AudioService.Instance;

        public POI? SelectedPOI
        {
            get => _selectedPOI;
            set
            {
                _selectedPOI = value;
                OnPropertyChanged();
            }
        }

        public bool IsSpeaking
        {
            get => _isSpeaking;
            set
            {
                _isSpeaking = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SpeakButtonText));
            }
        }

        public string SpeakButtonText =>
            IsSpeaking ? AppResources.Stop : AppResources.ListenNow;

        public ICommand SpeakCommand { get; }
        public ICommand CloseCommand { get; }

        public POIDetailViewModel()
        {
            SpeakCommand = new Command(async () => await OnSpeak());
            CloseCommand = new Command(async () => await OnClose());
        }

        private async Task OnSpeak()
        {
            if (SelectedPOI == null)
                return;

            if (_audioService.IsPlaying)
            {
                Stop();
                return;
            }

            IsSpeaking = true;

            await _audioService.Speak(SelectedPOI.Detail, true);

            IsSpeaking = false;
        }

        public void Stop()
        {
            _audioService.Stop();
            IsSpeaking = false;
        }

        private async Task OnClose()
        {
            Stop();
            await Shell.Current.GoToAsync("..");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}