using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pricom_IoT.Libs
{
    public enum PreditorCorrelacoes
    {
        // Valores de correlação já definidos para voz humana
        // Obtidos no livro do BP Lathi 4ed.
        VozHumana,      
    }

    /// <summary>
    /// Classe para DPCM
    /// </summary>
    public class DPCM_Calc
    {


        public double[] DPCMEncoding(double[] pcm, double[] particao, double[] codebook, Vector<double> Preditor)
        {
            Vector<double> x = Vector.Build.Dense(3); // Carrega as amostras passadas
            double[] dpcm = new double[pcm.Length];

            //                       e                   dpcm
            //   Input signal   -->+-----Quantization------|-------
            //                     ^-                      V
            //                     |---------------------->+
            //                 Out |<----Predictor<-x------| Old

            double Out;
            double Old;
            double e;
            for (uint i = 0; i < pcm.Length; i++)
            {

                Out = Preditor * x; // Out = Preditor[0]y[k-1]+Preditor[1]y[k-2]+Preditor[2]y[k-3]+
                e = pcm[i] - Out;

                // Quantizar o erro
                int NivelDecisao = indexQuant(particao, e);
                dpcm[i] = codebook[NivelDecisao]; //Pegar o valor quantizado

                Old = Out + dpcm[i];
                x = Vector<double>.Build.DenseOfArray(new double[] { Old, x[0], x[1] });
            }
            return dpcm;
        }

        public Vector<double> GerarPreditor(PreditorCorrelacoes TipoPreditor, double Msq)
        {
            Vector<double> Preditor;
            switch (TipoPreditor)
            {
                case PreditorCorrelacoes.VozHumana:
                    {
                        Matrix<double> Rij = Matrix<double>.Build
                            .DenseOfArray(new double[,]
                            { { Msq,  0.825 * Msq, 0.562 * Msq },
                            { 0.825 * Msq, Msq, 0.825 * Msq },
                            { 0.562 * Msq, 0.825 * Msq, Msq } }); 

                        Vector<double> R0 = Vector<double>.Build.DenseOfArray(new double[] { 0.825 * Msq, 0.562 * Msq, 0.308 * Msq });
                        Preditor = Rij.Inverse() * R0;   //Func. transf. do preditor
                    };
                    break;

                default:
                    Preditor = Vector.Build.Dense(new double[] { 1,0,0 }); // Modulação delta 
                    break;
            }

            return Preditor;
        }


        int indexQuant(double[] particao, double valor)
        {
            for (int i = 0; i < particao.Length; i++)
                if (particao[i] > valor) return i;

            // Caso o valor esteja superior ao último nível de decisão
            return particao.Length - 1;
        }

        public double meansqr(double[] values)
        {
            double r = 0;
            for (int i = 0; i < values.Length; i++)
                r += values[i] * values[i];
            // return Math.Sqrt(r / values.Length);
            return r / values.Length;
        }

        double[] GerarTempo(double max, int TotalElementos)
        {
            double[] t = new double[TotalElementos];
            double inc = max / (TotalElementos - 1);
            //t[0] = 0;
            for (uint i = 1; i < TotalElementos; i++)
                t[i] = i * inc;
            return t;
        }

        public void PreQuantizar(float min, float max, uint TotalSize, out double[] particao, out double[] codebook)
        {
            codebook = new double[TotalSize + 1];
            particao = new double[TotalSize];
            codebook[0] = particao[0] = min;
            double erro = (max - min) / TotalSize; // Δ = 2Mp/L -> Erro de quantização
            for (int i = 1; i < TotalSize; i++)
            {
                codebook[i] += codebook[i - 1] + erro;
                particao[i] += particao[i - 1] + erro;
            }
            codebook[TotalSize] = codebook[TotalSize - 1] + erro;
        }

        public double[] DPCMDecoding(double[] dpcm, double[] particao, double[] codebook, Vector<double> Preditor)
        {
            //  dpcm
            //------ + -----------------------|---> Sinal Decodificado
            //       ^                        V
            //   Out |<----Predictor<--x------| 

            Vector<double> x = Vector.Build.Dense(3); // Parte do preditor
            double Out;

            double[] SinalDecodificado = new double[dpcm.Length];

            for (int i = 0; i < dpcm.Length; i++)
            {
                Out = Preditor * x;
                SinalDecodificado[i] = dpcm[i] + Out;

                x = Vector<double>.Build.DenseOfArray(new double[] { SinalDecodificado[i], x[0], x[1] });
            }

            return SinalDecodificado;
        }

    }
}
