using FaceId.Services;

namespace FaceId
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "FaceId" };
        }

        protected override void OnResume()
        {
            base.OnResume();
            AppState.ShouldPromptBiometric = true;
        }

    }
}
