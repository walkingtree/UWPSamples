using LoadDataFromUSB.Helpers;
using MessagingServiceExtension;
using System;
using System.ComponentModel;
using System.IO;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LoadDataFromUSB
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        const string ImageDataPath = @"data\walkingtreelogo.jpg";
        const string VideoDataPath = @"data\b.mp4";
        StorageManager manager { get; set; }
        UserData LocalDataFolder { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string VideoPath;
        private string imagePath;
        public string ImagePath
        {
            get { return imagePath; }
            set
            {
                imagePath = value;
                RaisPropertyChanged("ImagePath");
            }
        }

        public MainPage()
        {
            InitializeComponent();
            DataContext = this;
            manager = new StorageManager();
            LocalDataFolder = manager.GetLocalStorageFolder();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            bool parameter = (bool)e.Parameter;
            if (parameter)
            {
                MessagingService.SubscribeToMessage<string>(this, "CopyData", sender =>
                {
                    ImagePath = Path.Combine(LocalDataFolder.Path, ImageDataPath);
                    VideoPath = Path.Combine(LocalDataFolder.Path, VideoDataPath);
                    VideoView.Source = new Uri(VideoPath);
                });
            }
            else
            {
                ImagePath = Path.Combine(LocalDataFolder.Path, ImageDataPath);
                VideoPath = Path.Combine(LocalDataFolder.Path, VideoDataPath);
                VideoView.Source = new Uri(VideoPath);
            }
            Progress.IsActive = false;
        }
    }
}