using MauiApp1.Models;
using Microsoft.Maui.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiApp1.ViewModels
{
    public class POIDetailViewModel : INotifyPropertyChanged
    {
        private POI? _selectedPOI;
        private CancellationTokenSource? _cts;
        private bool _isSpeaking;

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
            IsSpeaking ? "Dừng" : "Nghe ngay";

        public ICommand SpeakCommand { get; }
        public ICommand CloseCommand { get; }

        public POIDetailViewModel()
        {
            SpeakCommand = new Command(async () => await OnSpeak());
            CloseCommand = new Command(async () => await OnClose());
        }

        private async Task OnSpeak()
        {
            if (SelectedPOI == null || string.IsNullOrEmpty(SelectedPOI.Detail))
                return;

            // Nếu đang nói → dừng
            if (IsSpeaking)
            {
                _cts?.Cancel();
                IsSpeaking = false;
                return;
            }

            _cts = new CancellationTokenSource();
            IsSpeaking = true;

            try
            {
                await TextToSpeech.SpeakAsync(
                    SelectedPOI.Detail,
                    cancelToken: _cts.Token
                );
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                IsSpeaking = false;
            }
        }

        private async Task OnClose()
        {
            _cts?.Cancel();
            await Shell.Current.GoToAsync("..");
        }

        public void Stop()
        {
            _cts?.Cancel();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}