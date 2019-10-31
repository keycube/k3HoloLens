using System.IO;
using System.Threading.Tasks;

#if WINDOWS_UWP
using System;
using Windows.Storage;
#endif

public static class FileUtils
{
    public static async Task<string> ReadTextFile(string path)
    {
#if WINDOWS_UWP
        path = "Keycube/" + path;

        // For HoloLens, the root folder is 'Documents'
        StorageFolder storageFolderDocuments = KnownFolders.DocumentsLibrary;

        // For UWP, the path is written with '\' insteand of '/'
        path = path.Replace("/", "\\");

        StorageFile storageFile = await storageFolderDocuments.GetFileAsync(path);
        return await FileIO.ReadTextAsync(storageFile);
#else
        StreamReader streamReader = File.OpenText(path);
        string s = streamReader.ReadToEnd();
        streamReader.Close();
        return s;
#endif
    }

    public static async void AppendTextToFile(string path, string text)
    {
#if WINDOWS_UWP
        path = "Keycube/" + path;

        // For HoloLens, the root folder is 'Documents'
        StorageFolder storageFolderDocuments = KnownFolders.DocumentsLibrary;

        // For UWP, the path is written with '\' insteand of '/'
        path = path.Replace("/", "\\");

        StorageFile storageFile = await storageFolderDocuments.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);
        //StorageFile storageFile = await storageFolderDocuments.GetFileAsync(path);
        await FileIO.AppendTextAsync(storageFile, text);
#else
        File.AppendAllText(path, text);
#endif
    }
}
