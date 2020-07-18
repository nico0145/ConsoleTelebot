using System;
using System.Threading;
using System.Collections.Generic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UrbanDictionnet;
using System.Linq;
using System.Xml.Linq;
using System.IO;
using Google.Apis.Translate.v2;
using Api.Forex.Sharp;
using Google.Apis.Translate.v2.Data;
using System.Text.RegularExpressions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using VideoLibrary;
using MediaToolkit.Model;
using MediaToolkit;
using System.Threading.Tasks;
using System.Net;
using HtmlAgilityPack;
using System.Globalization;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace ConsoleTelebot
{
    static class Program
    {
        static XDocument OXml { set; get; }
        static XDocument OBLXML { set; get; }
        static XDocument ORespXml { set; get; }

        static ITelegramBotClient botClient;
        static Faqs oFaqlist;
        static Responses oResps;
        static Blacklist oBL;
        static string[] sLangs = { "af-ZA", "am-ET", "hy-AM", "az-AZ", "id-ID", "ms-MY", "bn-BD", "bn-IN", "ca-ES", "cs-CZ", "da-DK", "de-DE", "en-AU", "en-CA", "en-GH", "en-GB", "en-IN", "en-IE", "en-KE", "en-NZ", "en-NG", "en-PH", "en-SG", "en-ZA", "en-TZ", "en-US", "es-AR", "es-BO", "es-CL", "es-CO", "es-CR", "es-EC", "es-SV", "es-ES", "es-US", "es-GT", "es-HN", "es-MX", "es-NI", "es-PA", "es-PY", "es-PE", "es-PR", "es-DO", "es-UY", "es-VE", "eu-ES", "fil-PH", "fr-CA", "fr-FR", "gl-ES", "ka-GE", "gu-IN", "hr-HR", "zu-ZA", "is-IS", "it-IT", "jv-ID", "k", "-IN", "km-KH", "lo-LA", "lv-LV", "lt-LT", "hu-HU", "ml-IN", "mr-IN", "nl-NL", "ne-NP", "nb-NO", "pl-PL", "pt-BR", "pt-PT", "ro-RO", "si-LK", "sk-SK", "sl-SI", "su-ID", "sw-TZ", "sw-KE", "fi-FI", "sv-SE", "ta-IN", "ta-SG", "ta-LK", "ta-MY", "te-IN", "vi-VN", "t", "-TR", "ur-PK", "ur-IN", "el-GR", "bg-BG", "ru-RU", "sr-RS", "uk-UA", "he-IL", "ar-IL", "ar-JO", "ar-AE", "ar-BH", "ar-DZ", "ar-SA", "ar-IQ", "ar-KW", "ar-MA", "ar-TN", "ar-OM", "ar-PS", "ar-QA", "ar-LB", "ar-EG", "fa-IR", "hi-IN", "th-TH", "ko-KR", "zh-TW", "yue-Hant-HK", "ja-JP", "zh-HK", "zh" };
        static bool EnableConsoleLog;
        static bool EnableAdmChatLog;
        static string ForexApiKey = "YourAPIKeyHere";
        static string sToken = "YourTGBotTokenHere";
        static string googlekey = "YourAPIKeyHere";
        static string DownloadFolder = @"Files/";
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleMode(
            IntPtr hConsoleHandle,
            out int lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleMode(
            IntPtr hConsoleHandle,
            int ioMode);
        const int ExtendedFlags = 128;
        const int QuickEditMode = 64;
        static void DisableQuickEdit()
        {
            int mode = ~(QuickEditMode | ExtendedFlags);
            IntPtr conHandle = GetConsoleWindow();
            SetConsoleMode(conHandle, mode);
        }
        static void DisableQuickEditSafe()
        {
            IntPtr conHandle = GetConsoleWindow();
            int mode;

            if (!GetConsoleMode(conHandle, out mode))
            {
                // error getting the console mode. Exit.
                return;
            }

            mode = mode & ~(QuickEditMode | ExtendedFlags);

            if (!SetConsoleMode(conHandle, mode))
            {
                // error setting console mode.
            }
        }

        static void EnableQuickEdit()
        {
            IntPtr conHandle = GetConsoleWindow();
            int mode;

            if (!GetConsoleMode(conHandle, out mode))
            {
                // error getting the console mode. Exit.
                return;
            }

            mode = mode | (QuickEditMode | ExtendedFlags);

            if (!SetConsoleMode(conHandle, mode))
            {
                // error setting console mode.
            }
        }
        static void Main(string[] args)
        {
            //https://telegrambots.github.io/book/
            botClient = new TelegramBotClient(sToken);
            var me = botClient.GetMeAsync().Result;
            DisableQuickEdit();
            Console.WriteLine(
              $"Hola gil, soy {me.FirstName} y mi ID de usuario es {me.Id}."
            );
            if (!System.IO.File.Exists(@"FaqFile.xml"))
            {
                using (StreamWriter sw = System.IO.File.AppendText(@"FaqFile.xml"))
                {
                    sw.Write(@"<Faqs></Faqs>");
                }
            }
            if (!System.IO.File.Exists(@"WordFile.xml"))
            {
                using (StreamWriter sw = System.IO.File.AppendText(@"WordFile.xml"))
                {
                    sw.Write(@"<Palabras></Palabras>");
                }
            }
            if (!System.IO.File.Exists(@"RespFile.xml"))
            {
                using (StreamWriter sw = System.IO.File.AppendText(@"RespFile.xml"))
                {
                    sw.Write(@"<Responses></Responses>");
                }
            }
            OXml = XDocument.Load(@"FaqFile.xml");
            OBLXML = XDocument.Load(@"WordFile.xml");
            ORespXml = XDocument.Load(@"RespFile.xml");
            oFaqlist = new Faqs(OXml.Root);
            oResps = new Responses(ORespXml.Root);
            oBL = new Blacklist(OBLXML);
            EnableConsoleLog = false;
            EnableAdmChatLog = false;
            botClient.OnMessage += Bot_OnMessage;
            botClient.StartReceiving();
            while (true)
            {
                string sIn = Console.ReadLine();
                MandarMensaje(sIn);
                if (sIn == "Enable")
                {
                    EnableConsoleLog = true;
                }
                if (sIn == "Disable")
                {
                    EnableConsoleLog = false;
                }
            }
        }
        static void MandarMensaje(string sIn)
        {
            if (long.TryParse(sIn.Split(' ')[0], out long iChatId))
            {
                botClient.SendTextMessageAsync(
                                  chatId: iChatId,
                                  text: sIn.Substring(sIn.Split(' ')[0].Length + 1)
                                );
            }
        }
        static string SetFaqBlock(bool IsAdmin, string FaqKey, bool IsBlocked, long ChatId)
        {
            string sReturnMessage;
            if (IsAdmin)
            {
                Freqaskedq oFaq = oFaqlist.FirstOrDefault(x => x.Key == FaqKey);
                if (oFaq != null)
                {
                    oFaqlist.SetFaqBlock(oFaq, IsBlocked, OXml, ChatId);
                    OXml.Save(@"FaqFile.xml");
                    sReturnMessage = oFaq.Key + " " + (IsBlocked ? "B" : "Desb") + "loqueado";
                }
                else
                    sReturnMessage = @"Faq no encontrado, Despertate guacho";
            }
            else
                sReturnMessage = "Chupala gil, no podes cambiar esto";
            return sReturnMessage;
        }
        static async Task<string> Transcribe(ITelegramBotClient client, Message oRaw)
        {
            string sError = null;
            while (string.IsNullOrWhiteSpace(sError))
            {
                try
                {
                    var mFile = await client.GetFileAsync(oRaw.Voice.FileId);
                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFile($@"https://api.telegram.org/file/bot{sToken}/{mFile.FilePath}", "temp.ogg");//check extension
                    }
                    return TranscriberLib.Transcriber.Transcribe("temp.ogg", "es", "claveapi.json");
                }
                catch (Exception err)
                {
                    if (!err.Message.Contains("because it is being used by another process"))
                        sError = err.Message;
                }
            }
            return sError;
        }
        static async Task<string> Transcribe(ITelegramBotClient client, Message oRaw, string sLang)
        {
            string sRetu = "";
            try
            {
                string sAudioID = null;
                if (oRaw.ReplyToMessage.Audio != null)
                    sAudioID = oRaw.ReplyToMessage.Audio.FileId;
                if (oRaw.ReplyToMessage.Voice != null)
                    sAudioID = oRaw.ReplyToMessage.Voice.FileId;
                if (oRaw.ReplyToMessage.Document != null && oRaw.ReplyToMessage.Document.MimeType.ToLower().Contains("audio"))
                    sAudioID = oRaw.ReplyToMessage.Document.FileId;
                if (!string.IsNullOrWhiteSpace(sAudioID))
                {
                    var mFile = await client.GetFileAsync(sAudioID);
                    string FilePath = $"{DownloadFolder}{oRaw.Chat.Id.ToString()}-{oRaw.MessageId.ToString()}.{mFile.FilePath.Split('.').LastOrDefault()}";
                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFile($@"https://api.telegram.org/file/bot{sToken}/{mFile.FilePath}", FilePath);//check extension
                    }
                    var msgPW = await client.SendTextMessageAsync(
                          chatId: oRaw.Chat.Id,
                          replyToMessageId: oRaw.MessageId,
                          text: "Audio recibido, por favor espera a la transcripcion"
                        );
                    var sResponse = TranscriberLib.Transcriber.Transcribe(FilePath, sLang, "claveapi.json");
                    await client.DeleteMessageAsync(msgPW.Chat.Id, msgPW.MessageId);
                    System.IO.File.Delete(FilePath);
                    if (sResponse.Length > 4096)
                    {
                        System.IO.File.WriteAllText("Transcription.txt", sResponse);
                        using (var stream = System.IO.File.OpenRead("Transcription.txt"))
                        {

                            await client.SendDocumentAsync(document: new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, "Transcription.txt"),
                                                           chatId: oRaw.Chat.Id,
                                                           replyToMessageId: oRaw.MessageId,
                                                           caption: "Mensaje demasiado largo para Telegram, fijate el documento");
                        }
                        System.IO.File.Delete("Transcription.txt");
                    }
                    else
                    {
                        await client.SendTextMessageAsync(
                                chatId: oRaw.Chat.Id,
                                replyToMessageId: oRaw.MessageId,
                                text: sResponse
                        );
                    }
                }
                else
                {
                    sRetu = "Tenes que hacerle reply a algun mensaje con audio o voz";
                }
            }
            catch (Exception ex)
            {
                Log(ex.Message, "Log.txt");
                sRetu = "Hubo un error al transcribir el audio";// + Environment.NewLine + ex.Message;
            }
            return sRetu;
        }
        static async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            List<int> AdmList = new List<int>() { 889467441 };
            //889467441 nico
            //826418773 pancha
            bool IsAdmin = AdmList.Any(x => x == e.Message.From.Id);
            string Command = "";
            string MessageText = null;
            if (e.Message.Text != null)
                MessageText = e.Message.Text;
            else if (e.Message.Voice != null)
            {
                MessageText = await Transcribe(botClient, e.Message);
            }
            else if (e.Message.Sticker != null)
            {
                MessageText = e.Message.Sticker.Emoji;
            }
            if (MessageText != null)
            {
                try
                {

                    string sReturnMessage = "";
                    if (IsAdmin && EnableAdmChatLog)
                    {
                        MandarMensaje(MessageText);
                    }
                    if (MessageText.ToLower().Split(' ')[0].Split('@')[0] == "/blacklist")
                    {
                        if (IsAdmin)
                        {
                            switch (MessageText.ToLower().Split(' ')[1])
                            {
                                case "meter":
                                case "add":
                                case "poner":
                                    oBL.AddSave(MessageText.ToLower().Split(' ')[2], OBLXML);
                                    sReturnMessage = MessageText.ToLower().Split(' ')[2] + " metido a la lista de palabras prohibidas";
                                    break;
                                case "sacar":
                                case "quitar":
                                case "remove":
                                case "delete":
                                    oBL.RemoveSave(MessageText.ToLower().Split(' ')[2], OBLXML);
                                    sReturnMessage = MessageText.ToLower().Split(' ')[2] + " sacado de la lista de palabras prohibidas";
                                    break;
                                case "enable":
                                case "activar":
                                    oBL.Enabled = true;
                                    sReturnMessage = "Filtro activado, guarda con lo que decis ameo que cae la gorra";
                                    break;
                                case "disable":
                                case "desactivar":
                                    oBL.Enabled = false;
                                    sReturnMessage = "Filtro desactivado, cuando el gato no esta los ratones bailan ameo";
                                    break;
                            }
                        }
                        else
                        {
                            sReturnMessage = "Toca de aca logi, quien te juna gato";
                        }
                    }
                    else
                    {
                        string sBLWord = oBL.FindMatch(MessageText, e.Message.From.FirstName, IsAdmin);
                        if (sBLWord != null)
                        {
                            await botClient.DeleteMessageAsync(chatId: e.Message.Chat.Id, e.Message.MessageId);
                            await botClient.SendTextMessageAsync(
                                      chatId: e.Message.Chat,
                                      text: sBLWord
                                    );
                        }
                        else
                        {
                            Freqaskedq oFaq;
                            Response[] oResp;
                            string Parameter;
                            Command = ReplaceCommandText(MessageText.ToLower().Split("\n")[0].Split(' ')[0].Split('@')[0]);
                            Parameter = ReplaceCommandText(MessageText.ToLower()).Split("\n")[0].Substring(Command.Length).TrimStart(' ');
                            switch (Command)
                            {
                                case "/delete":
                                    if (IsAdmin && e.Message.ReplyToMessage != null)
                                    {
                                        await botClient.DeleteMessageAsync(chatId: e.Message.Chat.Id, e.Message.ReplyToMessage.MessageId);
                                        await botClient.DeleteMessageAsync(chatId: e.Message.Chat.Id, e.Message.MessageId);
                                    }
                                    break;
                                case "/pancha":
                                    sReturnMessage = "Pancha se la come";
                                    break;
                                case "/urban":
                                    sReturnMessage = ((new UrbanClient()).GetWordAsync(MessageText.Substring("/urban ".Length))).Result.Definitions.AsEnumerable().FirstOrDefault()?.Definition;
                                    if (string.IsNullOrWhiteSpace(sReturnMessage))
                                        sReturnMessage = "Definicion no encontrada";
                                    else
                                        sReturnMessage = MessageText.Substring("/urban ".Length) + Environment.NewLine + Environment.NewLine + sReturnMessage;
                                    break;
                                case "/setfaq":
                                    oFaq = oFaqlist.ModificarFaq(Parameter.Split(' ')[0], Parameter.Substring(Parameter.Split(' ')[0].Length + 1), OXml, IsAdmin, e.Message.Chat.Id);
                                    if (oFaq != null)
                                        sReturnMessage = "Se actualizo " + oFaq.Key + " en la lista de Faqs";
                                    else
                                        sReturnMessage = "Chupala, no podes cambiar este faq por gil";
                                    break;
                                case "/respondea":
                                case "/agregara":
                                    if (e.Message.ReplyToMessage != null && !e.Message.ReplyToMessage.From.IsBot)
                                    {
                                        var oMsg = new TGMessage();
                                        string sRetu = await oMsg.GetByMessage(e.Message.ReplyToMessage, botClient, DownloadFolder, sToken);
                                        if (string.IsNullOrEmpty(sRetu))
                                            oResp = oResps.ModificarRespuesta(Parameter, oMsg, ORespXml, e.Message.Chat.Id, Command == "/agregara");
                                        else
                                            Log(sRetu, $"{e.Message.Chat.Id}.txt");
                                        sReturnMessage = "Se actualizo " + Parameter + " en la lista de respuestas";
                                    }
                                    else
                                        sReturnMessage = "Tenes que hacerle reply a algo que no venga de otro Bot para guardarlo, nabito";
                                    break;
                                case "/norespondasa":
                                    sReturnMessage = oResps.BorrarRespuesta(Parameter, e.Message.Chat.Id, ORespXml);
                                    break;
                                case "/transcribir":

                                    if (sLangs.Any(x => x.ToLower().Equals(Parameter)))
                                        sReturnMessage = await Transcribe(botClient, e.Message, e.Message.Text.Split(' ').Last());
                                    else
                                        sReturnMessage = "parametro incorrecto, por favor referencia al mensaje con un codigo de lenguaje correcto (ejemplo /transcribir es-AR)" + Environment.NewLine
                                            + "Para una lista completa de los lenguajes disponibles visita esta pagina: https://cloud.google.com/speech-to-text/docs/languages";
                                    break;
                                case "/quitarde":
                                    int iMsgId = e.Message.ReplyToMessage.MessageId;
                                    if (e.Message.ReplyToMessage.From.IsBot)
                                    {
                                        oResp = oResps.Findresp(Parameter, e.Message.Chat.Id);
                                        string sMd5 = await TGMessage.GetMD5Hash(e.Message.ReplyToMessage, botClient, sToken);
                                        if (sMd5 != null)
                                            iMsgId = oResp.FirstOrDefault(x => x.Value.DoMessagesMatch(e.Message, sMd5))?.Value.MessageId ?? 0;
                                        else
                                            iMsgId = oResp.FirstOrDefault(x => x.Value.MessageText == e.Message.ReplyToMessage.Text)?.Value.MessageId ?? 0;
                                    }
                                    sReturnMessage = oResps.BorrarRespuesta(Parameter, e.Message.Chat.Id, ORespXml, iMsgId);
                                    break;
                                case "/faq":
                                    oFaq = oFaqlist.FirstOrDefault(x => x.Key == Parameter && x.ChatId == e.Message.Chat.Id);
                                    if (oFaq != null)
                                        sReturnMessage = /*oFaq.Key + ": " +*/ oFaq.Value;
                                    else
                                        sReturnMessage = @"Faq no encontrado, si queres hacer uno escribi /Setfaq " + Parameter + " [Mensaje]";
                                    break;
                                case "/faqlist":
                                    System.Text.StringBuilder oSb = new System.Text.StringBuilder("Lista de Faqs disponibles: ");
                                    foreach (var oRow in oFaqlist.Where(x => x.ChatId == e.Message.Chat.Id))
                                    {
                                        oSb.AppendLine(oRow.Key);
                                    }
                                    sReturnMessage = oSb.ToString();
                                    break;
                                case "/aaaa":
                                    sReturnMessage = new string('A', 1000);
                                    break;
                                case "/bloqfaq":
                                    sReturnMessage = SetFaqBlock(IsAdmin, Parameter, true, e.Message.Chat.Id);
                                    break;
                                case "/unbloqfaq":
                                    sReturnMessage = SetFaqBlock(IsAdmin, Parameter, false, e.Message.Chat.Id);
                                    break;
                                case "f":
                                    sReturnMessage = "F";
                                    break;
                                case "/aaaah":
                                    sReturnMessage = "Otra vez estoy tomando falopa"
                                        + Environment.NewLine + "AAAAHHH"
                                        + Environment.NewLine + "Me encanta maldita falopa"
                                        + Environment.NewLine + "AAAAHHH"
                                        + Environment.NewLine + "Tomo bartulo para olvidar falopa"
                                        + Environment.NewLine + "Me fumo un par de porros para olvidar la manija"
                                        + Environment.NewLine + "- Miguel de Cervantes Saavedra";
                                    break;
                                case "/milei":
                                    //if (IsAdmin) sacar
                                    //{en
                                    //    await botClient.PromoteChatMemberAsync(e.Message.Chat.Id, e.Message.From.Id, true,true,true,true,true,true,true,true); algun
                                    //}momento
                                    sReturnMessage = "https://i.imgur.com/GTWSgVq.png";
                                    break;
                                case "/ingles":
                                    sReturnMessage = GoogleTranslate(Parameter, "en");
                                    break;
                                case "/espanol":
                                    sReturnMessage = GoogleTranslate(Parameter, "es");
                                    break;
                                case "/italiano":
                                    sReturnMessage = GoogleTranslate(Parameter, "it");
                                    break;
                                case "/japones":
                                    sReturnMessage = GoogleTranslate(Parameter, "ja");
                                    break;
                                case "/chino":
                                    sReturnMessage = GoogleTranslate(Parameter, "zh-CN");
                                    break;
                                case "/aleman":
                                    sReturnMessage = GoogleTranslate(Parameter, "de");
                                    break;
                                case "/frances":
                                    sReturnMessage = GoogleTranslate(Parameter, "fr");
                                    break;
                                case "/peso":
                                    if (decimal.TryParse(Parameter, System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowCurrencySymbol, System.Globalization.CultureInfo.CreateSpecificCulture("es-AR"), out decimal Amt))
                                    {
                                        var apiForex = await ApiForex.GetRate(ForexApiKey);
                                        var mAmbito = new AmbitoBlue().GetAmbitoBlueValue();
                                        sReturnMessage = $"Oficial: U$D {Amt.ToString("#.##")} = $ {apiForex.Convert("ARS", "USD", Amt).ToString("#.##")}" + Environment.NewLine
                                                       + $"Blue: U$D {Amt.ToString("#.##")} = $ {(Amt * mAmbito.dCompra).ToString("#.##")}";
                                    }
                                    break;
                                case "/dolar":
                                    if (decimal.TryParse(Parameter, System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowCurrencySymbol, System.Globalization.CultureInfo.CreateSpecificCulture("es-AR"), out decimal Amtd))
                                    {
                                        var apiForex = await ApiForex.GetRate(ForexApiKey);
                                        var mAmbito = new AmbitoBlue().GetAmbitoBlueValue();
                                        sReturnMessage = $"Oficial: $ {Amtd.ToString("#.##")} = U$D {apiForex.Convert("USD", "ARS", Amtd).ToString("#.##")}" + Environment.NewLine
                                                       + $"Blue: $ {Amtd.ToString("#.##")} = $ {(Amtd / mAmbito.dVenta).ToString("#.####")}"; ;
                                    }
                                    break;
                                case "/convertir":
                                    if (decimal.TryParse(Parameter.Split(' ')[0], System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowCurrencySymbol, System.Globalization.CultureInfo.CreateSpecificCulture("es-AR"), out decimal Amtc))
                                    {
                                        if (Parameter.Split(' ').Count() == 4 && (Parameter.Split(' ')[2] == "a" || Parameter.Split(' ')[2] == "to"))
                                        {
                                            var apiForex = await ApiForex.GetRate(ForexApiKey);
                                            sReturnMessage = $"{Parameter.Split(' ')[1]} {Amtc.ToString("#.##")} = {Parameter.Split(' ')[3]} {apiForex.Convert(Parameter.Split(' ')[3].ToUpper(), Parameter.Split(' ')[1].ToUpper(), Amtc).ToString("#.##")}";
                                        }
                                        else
                                            sReturnMessage = "Formato incorrecto";
                                    }
                                    break;
                                case "/enablelog":
                                    if (IsAdmin)
                                    {
                                        EnableAdmChatLog = true;
                                        sReturnMessage = "Log activado";
                                    }
                                    break;
                                case "/desbloquear":
                                    if (IsAdmin)
                                    {
                                        sReturnMessage = "Log activado";
                                    }
                                    break;
                                case "/redditpfp":
                                    try
                                    {
                                        var jSon = new WebClient().DownloadString($"https://www.reddit.com/user/{Parameter}/about.json");
                                        dynamic redditUser = Newtonsoft.Json.JsonConvert.DeserializeObject(jSon);
                                        DownloadFile(redditUser.data.icon_img.ToString().Split('?')[0], "Temp.jpg");
                                        await ReplyWithFile(botClient, "Temp.jpg", e.Message);
                                        sReturnMessage = "";
                                    }
                                    catch (Exception err)
                                    {
                                        sReturnMessage = "no se pudo encontrar la imagen";
                                        Log(err.Message, "log.txt");
                                    }
                                    ///html/body/div[6]/div/img
                                    break;
                                case "/tapaclarin":

                                    if (DateTime.TryParseExact(Parameter, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                                    {
                                        try
                                        {
                                            DownloadFile($"https://tapas.clarin.com/tapa/{dateTime.Year.ToString("0000")}/{dateTime.Month.ToString("00")}/{dateTime.Day.ToString("00")}/{dateTime.Year.ToString("0000")}{dateTime.Month.ToString("00")}{dateTime.Day.ToString("00")}_thumb.jpg", "Temp.jpg");
                                            await ReplyWithFile(botClient, "Temp.jpg", e.Message);
                                            sReturnMessage = "";
                                        }
                                        catch (Exception ex)
                                        {
                                            sReturnMessage = $"Error: {ex.Message}";
                                        }
                                    }
                                    else
                                        sReturnMessage = "Formato de fecha incorrecto, usa dd/MM/yyyy";
                                    break;
                                case "/disablelog":
                                    if (IsAdmin)
                                    {
                                        EnableAdmChatLog = false;
                                        sReturnMessage = "Log desactivado";
                                    }
                                    break;
                                case "/youtubeaudio":
                                case "/youtube":
                                    var youtubeService = new YouTubeService(new BaseClientService.Initializer()
                                    {
                                        ApiKey = googlekey,
                                        ApplicationName = "TelegramBot"
                                    });
                                    var searchListRequest = youtubeService.Search.List("snippet");
                                    searchListRequest.Q = Parameter; // Replace with your search term.
                                    searchListRequest.MaxResults = 10;

                                    // Call the search.list method to retrieve results matching the specified query term.
                                    var searchListResponse = await searchListRequest.ExecuteAsync();
                                    sReturnMessage = @"https://www.youtube.com/watch?v=" + searchListResponse.Items.FirstOrDefault(x => x.Id.Kind == "youtube#video").Id.VideoId;
                                    break;
                                case "/youtubeaudioid":
                                    sReturnMessage = @"https://www.youtube.com/watch?v=" + MessageText.Split("\n")[0].Substring(Command.Length).TrimStart(' ');
                                    break;
                                default:
                                    break;
                            }
                            if (!MessageText.StartsWith("/"))
                            {
                                var replacedText = ReplaceCommandText(MessageText, false);
                                oResp = oResps.Findresp(replacedText, e.Message.Chat.Id);
                                if (oResp.Any())
                                {
                                    sReturnMessage = null;
                                    bool bFirst = true;
                                    foreach (var oResp1 in oResp)
                                    {
                                        await oResp1.Value.SendMessage(botClient, e.Message.Chat.Id, (bFirst ? e.Message.MessageId : 0));
                                        bFirst = false;
                                    }
                                }
                                else if (ReplaceCommandText(MessageText, false).Contains("me pelie con mi novi"))
                                {
                                    oResp = oResps.Findresp($"Contador{e.Message.From.Id}", e.Message.Chat.Id);
                                    if (oResp.Any())
                                    {
                                        sReturnMessage = null;
                                        var msg = oResp.FirstOrDefault().Value;
                                        if (int.TryParse(msg.MessageText, out int iCount))
                                        {
                                            iCount++;
                                            msg.MessageText = $"Otra vez macho, van {iCount} veces ya!";
                                            await msg.SendMessage(botClient, e.Message.Chat.Id, e.Message.MessageId);
                                            msg.MessageText = iCount.ToString();
                                            oResps.ModificarRespuesta($"Contador{e.Message.From.Id}", msg, ORespXml, e.Message.Chat.Id, false);
                                        }
                                    }
                                    else
                                    {
                                        var msg = new TGMessage()
                                        {
                                            MessageText = "1",
                                            MessageId = e.Message.MessageId,
                                            UserId = e.Message.From.Id,
                                            UserName = e.Message.From.FirstName
                                        };
                                        oResp = oResps.ModificarRespuesta($"Contador{e.Message.From.Id}", msg, ORespXml, e.Message.Chat.Id, false);
                                        sReturnMessage = "Que garron hermano, ya vendran tiempos mejores...";
                                    }
                                }
                                //else if (MessageText.Contains("Testiame"))
                                //{
                                //////////await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Basta chiques", ParseMode.Markdown, false, false, 0, new ReplyKeyboardRemove());
                                //    var rkm = new ReplyKeyboardMarkup();

                                //    rkm.Keyboard =
                                //    new KeyboardButton[][]
                                //    {
                                //        new KeyboardButton[]
                                //        {
                                //            new KeyboardButton("Envido"),
                                //            new KeyboardButton("Real Envido"),
                                //            new KeyboardButton("Falta Envido"),
                                //            new KeyboardButton("Truco")
                                //        },

                                //        new KeyboardButton[]
                                //        {
                                //            new KeyboardButton("Quiero"),
                                //            new KeyboardButton("No Quiero")
                                //        },

                                //        new KeyboardButton[]
                                //        {
                                //            new KeyboardButton("12 de Oro"),
                                //            new KeyboardButton("1 de Copa"),
                                //            new KeyboardButton("7 de Oro")
                                //        }
                                //    };

                                //    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "sarasa", ParseMode.Markdown, false, false, 0, rkm);
                                // }
                                else if (replacedText.EndsWith("milei"))
                                    sReturnMessage = "...di *Tips fedora*";
                                else if (replacedText.EndsWith("que fiesta"))
                                    sReturnMessage = "La de tu culo y esta";
                                else if (replacedText.EndsWith("que foto"))
                                    sReturnMessage = "La de tu culo y mi choto";
                                else if (replacedText.EndsWith("que marcha"))
                                    sReturnMessage = "La de tu culo y mi garcha";
                                else if (replacedText.EndsWith("que reunion"))
                                    sReturnMessage = "La de tu culo y mi cañon";
                                else if (replacedText.EndsWith("que marcelo"))
                                    sReturnMessage = "Agachate y conocelo";
                                else if (replacedText.EndsWith("que jose"))
                                    sReturnMessage = "El que te la puso y se fue";
                                if (e.Message.ReplyToMessage != null && !e.Message.ReplyToMessage.From.IsBot && e.Message.ReplyToMessage.From.Id != e.Message.From.Id)
                                {
                                    var oMsg = new TGMessage();
                                    string sRetu;
                                    if ((new string[] { "+", "d1", "<3", "mas", "gracias", "-purnie" }).Contains(replacedText))
                                    {
                                        sRetu = await oMsg.GetByMessage(e.Message.ReplyToMessage, botClient, DownloadFolder, sToken);
                                        if (string.IsNullOrEmpty(sRetu))
                                            oResp = oResps.ModificarRespuesta("upvotes" + e.Message.ReplyToMessage.From.FirstName.ToLower(), oMsg, ORespXml, e.Message.Chat.Id, true);
                                        else
                                            Log(sRetu, $"{e.Message.Chat.Id}.txt");
                                        //upvote
                                    }
                                    else if ((new string[] { "-", "puto", "mogolico", "pelotudo", "menos", "-gero" }).Contains(replacedText))
                                    {
                                        sRetu = await oMsg.GetByMessage(e.Message.ReplyToMessage, botClient, DownloadFolder, sToken);
                                        if (string.IsNullOrEmpty(sRetu))
                                            oResp = oResps.ModificarRespuesta("downvotes" + e.Message.ReplyToMessage.From.FirstName.ToLower(), oMsg, ORespXml, e.Message.Chat.Id, true);
                                        else
                                            Log(sRetu, $"{e.Message.Chat.Id}.txt");
                                        //downvote
                                    }
                                }
                            }

                        }
                    }
                    if (!string.IsNullOrWhiteSpace(sReturnMessage) && Command != "/youtubeaudio" && Command != "/youtubeaudioid")
                    {
                        try
                        {
                            await botClient.SendTextMessageAsync(
                                      chatId: e.Message.Chat,
                                      text: sReturnMessage
                                    );
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    }
                    else
                    {
                        if (MessageText.ToLower() == "/upbotnico")
                        {

                            await botClient.SendTextMessageAsync(
                                  chatId: e.Message.Chat, // or a chat id: 123456789
                                  text: "+",
                                  parseMode: ParseMode.Markdown,
                                  disableNotification: true,
                                  replyToMessageId: e.Message.MessageId
                                );

                        }
                        else
                        {
                            if ((Command == "/youtubeaudio" || Command == "/youtubeaudioid") && !string.IsNullOrWhiteSpace(sReturnMessage))
                            {
                                //var source = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);//folder
                                var youtube = YouTube.Default;
                                var vid = youtube.GetVideo(sReturnMessage);
                                System.IO.File.WriteAllBytes("video" + vid.FileExtension, vid.GetBytes());

                                var inputFile = new MediaFile { Filename = "video" + vid.FileExtension };
                                var outputFile = new MediaFile { Filename = "audio.mp3" };

                                using (var engine = new Engine())
                                {
                                    engine.GetMetadata(inputFile);
                                    engine.Convert(inputFile, outputFile);
                                }
                                TagLib.File oF = TagLib.File.Create(outputFile.Filename);
                                var oMBData = MusicBrainz.Search.Recording(recording: vid.Title.ToLower().Replace("youtube", "")); // ver detalles api aca http://musicbrainz.org/ws/2/recording/?query=recording:
                                var oFilteredMbData = oMBData?.Data.Where(y => vid.Title.Split(' ').Any(x => y.Artistcredit.FirstOrDefault()?.Artist.Name.Contains(x) ?? false) && y.Score > 90);
                                if (oFilteredMbData?.Any() == true)
                                {
                                    var oRec = oFilteredMbData.First();
                                    oF.Tag.Title = oRec.Title;
                                    oF.Tag.MusicBrainzArtistId = oRec.Artistcredit.FirstOrDefault()?.Artist.Id;
                                    oF.Tag.AlbumArtists = new string[] { oRec.Artistcredit.FirstOrDefault()?.Artist.Name };
                                    oF.Tag.Performers = oF.Tag.AlbumArtists;
                                    oF.Tag.Album = oRec.Releaselist.FirstOrDefault().Title;
                                }
                                else
                                {
                                    oF.Tag.Title = vid.Title;
                                }
                                string path = "tempcover.jpg";
                                DownloadFile("https://img.youtube.com/vi/" + sReturnMessage.Split('=').LastOrDefault() + "/default.jpg", path);
                                using (MemoryStream ms = new MemoryStream(System.IO.File.ReadAllBytes(path)))
                                {
                                    oF.Tag.Pictures = new TagLib.IPicture[]
                                    {
                                            new TagLib.Picture(TagLib.ByteVector.FromStream(ms))
                                            {
                                                Type = TagLib.PictureType.FrontCover,
                                                Description = "Cover",
                                                MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg
                                            }
                                    };
                                }
                                oF.Save();
                                using (FileStream stream = System.IO.File.Open(outputFile.Filename, FileMode.Open))
                                {
                                    await botClient.SendAudioAsync(
                                      chatId: e.Message.Chat, // or a chat id: 123456789
                                      audio: new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, ReplaceCommandText(vid.Title).Replace("\"", "") + ".mp3"),
                                      disableNotification: true,
                                      replyToMessageId: e.Message.MessageId
                                    );
                                }
                                System.IO.File.Delete(inputFile.Filename);
                                System.IO.File.Delete(outputFile.Filename);
                            }
                        }
                    }
                    int iRnd = (new Random()).Next(21000);
                    if (iRnd == 666)
                    {
                        await botClient.SendTextMessageAsync(
                                      chatId: e.Message.Chat,
                                      replyToMessageId: e.Message.MessageId,
                                      text: "The Game. Perdiste..."
                                    );
                    }
                    Log($"{e.Message.From.FirstName}: {MessageText}", $"{e.Message.Chat.Id}.txt");
                }
                catch (Exception ex)
                {
                    Log(ex.Message, "log.txt");
                }
            }
        }
        public async static Task ReplyWithFile(ITelegramBotClient botClient, string sFileName, Message e)
        {

            using (var stream = System.IO.File.OpenRead(sFileName))
            {
                await botClient.SendPhotoAsync(chatId: e.Chat,
                                              photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, sFileName.Split('/').LastOrDefault()),
                                              disableNotification: true,
                                              replyToMessageId: e.MessageId);
            }
        }
        public static void DownloadFile(string sIn, string sOut)
        {
            using (WebClient client = new WebClient())
            {

                client.DownloadFile(new Uri(sIn), sOut);
            }
        }
        public static void Log(string sLog, string sLogFile)
        {
            if (!System.IO.File.Exists(sLogFile))
            {
                System.IO.File.Create(sLogFile).Dispose();
            }
            sLog = DateTime.Now.ToString("[yyyyMMdd-HHmmss]") + sLog;
            using (StreamWriter w = System.IO.File.AppendText(sLogFile))
            {
                w.WriteLine(sLog);
            }
            if (EnableConsoleLog)
                Console.WriteLine(sLogFile + " - " + sLog);
            if (EnableAdmChatLog)
                MandarMensaje("889467441 " + sLogFile + " - " + sLog);
        }
        static string ReplaceCommandText(string sIn, bool ReemplazarEnie = true)
        {
            return sIn.Replace("?", "").Replace(".", "").Replace("!", "").ToLower().Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ñ", ReemplazarEnie ? "n" : "ñ").Replace("\r\n", "");
        }
        public static string GoogleTranslate(string Text, string targetlan)
        {
            try
            {
                var service = new TranslateService(new Google.Apis.Services.BaseClientService.Initializer()
                {
                    ApiKey = googlekey
                });
                string[] srcText = new[] { Text };
                TranslationsListResponse response = service.Translations.List(srcText, targetlan).Execute();
                var translations = new List<string>();

                // We need to change this code...
                // currently this code
                foreach (Google.Apis.Translate.v2.Data.TranslationsResource translation in response.Translations)
                {
                    translations.Add(translation.TranslatedText);
                }
                Log($"[GoogleTranslate] Text={Text}, targetlan={targetlan}, translation={translations[0]}.", "log.txt");
                return translations[0];
            }
            catch (Exception ex)
            {
                Log($"[GoogleTranslate] Exception={ex.Message}.", "log.txt");
                return null;
            }
        }
    }
    public class Blacklist : List<string>
    {
        public bool Enabled { set; get; }
        public TimeRange EnabledTime { set; get; }
        public Blacklist(XDocument oXML)
        {
            EnabledTime = new TimeRange(new TimeSpan(5, 0, 0), new TimeSpan(21, 0, 0));
            Enabled = false;
            foreach (XElement oWord in oXML.Root.Elements())
            {
                Add(oWord.Value);
            }
        }
        public bool AddSave(string sWord, XDocument oXML)
        {
            if (!this.Any(x => x == sWord))
            {
                Add(sWord);
                oXML.Descendants("Palabras").SingleOrDefault()?.Add(new XElement("Palabra", sWord));
                oXML.Save(@"WordFile.xml");
                return true;
            }
            return false;
        }
        public bool RemoveSave(string sWord, XDocument oXML)
        {
            if (this.Any(x => x == sWord))
            {
                Remove(sWord);
                oXML.Descendants("Palabra").FirstOrDefault(x => x.Value == sWord)?.Remove();
                oXML.Save(@"WordFile.xml");
                return true;
            }
            return false;
        }
        public string FindMatch(string sIn, string sUser, bool isAdmin)
        {
            if (Enabled && EnabledTime.IsInRange())
            {
                string sRetu = sIn;
                System.Text.StringBuilder sPalabras = new System.Text.StringBuilder();
                foreach (string sReplace in this.Where(x => sIn.ToLower().Contains(x)))
                {
                    sPalabras.Append((string.IsNullOrWhiteSpace(sPalabras.ToString()) ? "" : ", y la ") + sReplace.Substring(0, 1).ToUpper() + "-Word");
                    sRetu = sRetu.ToLower().Replace(sReplace, sReplace.Substring(0, 1).ToUpper() + new string('*', sReplace.Length - 1));
                }
                if (isAdmin && sIn.ToLower().StartsWith("/replace"))
                {
                    var sNewUser = sIn.Split(' ')[1];
                    Console.WriteLine("Replace, original by " + sUser + ": " + sIn);
                    return (string.IsNullOrWhiteSpace(sPalabras.ToString()) ? null : sNewUser + " Dijo la " + sPalabras + ", rescatate gil." + Environment.NewLine + Environment.NewLine + sNewUser + ": " + sRetu.Substring("/replace  ".Length + sNewUser.Length));
                }
                return (string.IsNullOrWhiteSpace(sPalabras.ToString()) ? null : sUser + " Dijo la " + sPalabras + ", rescatate gil." + Environment.NewLine + Environment.NewLine + sUser + ": " + sRetu);
            }
            return null;
        }
    }
    [JsonObject(MemberSerialization.OptIn)]
    public class AmbitoBlue
    {
        public AmbitoBlue GetAmbitoBlueValue()
        {
            string sResponse;
            using (WebClient client = new WebClient())
            {

                sResponse = client.DownloadString(new Uri("https://mercados.ambito.com/dolar/informal/variacion"));
            }
            AmbitoBlue mAmbitoBlue = Newtonsoft.Json.JsonConvert.DeserializeObject<AmbitoBlue>(sResponse);
            if (decimal.TryParse(mAmbitoBlue.compra.Replace(',','.'), out decimal mCompra))
            {
                mAmbitoBlue.dCompra = mCompra;
            }
            if (decimal.TryParse(mAmbitoBlue.venta.Replace(',', '.'), out decimal mVenta))
            {
                mAmbitoBlue.dVenta = mVenta;
            }
            return mAmbitoBlue;
        }
        [JsonProperty]
        public string compra { get; set; }
        public decimal dCompra { set; get; }
        [JsonProperty]
        public string venta { get; set; }
        public decimal dVenta { set; get; }
        [JsonProperty]
        public string fecha { get; set; }
        [JsonProperty]
        public string variacion { get; set; }
    }

    public class TimeRange
    {
        public TimeRange(TimeSpan from, TimeSpan to)
        {
            From = from;
            To = to;
        }
        public TimeSpan From { get; set; }
        public TimeSpan To { get; set; }
        public bool IsInRange()
        {
            if (From < To)
            {
                return DateTime.Now.TimeOfDay >= From && DateTime.Now.TimeOfDay <= To;
            }
            else
            {
                return DateTime.Now.TimeOfDay >= From || DateTime.Now.TimeOfDay <= To;
            }
        }
    }
    public class Faqs : List<Freqaskedq>
    {
        public Freqaskedq ModificarFaq(string sKey, string sValue, XDocument oDoc, bool IsAdmin, long ChatId)
        {
            Freqaskedq oFaq = this.FirstOrDefault(x => x.Key == sKey && (!x.IsAdminBlocked || IsAdmin) && x.ChatId == ChatId);
            if (oFaq != null)
            {
                oFaq.Value = sValue;
                oDoc.Descendants("Faq").FirstOrDefault(x => x.Element("Key").Value == sKey && x.Element("ChatId").Value == ChatId.ToString()).Element("Value").Value = sValue;
            }
            else
            {
                if (!this.Any(x => x.Key == sKey && x.ChatId == ChatId))
                {
                    oFaq = new Freqaskedq(sKey, sValue, false, ChatId);
                    Add(oFaq);
                    oDoc.Descendants("Faqs").SingleOrDefault()?.Add(oFaq.ToXelement());
                }//else no tiene privilegios de adm
            }
            oDoc.Save(@"FaqFile.xml");
            return oFaq;
        }
        public void SetFaqBlock(Freqaskedq oFaq, bool IsBlocked, XDocument oDoc, long ChatId)
        {
            oFaq.IsAdminBlocked = IsBlocked;
            oDoc.Descendants("Faq").FirstOrDefault(x => x.Element("Key").Value == oFaq.Key && x.Element("ChatId").Value == ChatId.ToString()).Element("IsAdminBlocked").Value = IsBlocked.ToString();
        }
        public Faqs() { }
        public Faqs(XElement oRaw)
        {
            Freqaskedq oTemp;
            if (oRaw != null)
            {
                foreach (XElement oRow in oRaw.Elements())
                {
                    oTemp = new Freqaskedq(oRow);
                    Add(oTemp);
                }
            }
        }
    }
    public class Freqaskedq
    {
        public Freqaskedq() { }
        public Freqaskedq(string sKey, string sValue, bool bIsAdminBlocked, long lChatId)
        {
            Key = sKey;
            Value = sValue;
            IsAdminBlocked = bIsAdminBlocked;
            ChatId = lChatId;
        }
        public Freqaskedq(XElement oRaw)
        {
            Key = oRaw.Element("Key").Value;
            Value = oRaw.Element("Value").Value;
            IsAdminBlocked = oRaw.Element("IsAdminBlocked").Value.ToLower() == "true";
            if (long.TryParse(oRaw.Element("ChatId").Value, out long iChat))
                ChatId = iChat;
        }
        public XElement ToXelement()
        {
            return new XElement("Faq",
                    new XElement("Key", Key),
                    new XElement("Value", Value),
                    new XElement("IsAdminBlocked", IsAdminBlocked.ToString()),
                    new XElement("ChatId", ChatId.ToString())
                );
        }
        public string Key { set; get; }
        public string Value { set; get; }
        public bool IsAdminBlocked { set; get; }
        public long ChatId { set; get; }
    }
    public class Responses : List<Response>
    {
        public string BorrarRespuesta(string sKey, long iChat, XDocument oDoc)
        {
            var oFaqs = this.Where(x => x.Key == sKey && x.ChatId == iChat).ToArray();
            if (oFaqs.Count() == 1)
            {
                oDoc.Descendants("Response").FirstOrDefault(x => x.Element("Key").Value == sKey && x.Element("ChatId").Value == iChat.ToString()).Remove();
                Remove(oFaqs.First());
                oDoc.Save(@"RespFile.xml");
                return "Respuesta Borrada";
            }
            if (oFaqs.Any())
                return "No podes borrar una lista de esta manera";
            else
                return "Respuesta no encontrada";
        }
        public string BorrarRespuesta(string sKey, long iChat, XDocument oDoc, int iValue)
        {
            Response oFaq = this.FirstOrDefault(x => x.Key == sKey && x.ChatId == iChat && x.Value.MessageId == iValue);
            if (oFaq != null)
            {
                oDoc.Descendants("Response").FirstOrDefault(x => x.Element("Key").Value == sKey && x.Element("ChatId").Value == iChat.ToString() && x.Element("Value").Element("Message").Element("MessageId").Value == iValue.ToString()).Remove();
                Remove(oFaq);
                oDoc.Save(@"RespFile.xml");
                return $"Mensaje borrado de la lista: {sKey}";
            }
            return $"Mensaje no encontrado en la lista: {sKey}";
        }
        public Response[] Findresp(string sSearch, long iChat)
        {
            Response[] oFaq;
            oFaq = this.Where(x => x.ChatId == iChat && (sSearch.Contains(x.Key)
                                                        || (x.Key.StartsWith("\"") && x.Key.EndsWith("\"")
                                                            && (sSearch.EndsWith(" " + x.Key.Replace("\"", ""))
                                                                || sSearch.StartsWith(x.Key.Replace("\"", "") + " ")
                                                                || sSearch.Contains(" " + x.Key.Replace("\"", "") + " ")
                                                                || sSearch.Equals(x.Key.Replace("\"", "")))
                                                            ))).ToArray();
            if (!oFaq.Any())
            {
                oFaq = this.Where(x => x.ChatId == iChat && Regex.IsMatch(sSearch, "^" + Regex.Escape(x.Key).Replace("\\*", "(.*)") + "$")).ToArray();
            }
            return oFaq;
        }
        public Response[] ModificarRespuesta(string sKey, TGMessage iValue, XDocument oDoc, long iChat, bool EsLista)
        {
            var oFaqs = this.Where(x => x.Key == sKey && x.ChatId == iChat);
            Response oFaq;
            if (oFaqs.Count() == 1 && !EsLista)
            {
                oFaq = oFaqs.First();
                oFaq.Value = iValue;
                oDoc.Descendants("Response").FirstOrDefault(x => x.Element("Key").Value == sKey
                                                              && x.Element("ChatId").Value == iChat.ToString())
                                            .Remove();
                oDoc.Descendants("Responses").SingleOrDefault()?.Add(oFaq.ToXelement());
            }
            else
            {
                oFaq = new Response(sKey, iValue, iChat);
                Add(oFaq);
                oDoc.Descendants("Responses").SingleOrDefault()?.Add(oFaq.ToXelement());
            }
            oDoc.Save(@"RespFile.xml");
            return new Response[] { oFaq };
        }
        public bool ReemplazarRespuesta(TGMessage iValue, XDocument oDoc, long iChat, int OriginalMessageId)
        {
            Response oFaq = this.FirstOrDefault(x => x.ChatId == iChat && x.Value.MessageId == OriginalMessageId);
            if (oFaq != null)
            {
                oFaq.Value = iValue;
                oDoc.Descendants("Response").FirstOrDefault(x => x.Element("ChatId").Value == iChat.ToString()
                                                              && x.Element("Value").Value == OriginalMessageId.ToString())
                                            .Remove();
                oDoc.Descendants("Responses").SingleOrDefault()?.Add(oFaq.ToXelement());
                oDoc.Save(@"RespFile.xml");
                return true;
            }
            return false;
        }
        public Responses() { }
        public Responses(XElement oRaw)
        {
            Response oTemp;
            if (oRaw != null)
            {
                foreach (XElement oRow in oRaw.Elements())
                {
                    oTemp = new Response(oRow);
                    Add(oTemp);
                }
            }
        }
    }
    public class TGMessage
    {
        public int MessageId { set; get; }
        public long UserId { set; get; }
        public string UserName { set; get; }
        public string MessageText { set; get; }
        public string AudioURL { set; get; }
        public string ImgUr { set; get; }
        public string VideoURL { set; get; }
        public string DocumentURL { set; get; }
        public string MD5Hash { set; get; }
        public TGMessage() { }
        public TGMessage(XElement oRaw)
        {
            if (!oRaw.HasElements)
            {
                if (int.TryParse(oRaw.Value, out int lMessageId))
                    MessageId = lMessageId;
            }
            else
            {
                oRaw = oRaw.Element("Message");
                if (int.TryParse(oRaw.Element("MessageId").Value, out int lMessageId))
                    MessageId = lMessageId;
                if (long.TryParse(oRaw.Element("UserId").Value, out long lUserId))
                    UserId = lUserId;
                UserName = oRaw.Element("UserName").Value;
                MessageText = oRaw.Element("MessageText").Value;
                AudioURL = oRaw.Element("AudioURL").Value;
                ImgUr = oRaw.Element("ImgUr").Value;
                VideoURL = oRaw.Element("VideoURL").Value;
                MD5Hash = oRaw.Element("MD5Hash").Value;
                DocumentURL = oRaw.Element("DocumentURL").Value;
            }
        }
        public async Task SendMessage(ITelegramBotClient client, long ChatId, int RTMId)
        {
            if (!string.IsNullOrWhiteSpace(AudioURL))
            {
                using (var stream = System.IO.File.OpenRead(AudioURL))
                {
                    await client.SendAudioAsync(chatId: ChatId,
                                                  audio: new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, AudioURL.Split('/').LastOrDefault()),
                                                  replyToMessageId: RTMId,
                                                  disableNotification: true);
                }
            }
            else if (!string.IsNullOrWhiteSpace(ImgUr))
            {
                using (var stream = System.IO.File.OpenRead(ImgUr))
                {
                    await client.SendPhotoAsync(chatId: ChatId,
                                                  photo: new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, ImgUr.Split('/').LastOrDefault()),
                                                  disableNotification: true,
                                                  replyToMessageId: RTMId,
                                                  caption: MessageText);
                }
            }
            else if (!string.IsNullOrWhiteSpace(VideoURL))
            {
                using (var stream = System.IO.File.OpenRead(VideoURL))
                {
                    await client.SendVideoAsync(chatId: ChatId,
                                                  video: new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, VideoURL.Split('/').LastOrDefault()),
                                                  replyToMessageId: RTMId,
                                                  disableNotification: true);
                }
            }
            else if (!string.IsNullOrWhiteSpace(DocumentURL))
            {
                using (var stream = System.IO.File.OpenRead(DocumentURL))
                {
                    await client.SendDocumentAsync(chatId: ChatId,
                                                  document: new Telegram.Bot.Types.InputFiles.InputOnlineFile(stream, DocumentURL.Split('/').LastOrDefault()),
                                                  replyToMessageId: RTMId,
                                                  disableNotification: true);
                }
            }
            else if (!string.IsNullOrWhiteSpace(MessageText))
            {
                await client.SendTextMessageAsync(
                                      chatId: ChatId,
                                      replyToMessageId: RTMId,
                                      text: MessageText
                                    );
            }
            else
            {
                await client.ForwardMessageAsync(chatId: ChatId,
                                                 fromChatId: ChatId,
                                                 messageId: MessageId);//reemplazar el mensaje por el formato nuevo
            }
        }
        public async Task<string> GetByMessage(Message oRaw, ITelegramBotClient client, string DownloadFolder, string sToken)
        {
            string sRetu = "";
            MessageId = oRaw.MessageId;
            UserId = oRaw.From.Id;
            UserName = oRaw.From.FirstName;
            MessageText = oRaw.Text;
            if (oRaw.Audio != null || oRaw.Voice != null)
            {
                var mFile = await client.GetFileAsync(oRaw.Audio != null ? oRaw.Audio.FileId : oRaw.Voice.FileId);
                AudioURL = SaveFile(DownloadFolder, oRaw, mFile, sToken);
                if (string.IsNullOrEmpty(AudioURL))
                {
                    sRetu = "No se pudo guardar el archivo de audio";
                }
            }
            if (oRaw.Video != null || oRaw.VideoNote != null)
            {
                var mFile = await client.GetFileAsync(oRaw.Video != null ? oRaw.Video.FileId : oRaw.VideoNote.FileId);
                VideoURL = SaveFile(DownloadFolder, oRaw, mFile, sToken);
                if (string.IsNullOrEmpty(VideoURL))
                {
                    sRetu = "No se pudo guardar el archivo de video";
                }
            }
            if (oRaw.Sticker != null || (oRaw.Photo?.Any()).GetValueOrDefault())
            {
                var mFile = await client.GetFileAsync(oRaw.Sticker != null ? oRaw.Sticker.FileId : oRaw.Photo.FirstOrDefault().FileId);
                ImgUr = SaveFile(DownloadFolder, oRaw, mFile, sToken);
                if (string.IsNullOrEmpty(ImgUr))
                {
                    sRetu = "No se pudo guardar el archivo de imagen";
                }
            }
            if (oRaw.Document != null)
            {
                var mFile = await client.GetFileAsync(oRaw.Document.FileId);
                DocumentURL = SaveFile(DownloadFolder, oRaw, mFile, sToken);
                if (string.IsNullOrEmpty(DocumentURL))
                {
                    sRetu = "No se pudo guardar el documento";
                }
            }
            return sRetu;
        }
        public static async Task<string> GetMD5Hash(Message oRaw, ITelegramBotClient client, string sToken)
        {
            string sFileId = null;
            if (oRaw.Audio != null)
            {
                sFileId = oRaw.Audio.FileId;
            }
            if (oRaw.Video != null)
            {
                sFileId = oRaw.Video.FileId;
            }
            if (oRaw.Document != null)
            {
                sFileId = oRaw.Document.FileId;
            }
            if (oRaw.Sticker != null || (oRaw.Photo?.Any()).GetValueOrDefault())
            {
                sFileId = (oRaw.Sticker != null ? oRaw.Sticker.FileId : oRaw.Photo.FirstOrDefault().FileId);
            }
            if (!string.IsNullOrWhiteSpace(sFileId))
            {
                var mFile = await client.GetFileAsync(sFileId);
                try
                {
                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadFile($@"https://api.telegram.org/file/bot{sToken}/{mFile.FilePath}", "tempfile");
                    }
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        using (var stream = System.IO.File.OpenRead("tempfile"))
                        {
                            System.Text.StringBuilder oSb = new System.Text.StringBuilder("");
                            md5.ComputeHash(stream).AsQueryable().ToList().ForEach(x => oSb.Append(x.ToString("x2")));
                            return oSb.ToString();
                        }
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }
        public bool DoMessagesMatch(Message oRaw, string sMD5Hash)
        {
            bool bRetu = false;
            if (MessageText == oRaw.Text)
                bRetu = true;
            if (sMD5Hash == MD5Hash)
            {
                bRetu = true;
            }
            return bRetu;
        }
        private string SaveFile(string DownloadFolder, Message oRaw, Telegram.Bot.Types.File mFile, string sToken)
        {
            string FilePath = $"{DownloadFolder}{oRaw.Chat.Id.ToString()}-{oRaw.MessageId.ToString()}.{mFile.FilePath.Split('.').LastOrDefault()}";
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.DownloadFile($@"https://api.telegram.org/file/bot{sToken}/{mFile.FilePath}", FilePath);//check extension
                }
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    using (var stream = System.IO.File.OpenRead(FilePath))
                    {
                        System.Text.StringBuilder oSb = new System.Text.StringBuilder("");
                        md5.ComputeHash(stream).AsQueryable().ToList().ForEach(x => oSb.Append(x.ToString("x2")));
                        MD5Hash = oSb.ToString();
                    }
                }
            }
            catch
            {
                FilePath = null;
            }
            return FilePath;
        }
        public XElement ToXelement()
        {
            return new XElement("Message",
                    new XElement("MessageId", MessageId),
                    new XElement("UserName", UserName),
                    new XElement("UserId", UserId),
                    new XElement("MessageText", MessageText),
                    new XElement("AudioURL", AudioURL),
                    new XElement("ImgUr", ImgUr),
                    new XElement("VideoURL", VideoURL),
                    new XElement("DocumentURL", DocumentURL),
                    new XElement("MD5Hash", MD5Hash)
                );
        }
    }
    public class Response
    {
        public Response() { }
        public Response(string sKey, TGMessage iVal, long iChat)
        {
            Key = sKey;
            Value = iVal;
            ChatId = iChat;
        }
        public Response(XElement oRaw)
        {
            Key = oRaw.Element("Key").Value;
            Value = new TGMessage(oRaw.Element("Value"));
            if (long.TryParse(oRaw.Element("ChatId").Value, out long iChat))
                ChatId = iChat;
        }
        public XElement ToXelement()
        {
            return new XElement("Response",
                    new XElement("Key", Key),
                    new XElement("Value", Value.ToXelement()),
                    new XElement("ChatId", ChatId)
                );
        }
        public string Key { set; get; }
        public TGMessage Value { set; get; }
        public long ChatId { set; get; }
    }
}
