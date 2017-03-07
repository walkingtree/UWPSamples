using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace LoadDataFromUSB.Helpers
{
    class USBManager
    {
        StorageManager manager = new StorageManager();
        const string PROMPTMESSAGE = "Please insert the USB drive that containing data and then press OK to continue";
        const string MESSAGE = "Please check if the USB drive has been inserted correctly and that it contains the files.";
        const int OK = 0;
        const int EXIT = 0;
        const int RETRY = 1;
        
        public async Task SetUpAppDataFromExternalStorrage()
        {
            //if external storage is connected with device then no need to show message so we just need to copy the files
            var externalStorageFolder = await manager.GetFolderFromExternalStorage();
            if (externalStorageFolder != null)
            {
                InitSetupAppdata(externalStorageFolder);
                return;
            }

            //show a prompt to end user to insert a memory stick and try again to read
            var messageResult = await MessageBox(PROMPTMESSAGE).ShowAsync();

            if (OK != (int)messageResult.Id) return;

            //again trying to read the folder from external storage
            externalStorageFolder = await manager.GetFolderFromExternalStorage();
            if (externalStorageFolder != null)
                InitSetupAppdata(externalStorageFolder);
            else
               ShowCopySetUpAppDataFailedMessage();
        }

        private async void ShowCopySetUpAppDataFailedMessage()
        {
            //ProgressDialog?.Hide();
            var messageResult = await MessageBox(MESSAGE, true).ShowAsync();

            if (EXIT == (int)messageResult.Id)
                Application.Current.Exit();
            else
            if (RETRY == (int)messageResult.Id)
                await SetUpAppDataFromExternalStorrage();
        }

        private MessageDialog MessageBox(string message, bool isError = false)
        {
            var showDialog = new MessageDialog(message);
            if (!isError)
                showDialog.Commands.Add(new UICommand("OK") { Id = OK });
            else
            {
                showDialog.Commands.Add(new UICommand("Exit") { Id = EXIT });
                showDialog.Commands.Add(new UICommand("Retry") { Id = RETRY });
            }
            showDialog.DefaultCommandIndex = 0;
            return showDialog;
        }

        private async void InitSetupAppdata(UserData externalStorageFolder)
        {
            if (!await SetupLocalAppData(externalStorageFolder))
                ShowCopySetUpAppDataFailedMessage();
        }

        private async Task<bool> SetupLocalAppData(UserData fromFolder)
        {
            var isDoneSetUp = false;
            //await SetUpProgressBar(fromFolder);
            var toFolder = ApplicationData.Current.LocalFolder;
            isDoneSetUp = await manager.SetupLocalAppData(fromFolder, new UserData { Path = toFolder.Path, Name = toFolder.Name });
            return isDoneSetUp;
        }
    }
}
