using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

namespace voiceduck
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        VNDB _db = new VNDB();
        Dictionary<int, Voice> voiceDict = new Dictionary<int, Voice>();
        private readonly static int threadPool = 5;
        Thread[] threads = new Thread[threadPool];
        BitmapImage loadingImage;

        public MainWindow()
        {
            InitializeComponent();
            foreach (var voice in VNDB.voices)
            {
                Run(voice.Key, voice.Value);
            }
            loadingImage = new BitmapImage();
            loadingImage.BeginInit();
            loadingImage.UriSource = new Uri(System.IO.Directory.GetCurrentDirectory() + @"\img\loading.jpg");
            loadingImage.EndInit();



        }


        async void Run(int id, string nickname = "", bool update = false)
        {
            Voice voice = await _db.GetVoice(id);
            if (voice != null)
            {
                if (nickname != "")
                    voice.Nickname = nickname;
                VNDB.voices[id] = voice.Nickname;
                if (update)
                    _db.Update();
                VoiceListBox.Items.Add(voice);
            }
        }

        private void VoiceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Voice voice = (Voice)VoiceListBox.SelectedItem;
            VNListBox.ItemsSource = voice.vns;
        }

        async private void VNListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VN vn = (VN)VNListBox.SelectedItem;
            if (vn == null)
                return;
//            if(vn.Color != "yellow")
            {
                vn.Color = "green";
                CharacterInfoListBox.Items.Clear();
                CharacterInfoListBox.Items.Add(vn.characters[0].name.Text);
                CharacterImage.Source = loadingImage;
                CharacterImage.Source = await _db.GetCharacterImage(vn.id, vn.characters[0].id);
                
            }
        }


        private void FindVoiceBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int id;
                if(int.TryParse(FindVoiceBox.Text, out id))
                {
                    Run(id, "" , true);
                    FindVoiceBox.Text = "";
                }
            }
        }

        private void VoiceListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Voice voice = (Voice)VoiceListBox.SelectedItem;
            if (voice == null)
                return;

            InputBox win = new InputBox("별칭");
            win.Owner = this;
            win.ShowDialog();

            if (win.TextBox.Text.Length > 0)
            {
                voice.Nickname = win.TextBox.Text;
                VNDB.voices[voice.id] = voice.Nickname;
                _db.Update();
            }
        }

        private async void FindVNBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string text = FindVNBox.Text;
                //FindVNBox.Text = "";
                VNListBox2.ItemsSource = await _db.GetVNSearch(text);
            }

        }

        private void CharacterListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void VNListBox2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VN vn = (VN)VNListBox2.SelectedItem;
            if (vn == null)
                return;
            if (vn.characters.Count == 0)
                await _db.GetMoreVNInfo(vn);
            CharacterListBox.ItemsSource = null;
            CharacterListBox.ItemsSource = vn.characters;
            VNImage.Source = loadingImage;
            VNImage.Source = await _db.GetVNImage(vn.id, vn.image);
            VNInfoListBox.Items.Clear();
            VNInfoListBox.Items.Add(vn.name.japName);
            VNInfoListBox.Items.Add(vn.name.engName);
            VNInfoListBox.Items.Add(vn.playTIme);
            VNInfoListBox.Items.Add(vn.developer);
        }

        private void VNListBox2_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void CharacterListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Character character = (Character)CharacterListBox.SelectedItem;
            if (character == null)
                return;
            if (character.voice == null)
                return;
            Run(character.voice.id, "", true);
        }
    }
}
