using System;
using System.Collections.Generic;
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

namespace voiceduck
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            VNDB _db = new VNDB();
            Voice voice = _db.GetVoice(5);

            List<string> items = new List<string>();

            VoiceListBox.Items.Add(voice.names[0].Text);

            for(int i=0; i < voice.vns.Count; i++)
            {
                items.Add("[" + voice.vns[i].date + "] " + voice.characters[i].name.Text + " - " + voice.vns[i].name.Text);
            }

            VNListBox.ItemsSource = items;
        }

        private void VoiceListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void VNListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void VNListBox_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
