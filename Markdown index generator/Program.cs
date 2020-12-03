using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Markdown_index_generator {
    internal class Program {
        private static void Main(String[] args) {
            Console.WriteLine("Directory?");
            String Dir = Console.ReadLine();

            if (Directory.Exists(Dir)) {
                GenerateIndex(Dir);
            }

            Console.WriteLine("\n\nDone!");
            Console.ReadLine();
        }

        private static void GenerateIndex(String Folder) {
            GenerateDIIndex(new DirectoryInfo(Folder));
        }

        [return: MaybeNull]
        private static String GenerateDIIndex(DirectoryInfo DI) {
            var Indexes = new List<String>();
            var Docs = new List<String>();

            //Collect all sub indexes
            foreach (DirectoryInfo Dir in DI.GetDirectories()) {
                if (Dir.Name == ".git") {
                    continue;
                }

                String Temp = GenerateDIIndex(Dir);

                if (Temp != null) {
                    Indexes.Add(Temp);
                }
            }

            //Collect all docs
            foreach (FileInfo F in DI.GetFiles("*.md", SearchOption.TopDirectoryOnly)) {
                if (F.Name == "index.md") {
                    continue;
                }

                Docs.Add(F.Name);
            }

            //If no sub indexes or documenents are found then ignore this folder
            if (Indexes.Count <= 0 && Docs.Count <= 0) {
                return null;
            }

            //If there is only 1 document and no sub indexes, return the document as if its the index
            if (Indexes.Count == 0 && Docs.Count == 1) {
                var Fi = new FileInfo(Docs[0]);

                return (DI.Name + "/" + Fi.Name).Replace(" ", "%20");
            }

            //Setup writing
            String Name = DI.Name;
            String Filepath = DI.FullName + $"/index.md";
            Console.WriteLine(Filepath);

            var Writer = new StreamWriter(Filepath, false);
            Writer.WriteLine($"# {Name}");

            //Write subindexes as category
            if (Indexes.Count > 0) {
                Writer.WriteLine("## Categories");

                foreach (String Ind in Indexes) {
                    //Remove index from name
                    String IndName = Ind.Replace("/index.md", String.Empty);
                    IndName = IndName.Replace("%20", " ");

                    //If there is still a folder / file structure then its probaly an actual file and not an index. 
                    //Thus retrieve the name of the file as the name of the link
                    Int32 Index = IndName.IndexOf('/');

                    if (Index > -1) {
                        IndName = IndName[(Index + 1)..].Replace(".md", String.Empty);
                    }

                    Writer.WriteLine($"- [{IndName}]({Ind})");
                }

                Writer.WriteLine("---");
            }

            //Write sub documents
            if (Docs.Count > 0) {
                Writer.WriteLine("## Documents");

                foreach (String Ind in Docs) {
                    String IndName = Ind.Replace(".md", String.Empty);
                    String Link = Ind.Replace(" ", "%20");

                    Writer.WriteLine($"- [{IndName}]({Link})");
                }

                Writer.WriteLine("---");
            }

            Writer.Flush();
            Writer.Close();

            return $"{Name.Replace(" ", "%20")}/index.md";
        }
    }
}
