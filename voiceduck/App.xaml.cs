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
        public Voice GetVoice(int id)
        {
            Voice v = new Voice();
            string url = "http://vndb.org/s" + id;
            var web = new HtmlAgilityPack.HtmlWeb();
            HtmlDocument doc = web.Load(url);

            Console.WriteLine(doc.DocumentNode.SelectNodes("div")[0].ToString());

            return v;
        }
    }


    public class Voice
    {
        private readonly int id;
        private readonly List<Name> names;
        private List<VN> vnList;
    }

    public class VN
    {
        private readonly int id;
        private readonly string name;
        private bool seen = false;
        private readonly List<Character> characters;
    }

    public class Character
    {
        private readonly string name;

    }

    public class Name
    {
        private readonly string engName;
        private readonly string japName;
    }
}
