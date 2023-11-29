using F_Bot_CS_9._4;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace CS_HW_WPF_10._5
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        F_Bot_class f_bot;
         public bool reply_elements_enable = false;
            
            
        public MainWindow()
        {
            InitializeComponent();
            f_bot = new F_Bot_class(this);
            logList.ItemsSource = f_bot.FBotMessegeObsCollection;
            
            //ListBoxItem listBoxItem = new ListBoxItem();
            send_the_answer_button.IsEnabled = reply_elements_enable;
            //download_chat_history_button.IsEnabled=reply_elements_enable;
            //show_list_of_files_button.IsEnabled=reply_elements_enable;
            //answer_textbox.IsEnabled=reply_elements_enable;
            
        }
        

        private void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            f_bot.Start();
            Title = "Lestening FBot";
            Button_Start.IsEnabled = false;
            Button_Start.Visibility = Visibility.Hidden;
            _Start_text_block.Visibility = Visibility.Hidden;
           
        }

        private void Messege_listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Download_chat_history_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"В коллекции: {f_bot.FBotMessegeObsCollection.Count} элементов");
            f_bot.F_Bot_save_messeges_to_json(f_bot.FBotMessegeObsCollection,reply_user_ID.Text);

            Title = "Messegaes saved";
        }

        private void show_list_of_files_button_Click(object sender, RoutedEventArgs e)
        { 
            f_bot.F_Bot_show_uploaded_files(reply_user_first_name.Text, reply_user_ID.Text);
        }

        private void send_the_answer_button_Click(object sender, RoutedEventArgs e)
        {
            f_bot.F_Bot_send_the_answer(reply_user_ID.Text, answer_textbox.Text);
        }

       


        private void answer_textbox_IsKeyboardFocusedChanged_1(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (answer_textbox.IsKeyboardFocused == true)
            {
                answer_textbox.Text = "";
                send_the_answer_button.IsEnabled = true;
            }
            
            if (answer_textbox.IsKeyboardFocused == false)
            {
                answer_textbox.Text = "Reply...";
                send_the_answer_button.IsEnabled = false;
            }
        }





        //private void Carrent_item_Selected(object sender, RoutedEventArgs e)
        //{
        //    show_list_of_files_button.IsEnabled = true;
        //    download_chat_history_button.IsEnabled = true;
        //    answer_textbox.IsEnabled = true;
        //}
    }
}
