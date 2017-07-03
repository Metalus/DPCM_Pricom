using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Gpio;
using Pricom_IoT.Libs;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Windows.Storage;
using Windows.UI.Core;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Pricom_IoT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DPCM_Calc dpcmLib = new DPCM_Calc();
        Vector<double> PreditorLeft;
        Vector<double> PreditorRight;
        WavHeader header;
        double[] left, right, particao, codebook;
        double[] dpcmLeft, dpcmRight;


        const uint sizeQuantizer = 256;// 8 Bits por sample
        public MainPage()
        {
            this.InitializeComponent();
        }

        public PCMLib Library = new PCMLib();

        private void CodeDpcm_Click(object sender, RoutedEventArgs e)
        {
            lock (this)
            {
                CodDpcm.Label = "Codificando...";
                var task = Task.Run(async () =>
                {
                    string folder = (await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music)).SaveFolder.Path;

                    header = WavController.OpenWavFile(Path.Combine(folder, "PCM_original.wav"), out left, out right);

                    dpcmLib.PreQuantizar(-1, 1, sizeQuantizer, out particao, out codebook);
                    double MsqLeft = dpcmLib.meansqr(left);
                    //double MsqRight = dpcmLib.meansqr(right);
                    PreditorLeft = dpcmLib.GerarPreditor(PreditorCorrelacoes.VozHumana, MsqLeft); // Coeficientes do preditor esquerdo
                    //PreditorRight = dpcmLib.GerarPreditor(PreditorCorrelacoes.VozHumana, MsqRight); // Coeficientes do preditor esquerdo

                    dpcmLeft = dpcmLib.DPCMEncoding(left, particao, codebook, PreditorLeft);
                    //dpcmRight = dpcmLib.DPCMEncoding(right, particao, codebook, PreditorRight);
                    WavController.SaveWavFile(Path.Combine(folder, "DPCM.wav"), dpcmLeft, dpcmLeft, 8, header);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => CodDpcm.Label = "Codificar");
                });
                
                
            }
        }

        private async void PlayDpcm_Click(object sender, RoutedEventArgs e)
        {
            await Library.Play("DPCM.wav");
        }

        private void DecodeDpcm_Click(object sender, RoutedEventArgs e)
        {
            lock (this)
            {
                DecodDpcm.Label = "Decodificando...";
                var task = Task.Run(async () =>
                {
                    string folder = (await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music)).SaveFolder.Path;
                    double[] decodedLeft = dpcmLib.DPCMDecoding(dpcmLeft, particao, codebook, PreditorLeft);
                    //double[] decodedRight = dpcmLib.DPCMDecoding(dpcmRight, particao, codebook, PreditorRight);
                    WavController.SaveWavFile(Path.Combine(folder, "Decodificado.wav"), decodedLeft, decodedLeft, 16, header);
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => DecodDpcm.Label = "Decodificar");
                });

            }
        }



        private async void PlayDecode_Click(object sender, RoutedEventArgs e)
        {
            await Library.Play("Decodificado.wav");
        }

        //AudioHandle audio;
        private void Record_Click(object sender, RoutedEventArgs e)
        {
            if (PCMLib.Recording)
            {
                Record.Label = "Gravar";
                Library.Stop();
                Record.Icon = new SymbolIcon(Symbol.Memo);
            }
            else
            {
                Record.Label = "Gravando";
                Library.Record();
                Record.Icon = new SymbolIcon(Symbol.Microphone);

            }
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            await Library.Play("PCM_original.wav");
        }
    }
}
