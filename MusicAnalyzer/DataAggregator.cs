using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Data;

namespace SongMarker
{
    class SongAddedEventArgs : EventArgs
    {
        public string SongName { get; set; }
    }
    enum FrequencyState { Off, Min, Max};

    class DataAggregator
    {
        public event EventHandler<SongAddedEventArgs> SongAddedToDBEvent;
        CSQLiteConnect db = new CSQLiteConnect();
        bool TerminateProcessing;
        public bool IsSafeToQuit {get; set;}

        public void ProcessSong(string SongPath)
        {
            SongAddedEventArgs e = new SongAddedEventArgs();
            TransferDataSong tds;
            tds = SongMarkerFFT.MakeSongMark(SongPath);
            if (tds != null)
            {
                db.Insert(tds);
                e.SongName = tds.artist + '-' + tds.title;
                if (SongAddedToDBEvent != null)
                    SongAddedToDBEvent(this, e);
            }
            else
            {
                e.SongName=SongPath+"   can't be processed, see log";
                if (SongAddedToDBEvent != null)
                    SongAddedToDBEvent(this, e);
            }
        }
        public void ProcessManySongsParallel(string[] files, int DegreeOfParallelism)
        {
            IsSafeToQuit = false;
            ParallelOptions po = new ParallelOptions();
            if (DegreeOfParallelism < 1)
                DegreeOfParallelism = 1;
            po.MaxDegreeOfParallelism = DegreeOfParallelism;

                Parallel.ForEach(files, po, (currentfile, pls) =>
                    {
                        if (TerminateProcessing)
                        {
                            pls.Stop();
                            TerminateProcessing = false;
                        }
                        if (!pls.ShouldExitCurrentIteration)
                            ProcessSong(currentfile);
                    });
            IsSafeToQuit = true;
        }

        public void StopProcessing()
        {
            TerminateProcessing = true;
        }

        public bool ClearDB()
        {
            return db.ClearDB();
        }
   
        public  DataSet GetDataSet()
        {
            return db.GetDataSet();
        }

        public void SaveDataSet(DataTable table)
        {
            db.LoadDataSet(table);
        }

        public int[] GetSimilarSongs(int song_id, int MethodId=1, int NumberOfSongsToShow=20)
        {
            List<TransferDataSong> list = db.GetAllRecords();
            int size = list.Count;
            TransferDataFFT[] arr = new TransferDataFFT[size];
            int[] indexes=new int[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = list[i].DeserializeData();
                indexes[i] = list[i].id;
            }
            int song_number = Array.IndexOf(indexes,song_id);
            double[] sim = new double[size];
            for (int i = 0; i < size; i++)
            {
                sim[i] = GetSimilarity(arr[i], arr[song_number], MethodId);
            }
            Array.Sort(sim,indexes);
            return indexes.Skip(1).Take(NumberOfSongsToShow).ToArray();
        }

        public int[] GetSongsBySpectra(FrequencyState[] State, int NumberOfSongsToShow = 20)
        {
            List<TransferDataSong> list = db.GetAllRecords();
            int size = list.Count;
            TransferDataFFT[] arr = new TransferDataFFT[size];
            int[] Score = new int[size];
            int[] indexes = new int[size];
            for (int i = 0; i < size; i++)
            {
                arr[i] = list[i].DeserializeData();
                indexes[i] = list[i].id;
            }
            int FreqCount = (arr[0].meanF.Length - 1) / State.Length;
            for (int j = 0; j < State.Length; j++)
            {
                if (State[j] != FrequencyState.Off)
                {
                    double sum;
                    int[] ind = Enumerable.Range(0, size).ToArray();
                    double[] Freq = new double[size];
                    int le = j*FreqCount, re = le + FreqCount;
                    for (int i = 0; i < size; i++)
                    {
                        sum = 0;
                        for (int k = le; k < re; k++)
                        {
                            sum += arr[i].meanF[k] / FreqCount;
                        }
                        Freq[i] = sum;
                    }
                    if (State[j] == FrequencyState.Min)
                    {
                        Array.Sort(Freq, ind);
                        for (int i = 0; i < size; i++)
                        {
                            Score[ind[i]] += size - i - 1;
                        }
                    }
                    else
                    {
                        Array.Sort(Freq, ind);
                        for (int i = 0; i < size; i++)
                        {
                            Score[ind[i]] += i;
                        }
                    }
                }
            }

            Array.Sort(Score, indexes);
            Array.Reverse(indexes);
            return indexes.Take(NumberOfSongsToShow).ToArray();
        }

        public double GetSimilarity(TransferDataFFT sample1,TransferDataFFT sample2, int MethodId)
        {
            double res = 0;
            int FreqNumber = sample1.meanF.Length;
            switch (MethodId)
            {
                case 1:
                    for (int i = 0; i < FreqNumber; i++)
                    {
                        res += Math.Pow(sample1.meanF[i] - sample2.meanF[i], 2) / Math.Pow(sample1.meanF[i] + sample2.meanF[i],2);
                    }
                    break;
                case 2:
                    for (int i = 0; i < FreqNumber; i++)
                    {
                        res += Math.Pow(sample1.meanF[i] - sample2.meanF[i], 2) / Math.Pow(sample1.meanF[i] + sample2.meanF[i],2) +
                            Math.Pow(sample1.medianF[i] - sample2.medianF[i], 2) / Math.Pow(sample1.medianF[i] + sample2.medianF[i],2) +
                            Math.Pow(sample1.stdF[i] - sample2.stdF[i], 2) / Math.Pow(sample1.stdF[i] + sample2.stdF[i],2);
                    }
                    break;
                case 3:
                    res = 1;
                    for (int i = 0; i < FreqNumber; i++)
                    {
                        if (sample1.meanF[i] > sample2.meanF[i])
                            res *= sample1.meanF[i] / sample2.meanF[i];
                        else
                            res *= sample2.meanF[i] / sample1.meanF[i];
                    }
                    break;
                case 4:
                    int size = sample1.meanF.Length;
                    res = 1;
                    for (int i = 0; i < size; i++)
                    {
                        if (sample1.meanF[i] > sample2.meanF[i])
                            res *= sample1.meanF[i] / sample2.meanF[i];
                        else
                            res *= sample2.meanF[i] / sample1.meanF[i];
                    }
                    int[] indexes1=Enumerable.Range(1,size).ToArray();
                    int[] indexes2 = Enumerable.Range(1, size).ToArray();
                    double[] sig1 = sample1.meanF;
                    double[] sig2 = sample2.meanF;
                    Array.Sort(sig1,indexes1);
                    Array.Sort(sig2, indexes2);
                    int sum=1;
                    for (int i = 0; i < size; i++)
                    {
                        sum+=Math.Abs(indexes1[i]-indexes2[i]);
                    }
                    res=res*sum;
                    break;
            }
            
            return res;
        }

        public string GetSongPath(string artist, string title)
        {
            return db.GetPath(artist,title);
        }

        public void RemoveDuplicatesFromDB()
        {
            db.RemoveDuplicates();
        }

        public TransferDataFFT GetSongDataViaID(int id)
        {
            return db.GetSongData(id);
        }
    }
}
