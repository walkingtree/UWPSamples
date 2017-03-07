using MessagingServiceExtension;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace LoadDataFromUSB.Helpers
{
    public class StorageManager
    {
        const string Data = "data";
        const string DataStoreFromUSB = "LoadDataFromUSB";
        
        public async Task<bool> IsDataAvailable()
        {
            var localAppDataFolderPath = ApplicationData.Current.LocalFolder;
            var folder = await localAppDataFolderPath.TryGetItemAsync(Data);
            return (folder != null);
        }

        public async Task<UserData> GetFolderFromExternalStorage()
        {
            var externalDevices = KnownFolders.RemovableDevices;
            var sdFolders = await externalDevices.GetFoldersAsync();
            foreach (var folder in sdFolders)
            {
                var externalStorage = await GetApplicationDirectory(folder);
                if (externalStorage != null) return externalStorage;
            }
            return null;
        }

        /// <summary>
        /// Get ActiveNomad folder from  external storages
        /// </summary>
        /// <param name="fromFolder"></param>
        /// <returns></returns>
        private async Task<UserData> GetApplicationDirectory(StorageFolder fromFolder)
        {
            if (FileAttributes.Directory != fromFolder.Attributes)
                return null;

            var projectDirectory = (StorageFolder)await fromFolder.TryGetItemAsync(DataStoreFromUSB);

            if (projectDirectory == null)
                return null;

            var filesCount = await GetFilesCount(projectDirectory);

            var userData = new UserData
            {
                Path = projectDirectory.Path,
                Name = projectDirectory.Name,
                FileCount = filesCount
            };

            return userData;
        }

        /// <summary>
        /// this method is used to copy app reelated files from external drive and 
        /// store into the local app data folder
        /// </summary>
        /// <param name="fromFolder">external derive folder</param>
        /// <param name="toFolder">local app data folder</param>
        /// <returns></returns>
        public async Task<bool> SetupLocalAppData(UserData fromFolder, UserData toFolder)
        {
            var isSetUpDone = false;
            if (await IsFolderExists(fromFolder, Data))
            {
                if (await CreateNewFolders(toFolder))
                    isSetUpDone = await CopyFilesIntoAppDataFolder(fromFolder, toFolder);
            }
            return isSetUpDone;
        }

        private async Task<bool> IsFolderExists(UserData fromFolder, string folderName)
        {
            var storageFolder = await StorageFolder.GetFolderFromPathAsync(fromFolder.Path);

            var folder = await storageFolder.TryGetItemAsync(folderName);
            return folder != null;
        }

        private async Task<bool> CreateNewFolders(UserData toFolder)
        {
            var storageFolder = await StorageFolder.GetFolderFromPathAsync(toFolder.Path);
            var isCreateNewFoldersDone = false;
            if (await CreateFolder(storageFolder, Data))
                isCreateNewFoldersDone = true;

            return isCreateNewFoldersDone;
        }

        private async Task<bool> CreateFolder(StorageFolder toFolder, string folderName)
        {
            bool isCreatFolderDone = false;
            var newFolder = await toFolder.CreateFolderAsync(folderName);
            if (newFolder != null) isCreatFolderDone = true;
            return isCreatFolderDone;
        }

        /// <summary>
        /// this method returns the total numbers of file going to copy
        /// </summary>
        /// <param name="fromFolder"></param>
        /// <returns></returns>
        public async Task<int> GetFilesCount(StorageFolder fromFolder)
        {
            var filesCount = 0;
            filesCount += await GetFilesCountFromAFolder(fromFolder, Data);
            return filesCount;
        }

        /// <summary>
        /// this method will return total number of file with in a folder
        /// </summary>
        /// <param name="fromFolder"></param>
        /// <param name="folderName"></param>
        /// <returns></returns>
        async Task<int> GetFilesCountFromAFolder(StorageFolder fromFolder, string folderName)
        {
            int filesCount = 0;
            var folder = (StorageFolder)await fromFolder.TryGetItemAsync(folderName);
            
            var files = await folder.GetFilesAsync();
            if (files != null)
                filesCount = files.Count;
                        
            return filesCount;
        }

        private async Task<bool> CopyFilesIntoAppDataFolder(UserData fromFolder, UserData toFolder)
        {
            var copyResult = await CopyData(fromFolder, toFolder); //TODO this should be done in parallel
            return copyResult;
        }

        public UserData GetLocalStorageFolder()
        {
            var storageFolder = ApplicationData.Current.LocalFolder;
            return new UserData
            {
                Name = storageFolder.Name,
                Path = storageFolder.Path
            };
        }

        //copy all the images file inside the app local imaages folder
        private async Task<bool> CopyData(UserData fromFolder, UserData toFolder)
        {
            var isCopyImagesDone = false;

            var fromImagesFolder = await GetInsideFolder(fromFolder, Data);

            var toImagesFolder = await GetInsideFolder(toFolder, Data);

            if (fromImagesFolder != null && toImagesFolder != null)
                isCopyImagesDone = await CopyFolders(fromImagesFolder, toImagesFolder);
            return isCopyImagesDone;
        }

        /// <summary>
        /// used to get the file folder from external storage which will contains the files which need to copy
        /// </summary>
        /// <param name="fromfolder">external folder reference "ActiveNomad"</param>
        /// <param name="folderName">instance of the file folder name like "settings, images"</param>
        /// <returns></returns>
        private async Task<StorageFolder> GetInsideFolder(UserData fromfolder, string folderName)
        {
            var storageFolder = await StorageFolder.GetFolderFromPathAsync(fromfolder.Path);

            return (StorageFolder)await storageFolder.TryGetItemAsync(folderName);
        }

        /// <summary>
        /// used to copy the files into the appdata localstate folder
        /// </summary>
        /// <param name="copyToFolder">folder ref in which files need to copy</param>
        /// <param name="copyFromFolder">folder ref from where files need to read </param>
        /// <returns></returns>
        private async Task<bool> CopyFolders(IStorageFolder copyFromFolder, IStorageFolder copyToFolder)
        {
            var files = await copyFromFolder.GetFilesAsync();
            if (files == null)
                return false;

            foreach (var file in files)
            {
                await file.CopyAsync(copyToFolder);
            }
            MessagingService.PublishMessage("Copy done", "CopyData");
            return true;
        }
    }
}