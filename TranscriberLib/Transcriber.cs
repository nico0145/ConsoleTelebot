using CSCore;
using CSCore.Codecs;
using CSCore.DSP;
using Google.Cloud.Speech.V1;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TranscriberLib
{
    public static class Transcriber
    {
        public static double GetFileMilliSeconds(string fileName)
        {
            var inputFile = new MediaToolkit.Model.MediaFile { Filename = fileName };
            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);
            }
            return inputFile.Metadata.Duration.TotalMilliseconds;
        }
        public static string Transcribe(string sInFile, string sLang, string sGoogleKey)
        {
            var Lenght = GetFileMilliSeconds(sInFile);
            var ArrayOfSizes = GetArrayOfLenghts(Lenght, 60000);
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", sGoogleKey);// @"D:\Downloads\clave.json");s
            if (!sInFile.EndsWith(".oga") && !sInFile.EndsWith(".ogg"))
                SetToMono(sInFile);
            var SplitFiles = SplitFileToolKit(sInFile, ArrayOfSizes);
            SplitFiles.ForEach(x => Console.WriteLine(x));
            StringBuilder oSb = new StringBuilder("");
            foreach (var sFile in SplitFiles)
            {
                oSb.Append(STT(sFile, sLang));
                File.Delete(sFile);
            }
            return oSb.ToString();
        }
        static void SetToMono(string sFile)
        {
            StereoToMono(sFile, "temp");
            File.Delete(sFile);
            File.Move("temp", sFile);
        }

        static void StereoToMono(string input, string sDest)
        {
            CSCore.DSP.ChannelMatrix cm = CSCore.DSP.ChannelMatrix.StereoToMonoMatrix;
            var sarasa = CodecFactory.Instance.GetCodec(input);
            if (sarasa.WaveFormat.Channels == 2)
            {
                IWaveSource waveSource = sarasa.AppendSource(x => new CSCore.Streams.CachedSoundSource(x))
                                                .AppendSource(x => new DmoChannelResampler(x, cm)) //append a channelresampler with the channelmatrix
                                                .ToSampleSource()
                                                .ToWaveSource(16);
                waveSource.WriteToFile(sDest);
            }
            else
                sarasa.WriteToFile(sDest);
            sarasa.Dispose();
        }
        static string STT(string sIn, string sLang)
        {
            while (!File.Exists(sIn))
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
                        LanguageCode = sLang,
                    }, RecognitionAudio.FromFile(sIn));
                    TryAgain = false;
                }
                catch (Exception err)
                {
                    if (!err.Message.Contains("because it is being used by another process."))
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
        static List<long> GetArrayOfLenghts(double Lenght, long CutOff)
        {
            List<long> iRet = new List<long>();
            long iHelp;
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
        static List<string> SplitFileToolKit(string sIn, List<long> SecondsCut)
        {
            var inputFile = new MediaFile { Filename = sIn };
            string outputPrefix = sIn.Split(@"\").Last();
            string outputDirectory = sIn.Substring(0, sIn.Length - outputPrefix.Length);
            outputPrefix = outputPrefix.Split('.').First();

            var conversionOptions = new ConversionOptions() { AudioSampleRate = AudioSampleRate.Hz48000 };

            List<string> sRetu = new List<string>();
            int i = 1;
            string sFile;
            long iAcum = 0;
            foreach (var s in SecondsCut)
            {
                sFile = outputDirectory + outputPrefix + i.ToString() + ".flac";
                sRetu.Add(sFile);

                var outputFile = new MediaFile { Filename = sFile };
                using (var engine = new Engine())
                {
                    conversionOptions.CutMedia(TimeSpan.FromMilliseconds(iAcum), TimeSpan.FromMilliseconds(s));
                    engine.Convert(inputFile, outputFile, conversionOptions);
                }
                iAcum += s;
                i++;
            }
            return sRetu;
        }
    }
}
