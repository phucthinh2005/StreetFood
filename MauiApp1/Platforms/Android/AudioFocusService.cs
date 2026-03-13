//using global::Android.Content;
//using global::Android.Media;
//using MauiApp1.Services;

//namespace MauiApp1.Platforms.Android
//{
//    public class AudioFocusService : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
//    {
//        private readonly AudioManager? _audioManager;

//        public AudioFocusService()
//        {
//            var context = global::Android.App.Application.Context;
//            _audioManager = context.GetSystemService(Context.AudioService) as AudioManager;
//        }

//        public bool RequestFocus()
//        {
//            if (_audioManager == null)
//                return false;

//            var result = _audioManager.RequestAudioFocus(
//                this,
//                global::Android.Media.Stream.Music,
//                AudioFocus.GainTransient);

//            return result == AudioFocusRequest.Granted;
//        }

//        public void AbandonFocus()
//        {
//            _audioManager?.AbandonAudioFocus(this);
//        }

//        public void OnAudioFocusChange(AudioFocus focusChange)
//        {
//            switch (focusChange)
//            {
//                case AudioFocus.Loss:
//                case AudioFocus.LossTransient:
//                case AudioFocus.LossTransientCanDuck:

//                    // 🔴 có notification → dừng TTS
//                    MainThread.BeginInvokeOnMainThread(() =>
//                    {
//                        AudioService.Instance.Stop();
//                    });

//                    break;
//            }
//        }
//    }
//}