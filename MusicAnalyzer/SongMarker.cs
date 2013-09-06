using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Meta.Numerics;
using Meta.Numerics.SignalProcessing;
using Meta.Numerics.Statistics;
using NAudio;
using NAudio.Wave;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TagLib;

namespace SongMarker
{
     class SongMarkerFFT
    {
        private const int _TimeToProcess=300; // Time for song analyze, s
        private const int _NumberOfFrames=20; // Number of song pieces for fft averaging
        private const int _FrameSize=131072; // Length of FFT, power of 2
        private const int _NumberOfFreqIntervals=10; // Number of frequency bands between 32 and 8400 Hz

        /// <summary>
        /// Makes FFT of music file, calculates stats in wide frequency range and stores data in special class 
        /// </summary>
        /// <param name="SongPath">Path of the music file</param>
        static public TransferDataSong MakeSongMark(string SongPath)
        {
            try
            {
                using (AudioFileReader rdr = new AudioFileReader(SongPath))
                {
                    return ReadAndProcess(rdr, SongPath);
                }
            }
            catch (Exception Ex)
            {
                using (StreamWriter w = System.IO.File.AppendText("MusicProcessingLog.txt"))
                {
                    w.WriteLine(SongPath + Environment.NewLine + Ex.Message);
                }
                return null;
            }
        }

        static TransferDataSong ReadAndProcess(AudioFileReader reader, string SongPath)
        {
            FourierTransformer ft = new FourierTransformer(_FrameSize); 
            Complex[] temp = new Complex[_FrameSize];
            double[] Amp = new double[_FrameSize];
            double[] F = new double[_FrameSize];
            double[] sBass = LinSpace(16, 60, _NumberOfFreqIntervals).ToArray();
            double[] Bass = LinSpace(60, 250, _NumberOfFreqIntervals).ToArray();
            double[] Mid = LinSpace(250, 2000, _NumberOfFreqIntervals).ToArray();
            double[] HighMid = LinSpace(2000, 6000, _NumberOfFreqIntervals).ToArray();
            double[] High = LinSpace(6000, 15000, _NumberOfFreqIntervals).ToArray();
            double[] MainFreqs = sBass.Concat(Bass).Concat(Mid).Concat(HighMid).Concat(High).Distinct().ToArray();
            double[] MeanFreqs = new double[MainFreqs.Length - 1];
            double[] MedianFreqs = new double[MainFreqs.Length - 1];
            double[] StdFreqs = new double[MainFreqs.Length - 1];
            Sample FreqSample = new Sample();
            double sum;
            for (int i = 0; i < _FrameSize; i++)
            {
                F[i] = (double)i / (_FrameSize / 2 + 1) * reader.WaveFormat.SampleRate / 2;
            }
            int[] FreqIndRange = new int[MainFreqs.Length];
            for (int i = 0; i < MainFreqs.Length; i++)
            {
                int j = 0;
                while (F[j] < MainFreqs[i])
                    j++;
                FreqIndRange[i] = j;
            }
            // Create mediumsize array for reading from stream 
            int sampleCount = Math.Min(Convert.ToInt32(reader.Length / sizeof(float) / _NumberOfFrames),
                Convert.ToInt32(reader.WaveFormat.SampleRate * _TimeToProcess *2/ _NumberOfFrames));
            if (sampleCount < _FrameSize * 2)
                sampleCount = _FrameSize * 2;
            float[] buffer = new float[sampleCount];

            for (int m = 0; m < _NumberOfFrames; m++)
            {
                reader.Read(buffer, 0, sampleCount);
                for (int k = 0; k < temp.Length; k++)
                {
                    temp[k] = buffer[2 * k ];
                }
                Complex[] FFTresults = ft.Transform(temp);
                for (int i = 0; i < _FrameSize; i++)
                {
                    Amp[i] = Meta.Numerics.ComplexMath.Abs(FFTresults[i]);
                }
                sum = Amp.Sum();
                Amp = Amp.Select(x => x / sum).ToArray();
                for (int s = 0; s < MainFreqs.Length - 1; s++)
                {
                    FreqSample.Clear();
                    double[] PartAmp = new double[FreqIndRange[s + 1] - FreqIndRange[s]];
                    Array.Copy(Amp, FreqIndRange[s], PartAmp, 0, PartAmp.Length);
                    FreqSample.Add(PartAmp);
                    MeanFreqs[s] += FreqSample.Mean / _NumberOfFrames;
                    MedianFreqs[s] += FreqSample.Median / _NumberOfFrames;
                    StdFreqs[s] += FreqSample.StandardDeviation / _NumberOfFrames;
                }

            }
            for (int i = 0; i < MeanFreqs.Length; i++)
            {
                if (Double.IsNaN(MeanFreqs[i]))
                    throw new System.Exception("Results of FFT is NaN");
            }
            MemoryStream DataStream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(DataStream, new TransferDataFFT(MeanFreqs, MedianFreqs, StdFreqs));
            //DataStream.Position = 0;
            //TransferDataFFT TransferStruct2 = (TransferDataFFT)formatter.Deserialize(DataStream);
            TransferDataSong tds = new TransferDataSong();
            tds.data = DataStream.ToArray();
            tds.path = SongPath;
            TimeSpan t = TimeSpan.FromSeconds(reader.TotalTime.TotalSeconds);
            tds.duration = string.Format("{0:D2}:{1:D2}",t.Minutes, t.Seconds);
            TagLib.ByteVector.UseBrokenLatin1Behavior = true;
            using (TagLib.File tagFile = TagLib.File.Create(SongPath))
            {
                tds.title = tagFile.Tag.Title;
                tds.artist = tagFile.Tag.FirstPerformer;
            }
            DataStream.Close();
            return tds;
        }

