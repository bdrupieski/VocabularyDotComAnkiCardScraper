using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;

namespace VocabularyDotComAnkiCardScraper
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var arguments = CommandLineArguments.BuildFromCommandLineArguments(args, Console.WriteLine);
            if (arguments == null)
            {
                return;
            }

            var wordsAndDefinitions = GetWords(arguments.VocabularyDotComListPageUrl).GetAwaiter().GetResult();

            var lines = wordsAndDefinitions.Select(x => $"{x.word} ~ {x.definition}");
            File.WriteAllLines(arguments.AnkiOutputFilePath, lines);
        }

        private static async Task<IEnumerable<(string word, string definition)>> GetWords(string url)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var document = await BrowsingContext.New(config).OpenAsync(url);
            var wordListElement = document.GetElementById("wordlist");

            var wordsAndDefinitions = new List<(string, string)>();
            foreach (var wordElement in wordListElement.Children)
            {
                var word = wordElement.Attributes["word"].Value;
                var definition = wordElement.Children.Single(x => x.ClassList.Contains("definition")).TextContent;
                wordsAndDefinitions.Add((word, definition));
            }

            return wordsAndDefinitions;
        }

        private class CommandLineArguments
        {
            public string VocabularyDotComListPageUrl { get; private set; }
            public string AnkiOutputFilePath { get; private set; }

            public static CommandLineArguments BuildFromCommandLineArguments(string[] args, Action<string> onError)
            {
                if (args.Length != 2)
                {
                    onError("There must be two arguments. The first is a vocabulary.com list page URL and the second is the path of the output file.");
                    return null;
                }

                string url = args[0];
                string outputPath = args[1];

                var arguments = new CommandLineArguments();

                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    onError("The vocabulary.com URL is not well-formed.");
                    return null;
                }

                arguments.VocabularyDotComListPageUrl = url;
                arguments.AnkiOutputFilePath = outputPath;

                return arguments;
            }
        }
    }
}
