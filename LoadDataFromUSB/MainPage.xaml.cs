using LoadDataFromUSB.Helpers;
using LoadDataFromUSB.Model;
using MessagingServiceExtension;
using Newtonsoft.Json;
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
        const string DataFolder = @"data\";
        const string JsonDataPath = @"userdata.json";
        StorageManager Manager { get; set; }
        UserData LocalDataFolder { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
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
                RaisePropertyChanged("ImagePath");
            }
        }

        public MainPage()
        {
            InitializeComponent();
            DataContext = this;
            LocalDataFolder = new StorageManager().GetLocalStorageFolder();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            bool parameter = (bool)e.Parameter;
            if (parameter)
            {
                MessagingService.SubscribeToMessage<string>(this, "CopyData", sender =>
                {
                    DisplayData();
                });
            }
            else
                DisplayData();

            Progress.IsActive = false;
        }

        private void DisplayData()
        {
            var JsonPath = Path.Combine(LocalDataFolder.Path, DataFolder, JsonDataPath);
            var textFromJson = File.ReadAllText(JsonPath);
            var data = JsonConvert.DeserializeObject<AppDetails>(textFromJson);

            ImagePath = Path.Combine(LocalDataFolder.Path, DataFolder, data.ImageFile);
            VideoView.Source = new Uri(Path.Combine(LocalDataFolder.Path, DataFolder, data.VideoFile));
            TextView.Text = data.ApplicationName;
        }
    }
}