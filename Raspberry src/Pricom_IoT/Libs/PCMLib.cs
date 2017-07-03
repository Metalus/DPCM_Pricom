using System;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Audio;

public class PCMLib
{
    private const string audioFilename = "PCM_original.wav";
    MediaElement media = new MediaElement();
    private MediaCapture capture;
    private InMemoryRandomAccessStream stream;

    public static bool Recording;

    private async Task<bool> init()
    {
        if (stream != null)
        {
            stream.Dispose();
        }

        if (capture != null)
        {
            capture.Dispose();
        }
        try
        {
           
            stream = new InMemoryRandomAccessStream();
            MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings
            {
                StreamingCaptureMode = StreamingCaptureMode.Audio,
            };
            capture = new MediaCapture();
            await capture.InitializeAsync(settings);
        }
        catch (Exception ex)
        {
            if (ex.InnerException != null && ex.InnerException.GetType() == typeof(UnauthorizedAccessException))
            {
                throw ex.InnerException;
            }
            throw;
        }
        return true;
    }

    public async void Record()
    {
        if (Recording) return;
        await init();

        var storageFile = await Windows.Storage.KnownFolders.MusicLibrary.CreateFileAsync(audioFilename, Windows.Storage.CreationCollisionOption.ReplaceExisting);
        var profile = MediaEncodingProfile.CreateWav(Windows.Media.MediaProperties.AudioEncodingQuality.Medium);
        
        await capture.StartRecordToStorageFileAsync(profile, storageFile);

        Recording = true;
    }


    public async void Stop()
    {
        await capture.StopRecordAsync();
        Recording = false;
    }

    public async Task Play(string Filename)
    {
        Windows.Storage.StorageFile storageFile = await KnownFolders.MusicLibrary.GetFileAsync(Filename);
        var file = await storageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
        media.SetSource(file, storageFile.ContentType);
        media.Play();
    }
}