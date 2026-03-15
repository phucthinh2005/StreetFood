using Microsoft.Maui.Media;
using System.Globalization;
#if ANDROID
using MauiApp1.Platforms.Android; // thêm using này để sử dụng AudioFocusService
#endif

namespace MauiApp1.Services
{
    public class AudioService
    {
        public static AudioService Instance { get; } = new AudioService();

        private CancellationTokenSource? _cts;

#if ANDROID
        private AudioFocusService? _audioFocus; //thêm
#endif

        public bool IsPlaying { get; private set; }

        // user đang nghe manual
        public bool IsManualMode { get; private set; }

        public async Task Speak(string text, bool manual = false)
        {
            if (IsPlaying)
                return;

#if ANDROID
            _audioFocus ??= new AudioFocusService();

            if (!_audioFocus.RequestFocus())
                return;
#endif

            _cts = new CancellationTokenSource();
            IsPlaying = true;

            if (manual)
                IsManualMode = true;

            try
            {
                var locales = await TextToSpeech.Default.GetLocalesAsync();

                var culture = CultureInfo.CurrentUICulture.Name;
                var lang = culture.Substring(0, 2);

                var voice =
                    locales.FirstOrDefault(l => l.Language == culture)
                    ?? locales.FirstOrDefault(l => l.Language.StartsWith(lang));

                await TextToSpeech.Default.SpeakAsync(
                    text,
                    new SpeechOptions
                    {
                        Locale = voice,
                        Volume = SettingsService.Volume / 100.0f,
                        Rate = (float)SettingsService.SpeechSpeed
                    },
                    _cts.Token
                );
            }
            catch
            {
            }
            finally
            {
                IsPlaying = false;

#if ANDROID
                _audioFocus?.AbandonFocus(); //thêm
#endif

                if (manual)
                    IsManualMode = false;
            }
        }

        public void Stop()
        {
            if (_cts == null)//thêm
                return;

            _cts?.Cancel();
            _cts = null;

#if ANDROID
            _audioFocus?.AbandonFocus(); //thêm
#endif

            IsPlaying = false;
            IsManualMode = false;
        }
    }
}