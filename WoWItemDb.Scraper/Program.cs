﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWItemDb.Scraper
{
    class Program
    {
        static void Main(string[] args)
        {
            Scraper s = new Scraper("c:\\temp\\wowitemdump.json", "xq8vpc767927jktq28fgvcchwtr96f7m", "en_US", 1, 133948);
            s.OnResponseRecieved += S_OnResponseRecieved;
            s.BeginScraping().Wait();

            Console.ReadLine();
        }

        private static void S_OnResponseRecieved(object source, ReponseEventArgs e)
        {
            Console.WriteLine($"{e.ItemId} \t {e.ResponseCode}");
        }
    }
}
