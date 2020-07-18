using System;
using Google.Cloud.Speech.V1;
using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MediaToolkit.Model;
using MediaToolkit;
using MediaToolkit.Options;


namespace audio_transcriber
{
    class Program
    {
        [DllImport("winmm.dll")] 
        private static extern uint mciSendString(
            string command,
            StringBuilder returnValue,
            int returnLength,
            IntPtr winHandle);
        [DllImport("opus32-float-avx.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr opus_encoder_create(int Fs, int channels, int application, out IntPtr error);

        [DllImport("opus32-float-avx.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int opus_encode(IntPtr st, byte[] pcm, int frame_size, IntPtr data, int max_data_bytes);

        private static string sOutFile = @"D:\Downloads\Transcript.txt";
        public static double GetFileSeconds(string fileName)
        {

            TagLib.File oFile = TagLib.File.Create(fileName);
            return oFile.Properties.Duration.TotalSeconds;

        }
        static void Main(string[] args)
        {
            string sInFile = @"D:\Downloads\fredda johnson interview.flac";
            var Lenght = GetFileSeconds(sInFile);
            Console.WriteLine("Len " + Lenght.ToString());
            var Weight = new System.IO.FileInfo(sInFile).Length;
            Console.WriteLine("weight " + Weight.ToString());
            var ArrayOfSizes = GetArrayOfLenghts(Lenght, 60);//GetArrayOfSizes(Lenght, Weight, 10000000);
            ArrayOfSizes.ForEach(x => Console.WriteLine(x));
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", @"D:\Downloads\clave.json");
            var SplitFiles = SplitFileToolKit(sInFile, ArrayOfSizes);
            SplitFiles.ForEach(x => Console.WriteLine(x));
            if (File.Exists(sOutFile))
                File.Delete(sOutFile);
            foreach (var sFile in SplitFiles)
            {
                File.AppendAllText(sOutFile, STT(sFile));
            }
            Console.Read();
        }
        static string STT(string sIn)
        {
            while(!File.Exists(sIn))
            {
                System.Threading.Thread.Sleep(2000);
            }
            var speech = SpeechClient.Create();
            RecognizeResponse response = new RecognizeResponse();
            bool TryAgain = true;
            while (TryAgain)
            {
                try
                {
                    response = speech.Recognize(new RecognitionConfig()
                    {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Flac,
                        SampleRateHertz = 48000,
                        LanguageCode = "en",
                    }, RecognitionAudio.FromFile(sIn));
                    TryAgain = false;
                }
                catch (Exception err)
                {
                    if(!err.Message.Contains("because it is being used by another process."))
                        TryAgain = false;
                }
            }
            StringBuilder oSb = new StringBuilder("");
            foreach (var result in response.Results)
            {
                foreach (var alternative in result.Alternatives)
                {
                    oSb.AppendLine(alternative.Transcript);
                }
            }
            return oSb.ToString();
        }
        static List<int> GetArrayOfSizes(double Lenght, long Weight, long CutOff)
        {
            var bps = Weight / Lenght;
            List<int> iRet = new List<int>();
            int iHelp;
            while (Weight > 0)
            {
                if (Weight >= CutOff)
                {
                    iHelp = (int)Math.Floor(CutOff / bps);
                    Weight -= (long)(bps * iHelp);
                }
                else
                {
                    iHelp = (int)Math.Floor(Weight / bps);
                    Weight = 0;
                }
                iRet.Add(iHelp);

            }
            return iRet;
        }
        static List<int> GetArrayOfLenghts(double Lenght, int CutOff)
        {
            List<int> iRet = new List<int>();
            int iHelp;
            while (Lenght > 0)
            {
                if (Lenght >= CutOff)
                {
                    iHelp = CutOff;
                    Lenght -= CutOff;
                }
                else
                {
                    iHelp = (int)Lenght;
                    Lenght = 0;
                }
                iRet.Add(iHelp);

            }
            return iRet;
        }
        static List<string> SplitFileToolKit(string sIn, List<int> SecondsCut)
        {
            var inputFile = new MediaFile { Filename = sIn };
            string outputPrefix = sIn.Split(@"\").Last();
            string outputDirectory = sIn.Substring(0, sIn.Length - outputPrefix.Length);
            outputPrefix = outputPrefix.Split('.').First();

            var conversionOptions = new ConversionOptions() { AudioSampleRate = AudioSampleRate.Hz48000};

            List<string> sRetu = new List<string>();
            int i = 1;
            string sFile;
            int iAcum = 0;
            foreach (var s in SecondsCut)
            {
                sFile = outputDirectory + outputPrefix + i.ToString() + ".flac";
                sRetu.Add(sFile);

                var outputFile = new MediaFile { Filename = sFile };
                using (var engine = new Engine())
                {
                    conversionOptions.CutMedia(TimeSpan.FromSeconds(iAcum), TimeSpan.FromSeconds(s));
                    engine.Convert(inputFile, outputFile,conversionOptions);
                }
                iAcum += s;
                i++;
            }



            return sRetu;
        }
        static List<string> SplitFileSOX(string sIn, List<int> SecondsCut)
        {
            string sox = @"C:\Program Files (x86)\sox-14-4-2\sox.exe";
            string inputFile = sIn;
            string outputPrefix = sIn.Split(@"\").Last();
            string outputDirectory = sIn.Substring(0, sIn.Length - outputPrefix.Length);
            outputPrefix = outputPrefix.Split('.').First();
            int[] segments = SecondsCut.ToArray();

            List<string> enumerable = segments.Select(s => "trim 0 " + s.ToString() + " remix 1").ToList();
            string @join = string.Join(" : newfile : ", enumerable);
            string cmdline = string.Format("\"{0}\" -r 16000 \"{1}%1n.flac" + "\" {2}", inputFile,
                Path.Combine(outputDirectory, outputPrefix), @join);

            var processStartInfo = new ProcessStartInfo(sox, cmdline);
            Process start = System.Diagnostics.Process.Start(processStartInfo);
            List<string> sRetu = new List<string>();
            int i = 1;
            foreach(var s in SecondsCut)
            {
                sRetu.Add(outputDirectory + outputPrefix + i.ToString() + ".flac");
                i++;
            }
            return sRetu;
        }
    }
}
