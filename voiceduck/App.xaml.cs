using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
        public static int ExtractId(string str)
        {
            int id = 0;
            for(int i=0; i < str.Length; i++)
            {
                if ('0' <= str[i] && str[i] <= '9')
                    id = id * 10 + (int)(str[i] - '0');
            }
            return id;
        }
        public Voice GetVoice(int id)
        {
            Voice voice = new Voice(id);
            string url = "http://vndb.org/s" + id;

            try
            {
                var web = new HtmlAgilityPack.HtmlWeb();
                HtmlDocument doc = web.Load(url);

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

                    int _id = ExtractId(titleTag.GetAttributeValue("href", ""));
                    japName = titleTag.GetAttributeValue("title", "");
                    engName = titleTag.InnerText;
                    name = new Name(engName, japName);
                    voice.PushVN(new VN(_id, name, date));

                    _id = ExtractId(characterTag.GetAttributeValue("href", ""));
                    japName = characterTag.GetAttributeValue("title", "");
                    engName = characterTag.InnerText;
                    voice.PushCharacter(new Character(_id, new Name(engName, japName)));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            voice.PrintNames();
            voice.PrintVNs();
            voice.PrintCharacters();
            return voice;
        }
    }


    public class Voice
    {
        private readonly int id;
        public readonly List<Name> names = new List<Name>();
        public readonly List<VN> vns = new List<VN>();
        public readonly List<Character> characters = new List<Character>();

        public Voice(int id)
        {
            this.id = id;
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
    }

    public class VN
    {
        private readonly int id;
        public readonly Name name;
        public readonly string date;
        

        private bool seen = false;
        private List<Character> characters;

        public VN(int id, Name name, string date)
        {
            this.id = id;
            this.name = name;
            this.date = date;
        }

        public void Print()
        {
            Console.WriteLine("[" + id + "] " + name.japName + "(" + name.engName + ") - " + date);
        }
    }

    public class Character
    {
        private readonly int id;
        public readonly Name name;
        
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
