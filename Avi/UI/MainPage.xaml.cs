using Avi.face;
using Avi.face.Animation;
using Avi.Services;
using Avi.UI;
using System.Diagnostics;

namespace Avi
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageAnim _anim = new();
        private CancellationTokenSource? _resizeCts;
        private readonly TaskManager? _taskManager;
        private readonly FaceAnimator? _faceAnimator;
        private readonly Face? _faceDrawable;
        private readonly FaceFadeAnimator _fadeAnimator;
        private readonly ISettingsService _settings;

        public MainPage(TaskManager taskManager, ISettingsService settings)
        {
            //ONLY FOR DEBUG
            //private int _emotionIndex = 0;
            //private readonly Emotion[] _emotions = (Emotion[])Enum.GetValues(typeof(Emotion));
            Debug.WriteLine("SUS");
            InitializeComponent();
            _taskManager = taskManager;
            _faceDrawable = FaceView;
            _settings = settings;
            _faceDrawable.MainText = "Sleep mode";
            _faceDrawable.SubText = "";
            _fadeAnimator = new FaceFadeAnimator(FaceView);
            _faceAnimator = new FaceAnimator(FaceView, _fadeAnimator);
            SizeChanged += MainPage_SizeChanged;
            LoadLambda();
            sec();
            SettingsContainer.Children.Add(new SettingsView(_settings));
        }
        protected override bool OnBackButtonPressed()
        {

            // np. zamknij _settings
            if (isOpen)
            {
                ToggleSettings();
                return true; // blokuje domyślne wyjście
            }

            return base.OnBackButtonPressed();
        }
        private void OpenSettings(object sender, EventArgs e)
        {
            ToggleSettings();
        }
        private async Task sec()
        {
#if ANDROID
            var statusWrite = await Permissions.RequestAsync<Permissions.StorageWrite>();
            var statusRead = await Permissions.RequestAsync<Permissions.StorageRead>();

            if (statusWrite != PermissionStatus.Granted || statusRead != PermissionStatus.Granted)
            {
                // Pokazać alert albo fallback
                Debug.WriteLine("Nie przyznano uprawnień do storage");
            }
#endif
        }
        bool isOpen;
        public async void ToggleSettings()
        {
            if (!isOpen)
            {
                SettingsContainer.IsVisible = true;
                Overlay.IsVisible = true;

                SettingsContainer.TranslationY = 1000;
                SettingsContainer.Opacity = 0;

                _ = Overlay.FadeTo(0.5, 200);

                await Task.WhenAll(
                    SettingsContainer.FadeTo(1, 250),
                    SettingsContainer.TranslateTo(0, 0, 350, Easing.CubicOut)
                );

                isOpen = true;
            }
            else
            {
                _ = Overlay.FadeTo(0, 200);

                await Task.WhenAll(
                    SettingsContainer.FadeTo(0, 200),
                    SettingsContainer.TranslateTo(0, 1000, 300, Easing.CubicIn)
                );

                SettingsContainer.IsVisible = false;
                Overlay.IsVisible = false;

                isOpen = false;
            }
        }

        private void LoadLambda()
        {
            _taskManager!.OnEmotion += emotion =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    _faceAnimator?.ApplyEmotionFromString(emotion);
                });
            };

            _taskManager.OnStatus += status =>
            {
                MainThread.BeginInvokeOnMainThread(() => _faceDrawable!.SubText = status);
            };

            _taskManager.OnMessage += token =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    OutputBox.Text += token;
                });
            };
            _taskManager.OnDebug += text =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    label.Text += "\n " + text;
                });
            };

            _faceDrawable!.MainTextClicked += () =>
            {
                if (!_taskManager.isLoaded)
                {
                    Load();
                    Debug.WriteLine("Started loading");
                }
            };
        }

        private void MainPage_SizeChanged(object? sender, EventArgs e)
        {
            _resizeCts?.Cancel();
            _resizeCts = new CancellationTokenSource();
            var token = _resizeCts.Token;

            _ = Task.Delay(50, token).ContinueWith(_ =>
            {
                if (token.IsCancellationRequested) return;

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    double scale = Math.Min(Width / Face.DesignWidth, Height / Face.DesignHeight);

                    FaceView.WidthRequest = Face.DesignWidth * scale;
                    FaceView.HeightRequest = Face.DesignHeight * scale;
                });
            }, token);
        }

        private async void Load()
        {
            //ONLY FOR DEBUG
            //var currentEmotion = _emotions[_emotionIndex];
            //_faceAnimator.ApplyEmotion(currentEmotion);
            //_emotionIndex = (_emotionIndex + 1) % _emotions.Length;
            
            _faceDrawable!.FadeOutBreathing(0.3f);
            _faceDrawable.MainText = "Loading";
            label.Text += "\n one sec loading bruh...";
            
            await _taskManager!.LoadAsync();
            await FadeOutTextAndShowFace();

            label.Text += "\n loaded :D";
            label.Text += "\n Btw mic is on :>";
        }

        private async Task FadeOutTextAndShowFace()
        {
            _faceDrawable.StartupMode = false;
            _faceDrawable.TextOpacity = 0f;
            //await _fadeAnimator.FadeTextOutAndShowFaceAsync(0.6f);
            _faceAnimator?.ApplyEmotion(Emotion.Neutral);
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            _taskManager?.Stop();
        }

        private void OnF1ToggleClicked(object sender, EventArgs e)
        {
            DEBUG2.IsVisible = !DEBUG2.IsVisible;
        }

        public void ToggleDebug()
        {
            OnF1ToggleClicked(this, EventArgs.Empty);
        }
    }
}