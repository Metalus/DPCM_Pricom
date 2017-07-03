using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Media.Audio;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Media.Render;
using Windows.Media.Transcoding;
using Windows.Storage;

namespace Pricom_IoT.Libs
{
    public class AudioHandle
    {
        private AudioGraph graph;
        private AudioFileOutputNode pcmFileNode;
        private AudioDeviceOutputNode deviceOutputNode;
        private AudioDeviceInputNode deviceInputNode;
        private DeviceInformation outputDevice;

        public bool Recording { get; private set; }

        public async Task Init()
        {
            Recording = false;
            // Selecionar o dispositivo para gravar e reproduzir
            var devices = await DeviceInformation.FindAllAsync(MediaDevice.GetAudioRenderSelector());
            var devicesIn = await DeviceInformation.FindAllAsync(MediaDevice.GetAudioCaptureSelector());
            outputDevice = devices[0];
            // Configurações de gravações
            AudioGraphSettings settings = new AudioGraphSettings(AudioRenderCategory.Media)
            {
                //QuantumSizeSelectionMode = QuantumSizeSelectionMode.LowestLatency,
                PrimaryRenderDevice = outputDevice,
            };


            CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
            graph = result.Graph;

 
            deviceOutputNode = (await graph //Criar nó de saída (Reprodução no headset)
                                     .CreateDeviceOutputNodeAsync())
                                     .DeviceOutputNode;
            
            deviceInputNode = (await graph //Criar nó de entrada (Microfone) - Real-time communication
                                     .CreateDeviceInputNodeAsync(MediaCategory.Communications,graph.EncodingProperties,devicesIn[0]))
                                     .DeviceInputNode;

            // Criar o arquivo para ser armazenado o PCM gravado direto do microfone
            StorageFile pcmfile = await KnownFolders
                                       .MusicLibrary
                                       .CreateFileAsync("PCM_original.wav", Windows.Storage.CreationCollisionOption.ReplaceExisting);

            // PCM 16bits com 44,1kHZ com 96kbps
            MediaEncodingProfile profile = MediaEncodingProfile.CreateWav(Windows.Media.MediaProperties.AudioEncodingQuality.Medium);

            
            pcmFileNode = (await graph // Criar nó do arquivo de saída
                            .CreateFileOutputNodeAsync(pcmfile, profile))
                            .FileOutputNode;

            // Conectar os nós de reprodução e do arquivo PCM ao nó do microfone
            // Ou seja, passar os sinais para o fone reproduzir e o arquivo armazenar ao mesmo tempo
            deviceInputNode.AddOutgoingConnection(pcmFileNode);
            deviceInputNode.AddOutgoingConnection(deviceOutputNode);
        }

        public async Task ToggleRecordStop()
        {
            if (!Recording) //Se não estiver gravando
            {
                graph.Start();
            }
            else if (Recording) // Se já estiver gravando
            {
                graph.Stop(); //Parar de gravar e salvar o pcm
                await pcmFileNode.FinalizeAsync();
            }
            Recording = !Recording;
        }
    }
}