        /// <summary>
        /// Returns logarithmically spaced array of doubles
        /// </summary>
        /// <param name="StartValue">Starting value</param>
        /// <param name="EndtValue">Ending value</param>
        /// <param name="Nbins">Length of resulting array</param>
        public static double[] LogSpace(double StartValue, double EndValue, int Nbins)
        {
            double[] SpacedArray = new double[Nbins];
            double step = (Math.Log10(EndValue) - Math.Log10(StartValue)) / (Nbins - 1);
            for (int i = 0; i < Nbins; i++)
            {
                SpacedArray[i] = Math.Pow(10, (Math.Log10(StartValue) + i * step));
            }
            SpacedArray[Nbins - 1] = EndValue;
            return SpacedArray;
        }

        /// <summary>
        /// Returns linearily spaced array of doubles
        /// </summary>
        /// <param name="StartValue">Starting value</param>
        /// <param name="EndtValue">Ending value</param>
        /// <param name="Nbins">Length of resulting array</param>
        public static double[] LinSpace(double StartValue, double EndValue, int Nbins)
        {
            double[] SpacedArray = new double[Nbins];
            double step = (EndValue - StartValue) / (Nbins - 1);
            for (int i = 0; i < Nbins; i++)
            {
                SpacedArray[i] = StartValue + i * step;
            }
            SpacedArray[Nbins - 1] = EndValue;
            return SpacedArray;
        }

    }

    [Serializable]
    class TransferDataFFT
    {
        public double[] meanF { get; set;}
        public double[] medianF { get; set;}
        public double[] stdF { get; set; }
        public TransferDataFFT() { } 
        public TransferDataFFT(double[] MeanF, double[] MedianF, double[] StdF)
        {
            meanF = MeanF;
            medianF = MedianF;
            stdF = StdF;
        }
    }
    class TransferDataSong
    {
        public int id { get; set; }
        public byte[] data { get; set;}
        public string title { get; set; }
        public string artist { get; set; }
        public string path { get; set; }
        public string duration { get; set; }
        public TransferDataFFT DeserializeData()
        {
            MemoryStream ms = new MemoryStream(this.data);
            ms.Position = 0;
            BinaryFormatter formatter = new BinaryFormatter();
            return (TransferDataFFT)formatter.Deserialize(ms);
        }
    }
}
