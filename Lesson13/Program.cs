using CMDMenu;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace Lesson13
{
    
    public class Program
    {
        private static DirectoryInfo CheckFolder(string path, int amountOfFiles = 1, int amountOfFolders = 0)
        {
            DirectoryInfo directory;
            if (File.Exists(path))
            {
                directory = new FileInfo(path).Directory ?? throw new MessageException($"Couldn't access parent directory of file {path}");

            }
            else if (Directory.Exists(path))
            {
                directory = new DirectoryInfo(path);
            }
            else throw new MessageException($"No such directory - {path}");

            var files = directory.GetFiles();
            var dirs = directory.GetDirectories();
            if (files.Length != amountOfFiles) throw new MessageException($"There must be {amountOfFiles} file(s) in directory (Received {files.Length})");
            if (dirs.Length != amountOfFolders) throw new MessageException($"There must {amountOfFolders} subdirectories (Received {dirs.Length})");
            return directory;
        }


        //Clears folder from created files so you can create again
        internal static void Clean(CMDHandler cmd)
        {
            string p = cmd.AskForInput("Enter folder to clean");
            var dir = CheckFolder(p, amountOfFiles: 3);
            foreach (var file in dir.GetFiles())
            {
                if (file.Name.EndsWith(".xml") || file.Name.Equals("abreviated.json"))
                {
                    file.Delete();
                }
            }
            Console.WriteLine("Successfully cleaned folder");
        }


        internal static bool MainFunction(string input) 
        {
            var directory = CheckFolder(input, amountOfFiles: 1);

            using (var instream = directory.GetFiles()[0].Open(FileMode.Open))
            {
                var opts = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
                Squad? s = JsonSerializer.Deserialize<Squad>(instream, opts);
                if (s == null) throw new MessageException("Couldn't deserialize file");
                Console.WriteLine("Successfully parsed file:");
                Console.WriteLine(new string('=', 8));
                Console.WriteLine(s);
                Console.WriteLine(new string('=', 8));
                using (var outstream = File.Create(directory.FullName + $@"\{s.SquadName}.xml"))
                {
                    var xmlser = new XmlSerializer(typeof(Squad));
                    xmlser.Serialize(outstream, s);
                    Console.WriteLine($"Successfully serialized squad into xml");
                }
                var outopts = new JsonSerializerOptions()
                {
                    WriteIndented = true,
                    Converters = { new JSONAbbreviateConverter() }
                };
                using (var outstream = File.CreateText(directory.FullName + @"\abreviated.json"))
                {
                    outstream.Write(JsonSerializer.Serialize(s, outopts));
                    Console.WriteLine($"Successfully serialized squad into abbreviated json");
                }
                using (var instreamAbr = File.Open(directory.FullName + @"\abreviated.json", FileMode.Open))
                {
                    Squad abrv = JsonSerializer.Deserialize<Squad>(instreamAbr, outopts);
                    Console.WriteLine($"Successfully deserialized abbreviated json:");
                    Console.WriteLine(new string('=', 8));
                    Console.WriteLine(abrv);
                    Console.WriteLine(new string('=', 8));
                }
            }
            return true;
        }

        public static void Main(string[] args)
        {
            CMDHandler cmd = new CMDHandler();
            cmd.Description = "Enter folder with json to parse, or commands \"clean\" for cleaning the folder from created files or \"quit\" for exit.";
            cmd.RegisterCommand("clean", () => Clean(cmd), "cleans folder from created files");
            cmd.DefaultAction = MainFunction;
            cmd.Run(showDescriptionAtBeginning: true);
        }
    }
}