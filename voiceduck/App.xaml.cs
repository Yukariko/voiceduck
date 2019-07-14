using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Net;
using System.IO;

namespace voiceduck
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {        
    }



    public class VNDB
    {
        private readonly string dbPath = System.IO.Directory.GetCurrentDirectory() + @"\db.txt";
        public static Dictionary<int, string> voices = new Dictionary<int, string>();
  
        public VNDB()
        {
            DirectoryInfo dir = new DirectoryInfo(System.IO.Directory.GetCurrentDirectory() + @"\cache");
            if (!dir.Exists)
                dir.Create();
            if (!File.Exists(@"db.txt"))
            {
                File.Create(@"db.txt");
            }
            Init();
        }

        public void Init()
        {
            using (System.IO.StreamReader file = new System.IO.StreamReader(dbPath))
            {
                int count;
                if (!Int32.TryParse(file.ReadLine(), out count))
                {
                    return;
                }
                for(int i=0; i < count; i++)
                {
                    string read = file.ReadLine();
                    string[] tokens = read.Split(new char[1] { ' ' }, 2);
                    int id;
                    if (!Int32.TryParse(tokens[0], out id))
                        return;
                    string nickname = tokens[1];
                    voices[id] = nickname;
                }
            }
        }

        public static string CreateCachePath(int vnId, int characterId)
        {
            return System.IO.Directory.GetCurrentDirectory() + @"\cache\" + vnId + "_" + characterId + ".jpg";
        }
        public static int ExtractId(string str)
        {
            int id = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if ('0' <= str[i] && str[i] <= '9')
                    id = id * 10 + (int)(str[i] - '0');
            }
            return id;
        }
        async public Task<Voice> GetVoice(int id)
        {
            Voice voice = new Voice(id);
            string url = "https://vndb.org/s" + id;

            try
            {
                var web = new HtmlAgilityPack.HtmlWeb();
                HtmlDocument doc = await web.LoadFromWebAsync(url);
                var profile = doc.DocumentNode.SelectSingleNode("//body/div[@id='maincontent']/div[@class='mainbox staffpage']");

                string engName;
                string japName;

                engName = profile.SelectSingleNode("./h1").InnerText;
                japName = profile.SelectSingleNode("./h2").InnerText;

                Name name = new Name(engName, japName);
                voice.PushName(name);

                var aliases = profile.SelectNodes(".//tr[@class='nostripe']");

                foreach (var alias in aliases)
                {
                    var elem = alias.SelectNodes("./td");
                    engName = elem[0].InnerText;
                    japName = elem[1].InnerText;
                    name = new Name(engName, japName);
                    voice.PushName(name);
                }

                var vns = doc.DocumentNode.SelectNodes("//div[@class='mainbox browse staffroles']/table/tr");
                foreach (var vn in vns)
                {
                    var characterTag = vn.SelectSingleNode("./td[@class='tc3']/a");
                    if (characterTag == null)
                        continue;
                    var date = vn.SelectSingleNode("./td[@class='tc2']").InnerText;
                    var titleTag = vn.SelectSingleNode("./td[@class='tc1']/a");

                    int characterId = ExtractId(characterTag.GetAttributeValue("href", ""));
                    japName = characterTag.GetAttributeValue("title", "");
                    engName = characterTag.InnerText;

                    var character = new Character(characterId, new Name(engName, japName));
                    voice.PushCharacter(character);

                    int vnId = ExtractId(titleTag.GetAttributeValue("href", ""));
                    japName = titleTag.GetAttributeValue("title", "");
                    engName = titleTag.InnerText;
                    name = new Name(engName, japName);

                    string color = "red";
                    if (File.Exists(CreateCachePath(vnId, characterId)))
                        color = "green";

                    var _vn = new VN(vnId, name, date);
                    _vn.characters.Add(character);
                    _vn.Color = color;

                    voice.PushVN(_vn);

                }

                voice.vns.Reverse();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

            return voice;
        }

        async public Task<string> GetCharacterImageSrc(int id)
        {
            string url = "http://vndb.org/c" + id;

            //            try
            {
                var web = new HtmlAgilityPack.HtmlWeb();
                HtmlDocument doc = await web.LoadFromWebAsync(url);

                var profile = doc.DocumentNode.SelectSingleNode("//div[@class='charimg']/img");
                if (profile == null)
                    return null;

                string src = profile.GetAttributeValue("src", "");
                return src;
            }

            return null;
        }

        async public Task<BitmapImage> GetCharacterImage(int vnId, int characterId)
        {
            var bitmapImage = new BitmapImage();
            string localImagePath = CreateCachePath(vnId, characterId);
            try
            {
                if (!File.Exists(localImagePath))
                {
                    string img = await GetCharacterImageSrc(characterId);
                    if (img == null)
                        return null;

                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(img, localImagePath);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            try
            {   
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(localImagePath);
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        
        async public Task<List<VN>> GetVNSearch(string searchString)
        {
            List<VN> result = new List<VN>();
            string url = "https://vndb.org/v/all?sq=" + searchString;

            try
            {
                var web = new HtmlAgilityPack.HtmlWeb();
                web.CaptureRedirect = true;

                HtmlDocument doc = await web.LoadFromWebAsync(url);
                var searchList = doc.DocumentNode.SelectNodes("//table[@class='stripe']/tr");

                foreach (var search in searchList)
                {
                    string id = search.SelectSingleNode("./td[@class='tc1']/a").GetAttributeValue("href", "");
                    string japName = search.SelectSingleNode("./td[@class='tc1']/a").GetAttributeValue("title", "");
                    string engName = search.SelectSingleNode("./td[@class='tc1']").InnerText;
                    string date = search.SelectSingleNode("./td[@class='tc4']").InnerText;
                    VN vn = new VN(ExtractId(id), new Name(engName, japName), date);
                    result.Add(vn);
                }

            }
            catch(Exception e)
            {

            }

            return result;
        }
        
        public async Task<VN> GetMoreVNInfo(VN vn)
        {
            string url = "https://vndb.org/v" + vn.id;

            try
            {
                var web = new HtmlAgilityPack.HtmlWeb();
                HtmlDocument doc = await web.LoadFromWebAsync(url);

                var details = doc.DocumentNode.SelectNodes("//div[@class='vndetails']/table/tr");
                foreach (var detail in details)
                {
                    var key = detail.SelectSingleNode("./td").InnerText;
                    if (key == "Length")
                        vn.playTIme = detail.SelectNodes("./td")[1].InnerText;
                    else if (key == "Developer")
                        vn.developer = detail.SelectNodes("./td")[1].InnerText;
                }

                var imagePath = doc.DocumentNode.SelectSingleNode("//div[@class='vnimg']//img");
                if (imagePath != null)
                {
                    vn.image = imagePath.GetAttributeValue("src", "");
                }

                var characterList = doc.DocumentNode.SelectNodes("//div[contains(@class,'charsum_bubble')]");
                foreach (var character in characterList)
                {
                    int id = ExtractId(character.SelectSingleNode("./div[@class='name']/a").GetAttributeValue("href", ""));
                    string engName = character.SelectSingleNode("./div[@class='name']/a").InnerText;
                    string japName = character.SelectSingleNode("./div[@class='name']/a").GetAttributeValue("title", "");

                    Character _character = new Character(id, new Name(engName, japName));

                    if (character.SelectSingleNode("./div[@class='actor']") != null)
                    {
                        int voiceId = ExtractId(character.SelectSingleNode("./div[@class='actor']/a").GetAttributeValue("href", ""));
                        string voiceJapName = character.SelectSingleNode("./div[@class='actor']/a").GetAttributeValue("title", "");
                        string voiceEngName = character.SelectSingleNode("./div[@class='actor']/a").InnerText;
                        _character.voice = new Voice(voiceId, new Name(voiceEngName, voiceJapName));
                        if (voices.ContainsKey(voiceId))
                            _character.voice.Nickname = voices[voiceId];
                    }



                    vn.characters.Add(_character);
                }

            }
            catch (Exception e)
            {

            }

            return vn;
        }
        async public Task<BitmapImage> GetVNImage(int id, string url)
        {
            var bitmapImage = new BitmapImage();
            string localImagePath = CreateCachePath(id, 0);
            try
            {
                if (!File.Exists(localImagePath))
                {
                    if (url == null)
                        return null;

                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(new Uri(url), localImagePath);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            try
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(localImagePath);
                bitmapImage.EndInit();
                return bitmapImage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void Update()
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(dbPath))
            {
                file.WriteLine(voices.Count);
                foreach (var voice in voices)
                {
                    file.WriteLine(voice.Key + " " + voice.Value);
                }
                file.Close();
            }
        }
    }


    public class Voice : INotifyPropertyChanged
    {
        public readonly int id;
        private string nickname;
        public string Nickname
        {
            get
            {
                if (nickname != null)
                    return nickname;
                if (VNDB.voices.ContainsKey(id))
                    return VNDB.voices[id];
                return names[0].Text;
            }
            set
            {
                nickname = value;
                OnPropertyChanged("Nickname");
            }
        }
        public readonly List<Name> names = new List<Name>();
        public readonly List<VN> vns = new List<VN>();
        public readonly List<Character> characters = new List<Character>();

        public event PropertyChangedEventHandler PropertyChanged;

        public string Text { get { return names[0].Text; } }

        public Voice(int id)
        {
            this.id = id;
        }

        public Voice(int id, Name name)
        {
            this.id = id;
            names.Add(name);
        }

        public void PushName(Name name)
        {
            names.Add(name);
        }

        public void PushVN(VN vn)
        {
            vns.Add(vn);
        }

        public void PushCharacter(Character character)
        {
            characters.Add(character);
        }

        public void PrintNames()
        {
            foreach (Name name in names)
            {
                name.Print();
            }
        }

        public void PrintCharacters()
        {
            foreach (Character character in characters)
            {
                character.Print();
            }
        }

        public void PrintVNs()
        {
            foreach (VN vn in vns)
            {
                vn.Print();
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VN : INotifyPropertyChanged
    {
        public readonly int id;
        public readonly Name name;
        public readonly string date;
        public string image;
        public string playTIme;
        public string developer;
        private string color;
        public string Color { get { return color; } set { color = value; OnPropertyChanged("Color"); } }

        public string Text { get { return "[" + date + "] " + characters[0].name.Text + " - " + name.Text; } }
        
        public string SearchName { get { return "[" + date + "] " + name.engName; } }

        private bool seen = false;
        public List<Character> characters = new List<Character>();

        public event PropertyChangedEventHandler PropertyChanged;

        public VN(int id, Name name, string date)
        {
            this.id = id;
            this.name = name;
            this.date = date;

            Color = "red";
        }

        public void Print()
        {
            Console.WriteLine("[" + id + "] " + name.japName + "(" + name.engName + ") - " + date);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Character
    {
        public readonly int id;
        public readonly Name name;
        public Voice voice;

        public string Text
        {
            get
            {
                string res = name.japName + "(" + name.engName + ")";
                if (voice != null)
                    return res + " -" + voice.Nickname;
                return res;
            }
        }
        public Character(int id, Name name)
        {
            this.id = id;
            this.name = name;
        }

        public void Print()
        {
            name.Print();
        }
    }

    public class Name
    {
        public readonly string engName; 
        public readonly string japName;
        public string Text { get { return japName + "(" + engName + ")"; } }
        
        public Name(string engName, string japName)
        {
            this.engName = engName;
            this.japName = japName;
        }

        public void Print()
        {
            Console.WriteLine(engName + " / " + japName);
        }
        
    }
}
