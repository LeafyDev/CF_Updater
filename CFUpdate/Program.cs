// This program is a private software, based on c# source code.
// To sell or change credits of this software is forbidden,
// except if someone approve it from CFUpdate INC. team.
//  
// Copyrights (c) 2014 CFUpdate INC. All rights reserved.

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using CFUpdate.API;

namespace CFUpdate
{
    internal class Program
    {
        private const int STD_INPUT_HANDLE = -10;
        private const int ENABLE_QUICK_EDIT_MODE = 0x40 | 0x80;
        public static Random rnd = new Random();
        public static string ExtIP = string.Empty;
        public static int choice = 0;
        public static int errors = 0;
        public static CFConnectionInfo ConnectionInfo = new CFConnectionInfo();
        public static string NAME = string.Empty;
        public static string SUB = string.Empty;
        public static bool valid = false;
        public static readonly string path = Environment.CurrentDirectory + @"\CF-Updater.ini";

        private static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.Title = @"CF-Updater";

            if(File.Exists(path))
            {
                var settings = File.ReadAllLines(path);
                ConnectionInfo.Email = settings[0].Split('=')[1];
                ConnectionInfo.Token = settings[1].Split('=')[1];
                ConnectionInfo.ID = settings[2].Split('=')[1];
                NAME = settings[3].Split('=')[1];
                SUB = settings[4].Split('=')[1];

                if(string.IsNullOrEmpty(ConnectionInfo.Email) || string.IsNullOrEmpty(ConnectionInfo.ID) || string.IsNullOrEmpty(ConnectionInfo.Token)
                   || string.IsNullOrEmpty(NAME) || string.IsNullOrEmpty(SUB))
                {
                    Console.WriteLine(@"Invalid settings file." + Environment.NewLine);
                    InitSettings();
                }
                else
                {
                    Console.WriteLine(@"Loaded settings from file.");
                    Thread.Sleep(2000);
                }
            }
            else
            {
                InitSettings();
            }

            Console.Clear();

            valid = false;
            do
            {
                Console.WriteLine(@"Which IP do you want to use? Make a choice.");
                Console.WriteLine(@"-----------------------------");
                Console.WriteLine(@"1. Random IP");
                Console.WriteLine(@"2. Your IP");
                Console.WriteLine(@"3. Custom IP" + Environment.NewLine);
                Console.WriteLine(@"0. Exit");
                Console.WriteLine(@"-----------------------------");
                Console.WriteLine(@"Your choice:");

                var input = Console.ReadLine();
                Int32.TryParse(input, out choice);
                Console.WriteLine(@"-----------------------------");

                switch(choice)
                {
                    case 1:
                        SetRandomIP();
                        valid = true;
                        break;
                    case 2:
                        SetSelfIP();
                        valid = true;
                        break;
                    case 3:
                        SetCustomIP();
                        valid = true;
                        break;
                    case 0:
                        Environment.Exit(0);
                        break;
                    default:
                        errors = errors + 1;
                        Console.WriteLine(errors < 5 ? @"Wrong selection." : @"Seriously? ._.");
                        Thread.Sleep(1000);
                        Console.Clear();
                        valid = false;
                        break;
                }
            }
            while(valid == false);

            Console.WriteLine(@"-----------------------------");
            Console.WriteLine(@"Setting " + SUB + @"." + NAME + @" to " + ExtIP);

            var connection = CFConnection.CreateConnection(ConnectionInfo);
            connection.UpdateDomainRecord(NAME, SUB, ExtIP);

            if(choice == 2 || choice == 3)
            {
                Thread.Sleep(1000);
                Console.Clear();
                Console.WriteLine(@"-----------------------------");
                Console.WriteLine(@"Waiting for DNS to update." + Environment.NewLine);
                var i = 1;

                Console.WriteLine(@"-- Check #" + i);
                var host = NAME;
                var address = Dns.GetHostEntry(host).AddressList[0];
                Console.WriteLine(host + @" = " + address);
                Console.WriteLine(@"Rechecking in 30 seconds.");

                while(address.ToString() != ExtIP)
                {
                    i = i + 1;
                    Thread.Sleep(30000);
                    var p = new Process {StartInfo = {WindowStyle = ProcessWindowStyle.Hidden, FileName = "ipconfig", Arguments = "/flushdns"}};
                    p.Start();
                    Console.WriteLine(@"-- Check #" + i);
                    address = Dns.GetHostEntry(host).AddressList[0];
                    Console.WriteLine(host + @" = " + address);
                }
            }

