using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using System.Collections.ObjectModel;
using System.Windows;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CS_HW_WPF_10._5;
using System.ComponentModel.DataAnnotations;
using System.Windows.Data;
using System.Runtime.Intrinsics.X86;

namespace F_Bot_CS_9._4
{ 
     class F_Bot_class
    {
        private MainWindow wind;
        private TelegramBotClient F_bot;
        public struct MsgLog_F_Bot
        {
            public string Time { get; set; }

            public long Id { get; set; }

            public string Msg { get; set; }

            public string FirstName { get; set; }

            public MsgLog_F_Bot(string Time, string Msg, string FirstName, long Id)
            {
                this.Time = Time;
                this.Msg = Msg;
                this.FirstName = FirstName;
                this.Id = Id;
            }
        }
        public ObservableCollection<MsgLog_F_Bot> FBotMessegeObsCollection { get; set; }
        
        public F_Bot_class(MainWindow W, string Token_Path = @"D:\F_Bot_token.txt")
        {
            this.FBotMessegeObsCollection = new ObservableCollection<MsgLog_F_Bot>();
            this.wind = W;
            F_bot = new TelegramBotClient(System.IO.File.ReadAllText(Token_Path)); // файл с токеном бота
            
        }
       
        public void Start()
        {
            Debug.WriteLine("Вызван метод Start.");
            var files_list = new List<string>();
            // тестируем бота и получаем необходимую информацию
            var user_f_bot = F_bot.GetMeAsync().Result;
            var receverOptions = new ReceiverOptions
            {
                AllowedUpdates = new UpdateType[]
                {
                    UpdateType.Message
                }
            };
            // запускаем основной метод
            F_bot.StartReceiving(F_UpdateHandler, F_bot_ErrorHandler, receverOptions);
            Debug.WriteLine($"Start listening updates from {user_f_bot}");
            Console.ReadLine();
        }
        /// <summary>
        /// Метод вызываемый при получении обновления <paramref name="update"/> в <paramref name="F_bot"/>. 
        /// В консоль выводится сообщения по ключевым стадиям выполнения программы.
        /// Если обновление представляет собой файл документ, аудио или изображение, то оно сохраняется в существующую или вновь созданную директорию.
        /// Если обновеление текс, то ,при соответствии текста определенной команде, выполняется соответствующий метод.
        /// </summary>
        /// <param name="F_bot"></param>
        /// <param name="update"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        private async Task F_UpdateHandler(ITelegramBotClient F_bot, Update update, CancellationToken arg3)
        {
            string main_dir_name = $"E:\\FBot";
            DirectoryInfo main_dir = new DirectoryInfo(main_dir_name);
            main_dir.Create();
            
            var recived_massage = update.Message;
            if(recived_massage != null)
            {   
                main_dir.CreateSubdirectory($"Downloads_from_{recived_massage.From.FirstName}_{recived_massage.From.Id}");
                
                DirectoryInfo[] dirs = main_dir.GetDirectories($"*{recived_massage.From.Id}");
                DirectoryInfo dir = dirs[0];
                string dirName = dir.FullName;
                Debug.WriteLine($"Создана папка для пользователя {recived_massage.From.FirstName} в {dir.FullName}");
                // создаем метод получения списка строк с информацией о файлах в указанной директории, 
                // куда будут загружаться файлы и откуда они будут выгружаться при соответствующем запросе
                List<string> GetListOfFiles(DirectoryInfo dir, string dirName)
                {
                    List<string> files = new List<string>();
                    var files_array = dir.GetFiles();

                    Debug.WriteLine($"List of files in {dirName} geted:");

                    for (int i = 0; i < files_array.Length; i++)
                    {
                        string[] KB_or_bytes = new string[2];
                        KB_or_bytes[0] = "KB";
                        KB_or_bytes[1] = "bytes";
                        int с = 1;
                        int d = 1;
                        if (files_array[i].Length > 1024)
                        {
                            с = 0;
                            d = 1024;
                        }
                        else;
                        string file_info = $"\n #/{i + 1}\n {files_array[i].Name}\n {files_array[i].FullName}\n {files_array[i].Length / d} {KB_or_bytes[с]}\n";
                        Debug.WriteLine(file_info);
                        files.Add(file_info);
                    }
                    return files;
                };
                // cоздаем клавиатуры  бота для более удобного взаимодействия
                ReplyKeyboardMarkup Keyboard_1 = new(new[] { new KeyboardButton[] { "Show list of files", "Download all files", "Exit" }, })
                {
                    ResizeKeyboard = true
                };
                ReplyKeyboardMarkup Keyboard_2 = new(new[] { new KeyboardButton[] { "Show list of files", "Exit" }, })
                {
                    ResizeKeyboard = true
                };
                // в зависимости от типа полученного сообщения вормируется соответсвующий ответ. Можно скинуть боту различные файлы (документы, аудио, фото), 
                // запросить список файлов в директории
                // скачать выбранный файл или скачать все файлы сразу
                if (update.Type == UpdateType.Message)
                {
                    if (recived_massage == null) return;
                    var type = recived_massage.Type;
                    var chat_id = recived_massage.Chat.Id;
                    var recived_text = recived_massage.Text;

                    Debug.WriteLine($"Message recived. \n{type.ToString()}: {recived_text}");
                    if (recived_massage.Type == MessageType.Text)
                    {
                        if (recived_text == null) return;
                        wind.Dispatcher.Invoke(() =>
                        {
                            Debug.WriteLine("СРАБОТАЛ Dispatcher.Invoke");
                            FBotMessegeObsCollection.Add(
                        new MsgLog_F_Bot(
                            DateTime.Now.ToLongTimeString(), recived_massage.Text, recived_massage.From.FirstName, recived_massage.From.Id));
                            string obscol = "";
                            for (int i = 0; i < FBotMessegeObsCollection.Count; ++i)
                            {
                                obscol += $"{FBotMessegeObsCollection[i].Msg} \n";
                            }
                            Debug.WriteLine($" В ObservableCollection находится следующие сообщения \n{obscol}");
                        });
                        if (recived_text == "/start")
                        {
                            Message start_message = await F_bot.SendTextMessageAsync(
                    chatId: chat_id,
                    text: "This is a bot for saving files to disk or download already uploaded files. ",
                    replyMarkup: Keyboard_1,
                    cancellationToken: arg3);
                            Message choose_message = await F_bot.SendTextMessageAsync(
                    chatId: chat_id,
                    text: "Choose the option below:",
                    replyMarkup: Keyboard_1,
                    cancellationToken: arg3);
                        }
                        if (recived_text == "Show list of files")
                        {
                            List<string> files = GetListOfFiles(dir, dirName);

                            string message_list_of_files = "";
                            for (int i = 0; i < files.Count; i++)
                            {
                                message_list_of_files += files[i];
                            }
                            message_list_of_files += $"\n Press the number of file to download it.";
                            Message files_list_responce_msg = await F_bot.SendTextMessageAsync(
                                chatId: chat_id,
                                text: message_list_of_files,
                                replyMarkup: Keyboard_1,
                                cancellationToken: arg3);

                        }
                        if (recived_text == "Download all files")
                        {
                            var files_array = dir.GetFiles();
                            for (int i = 0; i < files_array.Length; i++)
                            {
                                await using Stream stream = System.IO.File.OpenRead($"{files_array[i].FullName}");
                                Message message = await F_bot.SendDocumentAsync(
                                    chatId: chat_id,
                                    document: new InputOnlineFile(content: stream, fileName: $"{files_array[i].Name}")
                                  );
                            }
                            Message sentMessage = await F_bot.SendTextMessageAsync(
                                chatId: chat_id,
                                text: "Files already sended. \n Type /start if you need to see commands again.",
                                replyMarkup: new ReplyKeyboardRemove(),
                                cancellationToken: arg3);
                        }
                        if (recived_text == "Exit")
                        {

                            Message sentMessage = await F_bot.SendTextMessageAsync(
                                chatId: chat_id,
                                text: "Type /start if you need to see commands again. ",
                                replyMarkup: new ReplyKeyboardRemove(),
                                cancellationToken: arg3);
                        }

                        if (recived_text.StartsWith("/") == true && recived_text != "/start")
                        {
                            var f = recived_text.Replace("/", "");
                            int t = Convert.ToInt32(f);
                            var files_array = dir.GetFiles();
                            await using Stream stream = System.IO.File.OpenRead($"{files_array[t - 1].FullName}");
                            Message message = await F_bot.SendDocumentAsync(
                                chatId: chat_id,
                                document: new InputOnlineFile(content: stream, fileName: $"{files_array[t - 1].Name}")
                              );
                            Message sentMessage = await F_bot.SendTextMessageAsync(
                                chatId: chat_id,
                                text: "Type /start if you need to see commands again. ",
                                replyMarkup: new ReplyKeyboardRemove(),
                                cancellationToken: arg3);
                        }
                    }
                    if (type == MessageType.Document)
                    {
                        var file_id = recived_massage.Document.FileId;
                        var file_name = recived_massage.Document.FileName;
                        Debug.WriteLine($"\nName: {file_name}\nFile_ID: {file_id} ");

                        if (!dir.Exists)
                        {
                            dir.Create();
                            Debug.WriteLine($"New directory for downloads created:{dir.FullName} ");
                        }
                        else
                        {
                            Debug.WriteLine($"Directory for downloads exist: {dir.FullName}");
                        }
                        FileStream direction = new FileStream($"{dir.FullName}\\{file_name}", FileMode.Create); ; ;
                        var file = F_bot.GetFileAsync(file_id).Result;
                        Debug.WriteLine("FilePath received.");
                        await F_bot.DownloadFileAsync(file.FilePath, direction);
                        Debug.WriteLine($"File {file_name} downloaded to {dirName}");
                        var files = dir.GetFiles();
                        Debug.WriteLine($"List of files in {dirName} geted:");
                        for (int i = 0; i < files.Length; i++)
                        {
                            Debug.WriteLine($"\n #{i + 1}\n {files[i].Name}\n {files[i].FullName}\n {files[i].Length}\n");
                        }
                        Debug.WriteLine($"List of files in {dirName} geted:");
                        Message sentMessage = await F_bot.SendTextMessageAsync(
                                chatId: chat_id,
                                text: $"The file was uploaded to disk {dir.FullName}",
                                replyMarkup: Keyboard_2,
                                cancellationToken: arg3);

                        direction.Close();
                    }
                    if (type == MessageType.Audio)
                    {
                        var file_id = recived_massage.Audio.FileId;
                        var file_name = recived_massage.Audio.FileName;
                        Debug.WriteLine($"\nName: {file_name}\nFile_ID: {file_id} ");

                        if (!dir.Exists)
                        {
                            dir.Create();
                            Debug.WriteLine($"New directory for downloads created:{dir.FullName} ");
                        }
                        else
                        {
                            Debug.WriteLine($"Directory for downloads exist: {dir.FullName}");
                        }
                        FileStream direction = new FileStream($"{dir.FullName}\\{file_name}", FileMode.Create); ; ;
                        var file = F_bot.GetFileAsync(file_id).Result;
                        Debug.WriteLine("FilePath received.");
                        await F_bot.DownloadFileAsync(file.FilePath, direction);
                        Debug.WriteLine($"File {file_name} downloaded to {dirName}");
                        GetListOfFiles(dir, dirName);
                        Message sentMessage = await F_bot.SendTextMessageAsync(
                                chatId: chat_id,
                                text: $"The file was uploaded to disk {dir.FullName}",
                                replyMarkup: Keyboard_2,
                                cancellationToken: arg3);
                        direction.Close();
                    }
                    if (type == MessageType.Photo)
                    {
                        var file_photo = recived_massage.Photo;
                        var file_id = file_photo[3].FileId;
                        string user_name = recived_massage.From.FirstName;

                        Debug.WriteLine($"\nFile_ID: {file_id} \nFrom: {user_name}");

                        if (!dir.Exists)
                        {
                            dir.Create();
                            Debug.WriteLine($"New directory for downloads created:{dir.FullName} ");
                        }
                        else
                        {
                            Debug.WriteLine($"Directory for downloads exist: {dir.FullName}");
                        }
                        string date = recived_massage.Date.ToLongTimeString();
                        date = date.Replace(":", "_");
                        FileStream direction = new FileStream($"{dir.FullName}\\{user_name}_{date}.png", FileMode.Create); ; ;
                        var file = F_bot.GetFileAsync(file_id).Result;
                        Debug.WriteLine("FilePath received.");
                        await F_bot.DownloadFileAsync(file.FilePath, direction);
                        Debug.WriteLine($"File {user_name} downloaded to {dirName}");
                        var files = dir.GetFiles();
                        Debug.WriteLine($"List of files in {dirName} geted:");
                        for (int i = 0; i < files.Length; i++)
                        {
                            Debug.WriteLine($"\n #{i + 1}\n {files[i].Name}\n {files[i].FullName}\n {files[i].Length}\n");
                        }
                        Message sentMessage = await F_bot.SendTextMessageAsync(
                                chatId: chat_id,
                                text: $"The file was uploaded to disk {dir.FullName}",
                                replyMarkup: Keyboard_2,
                                cancellationToken: arg3);
                        direction.Close();
                    }

                }
            }
            
        }
        private static Task F_bot_ErrorHandler(ITelegramBotClient arg1, Exception arg2, CancellationToken arg4)
        {
            Debug.WriteLine(arg2.Message);
            return null;
        }
        /// <summary>
        /// Фильтрует коллекцию полученных сообщений <paramref name="MsgLog"/> по определенному <paramref name="selected_ID"/>.
        /// Сериализует новую коллекцию в формат JSON и сохраняет файл в основную папку F_Bot
        /// </summary>
        /// <param name="MsgLog"></param>
        /// <param name="selected_ID"></param>
        public void F_Bot_save_messeges_to_json (ObservableCollection<MsgLog_F_Bot> MsgLog,string selected_ID)
        {
            ObservableCollection<MsgLog_F_Bot> filtered_MsgLog = new ObservableCollection<MsgLog_F_Bot>();
            Debug.WriteLine("Метод F_Bot_save_messeges_to_json запущен");
            Debug.WriteLine("Проверяем коллекцию по индексу");
            for (int i = 0; i < MsgLog.Count; i++)
            {
                
                if (MsgLog[i].Id.ToString() == selected_ID)
                {
                    filtered_MsgLog.Add(new MsgLog_F_Bot(MsgLog[i].Time, MsgLog[i].Msg, MsgLog[i].FirstName, MsgLog[i].Id));
                }
                else continue;
            }
            Debug.WriteLine("Сообщения отсортированы");
            
            FileStream fileStream = new FileStream($"E:\\FBot\\Messages_from_{filtered_MsgLog[0].FirstName}_{filtered_MsgLog[0].Id}.json", FileMode.Create);
            Debug.WriteLine("Открыт файловый поток");
            JsonSerializer.Serialize(fileStream,filtered_MsgLog);
            Debug.WriteLine($"Соощения сохранены в {fileStream.Name}");
            fileStream.Close();
        }
        /// <summary>
        /// Открыват папку с файлами полученными от пользователся с указанным <paramref name="selected_first_name"/> и <paramref name="selected_ID"/>
        /// </summary>
        /// <param name="selected_first_name"></param>
        /// <param name="selected_ID"></param>
        public void F_Bot_show_uploaded_files (string selected_first_name, string selected_ID)
        {
            
            Process Proc = new Process();
            Proc.StartInfo.FileName = "explorer";
            Proc.StartInfo.Arguments = $"E:\\FBot\\Downloads_from_{selected_first_name}_{selected_ID}";
            Proc.Start();
            Proc.Close();
        }
        /// <summary>
        /// Посылает сообщение с текстом <paramref name="answer_text"/> пользоваетелю с указанным <paramref name="chat_id"/>
        /// </summary>
        /// <param name="chat_id"></param>
        /// <param name="answer_text"></param>
        public void F_Bot_send_the_answer (string chat_id,string answer_text)
        {
            F_bot.SendTextMessageAsync(chat_id, answer_text);
        }

    }
}