            Console.WriteLine(@"-----------------------------");
            Console.WriteLine(@"Done! Press any key to GTFO.");
            Console.ReadKey();
        }

        private static void InitSettings()
        {
            Console.WriteLine(@"Enter CloudFlare email:");
            ConnectionInfo.Email = Console.ReadLine();
            Console.WriteLine(@"Enter CloudFlare token:");
            ConnectionInfo.Token = Console.ReadLine();
            Console.WriteLine(@"Enter CloudFlare subdomain ID:");
            ConnectionInfo.ID = Console.ReadLine();
            Console.WriteLine(@"Enter domain name:");
            NAME = Console.ReadLine();
            Console.WriteLine(@"Enter subdomain name:");
            SUB = Console.ReadLine();
            Console.WriteLine(@"Save these settings? (y/n)");
            var savechoice = Console.ReadLine();

            valid = false;
            do
            {
                switch(savechoice)
                {
                    case "Y":
                        SaveSettings();
                        valid = true;
                        break;
                    case "y":
                        SaveSettings();
                        valid = true;
                        break;
                    case "n":
                        valid = true;
                        break;
                    case "N":
                        valid = true;
                        break;
                    default:
                        Console.WriteLine(@"Invalid choice.");
                        valid = false;
                        break;
                }
            }
            while(valid == false);
            Thread.Sleep(2000);
        }

        private static void SaveSettings()
        {
            var sb = new StringBuilder();
            sb.AppendLine("EMAIL=" + ConnectionInfo.Email);
            sb.AppendLine("TKN=" + ConnectionInfo.Token);
            sb.AppendLine("ID=" + ConnectionInfo.ID);
            sb.AppendLine("NAME=" + NAME);
            sb.AppendLine("SUB=" + SUB);

            using(var outfile = new StreamWriter(Environment.CurrentDirectory + @"\CF-Updater.ini", true))
            {
                outfile.WriteAsync(sb.ToString());
            }
            Console.WriteLine(@"Settings saved.");
        }

        private static void SetCustomIP()
        {
            Console.WriteLine(@"Setting to custom IP.");
            valid = false;
            do
            {
                Console.WriteLine(@"Enter desired IP:");
                var response = Console.ReadLine();
                if(Regex.IsMatch(response, @"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"))
                {
                    ExtIP = response;
                    valid = true;
                }
                else
                {
                    Console.WriteLine(@"ERROR: not a valid IP.");
                    Console.Clear();
                }
            }
            while(valid != true);
        }

        private static void SetSelfIP()
        {
            Console.WriteLine(@"Setting to own IP.");
            Console.WriteLine(@"Obtaining info...");
            ExtIP = (new WebClient()).DownloadString("http://www.telize.com/ip");
            ExtIP = (new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(ExtIP)[0].ToString();
            Console.WriteLine(@"Found external IP: " + ExtIP);
        }

        private static void SetRandomIP()
        {
            Console.WriteLine(@"Setting to random IP.");
            Console.WriteLine(@"Obtaining info...");
            ExtIP = getRandomIp();
            Console.WriteLine(@"Generated external IP: " + ExtIP);
        }

        private static string getRandomIp()
        {
            var ExternalIP = getRandomInt() + "." + getRandomInt() + "." + getRandomInt() + "." + getRandomInt();
            return ExternalIP;
        }

        protected static int getRandomInt()
        {
            return rnd.Next(1, 254);
        }

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleMode(IntPtr hConsoleHandle, int mode);

        [DllImport("kernel32.dll")]
        private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(int handle);

        public static void EnableQuickEditMode()
        {
            int mode;
            var handle = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(handle, out mode);
            mode |= ENABLE_QUICK_EDIT_MODE;
            SetConsoleMode(handle, mode);
        }
    }
}