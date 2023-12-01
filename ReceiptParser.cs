using System.Globalization;
using System.Text.RegularExpressions;

namespace WillysReceiptParser
{
    public class ReceiptParser
    {
        private readonly string _pdfFile;
        private readonly string _metaDataFile;
        private readonly ReceiptSplitter splitter;
        private IFormatProvider? priceCultureInfo;

        public ReceiptParser(string pdfFile, string metaDataFile)
        {
            if (string.IsNullOrWhiteSpace(metaDataFile))
            {
                throw new ArgumentException($"'{nameof(metaDataFile)}' cannot be null or whitespace.", nameof(metaDataFile));
            }

            splitter = new ReceiptSplitter();
            if (string.IsNullOrWhiteSpace(pdfFile))
            {
                throw new ArgumentException($"'{nameof(pdfFile)}' cannot be null or whitespace.", nameof(pdfFile));
            }
            _pdfFile = pdfFile;
            _metaDataFile = metaDataFile;

            if (!File.Exists(_pdfFile))
            {
                throw new FileNotFoundException(_pdfFile);
            }
            if (!File.Exists(_metaDataFile))
            {
                throw new FileNotFoundException(_metaDataFile);
            }
        }

        public Receipt Parse()
        {
            var result = new Receipt();
            var segmentedReceipt = splitter.GetSegmentedReceipt(_pdfFile);

            var lineItems = new List<LineItem>();
            for (int i = 0; i < segmentedReceipt.LineItems.Length; i++)
            {
                var item = new LineItem();

                string line = segmentedReceipt.LineItems[i];
                var parts = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                // Hitta index för känt innehåll när vi är på rad 1.
                // Namnet är allt fram till innehåll vid känt index (ex.vis. fram till 2st*4,90 eller fram till pris)
                // Kolla om sista posten på raden är pris, om inte måste vi leta vidare på senare rader.
                // De senare raderna måste kategoriseras på om de är prisrad (som för ost)
                // eller om de är rabatt eller något annat.
                int quantityPriceIndex = GetQuantityPriceIndex(parts);

                lineItems.Add(item);
            }

            FixMissingCharacters(lineItems);
            return result;
        }

        private int GetQuantityPriceIndex(string[] parts)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                string? part = parts[i];
                var trimmedPart = part.Trim();
                if (Regex.IsMatch(part, "\d+st\*\d+,\d\d"))
                {
                    return i;
                }
            }
            return -1;
        }

        private bool IsItemAdditionalLine(string line)
        {
            line = line.ToLower();
            if (line.Contains("rabatt"))
                return true;
            if (line.Contains("pant"))
                return true;
            if (line.StartsWith(" "))
            {
                return true;
            }
            return false;
        }

        private static void FixMissingCharacters(List<LineItem> items)
        {
            var wordDictionary = items.Where(i =>
            {
                return
                i.Name.ToLower().Contains('å')
                || i.Name.ToLower().Contains('ä')
                || i.Name.ToLower().Contains('ö');
            }
            ).Distinct().Select(i => i.Name).ToHashSet();

            var savedWords = File.ReadAllLines("wordlist.txt");

            foreach (var w in savedWords)
            {
                if (!wordDictionary.Contains(w))
                    wordDictionary.Add(w);
            }

            foreach (var i in items.Where(i => i.Name.Contains('?')))
            {
                var pattern = i.Name.Replace('?', '.');
                var newWord = wordDictionary.FirstOrDefault(w => Regex.IsMatch(w, pattern));
                if (newWord is null)
                {
                    newWord = GetWordInteractive(wordDictionary, i);
                }
                Console.WriteLine($"Replacing {i.Name} with {newWord}");
                i.Name = newWord;
            }
            var words = wordDictionary.OrderBy(w => w).ToList();
            File.WriteAllLines("wordlist.txt", words);
        }

        private static string GetWordInteractive(HashSet<string> wordDictionary, LineItem? lineItem)
        {
            string? newWord;
            string wordToAdd = string.Empty;
            bool keepLooping = true;
            while (keepLooping)
            {
                Console.WriteLine();
                Console.WriteLine($"No word found for {lineItem.Name}");
                Console.WriteLine("Please input replacement: ");
                wordToAdd = Console.ReadLine().ToUpper();
                Console.WriteLine();
                Console.WriteLine($"Are you happy with the word {wordToAdd} (y/n)?");
                keepLooping = Console.ReadKey().KeyChar != 'y';
                Console.WriteLine();
            }
            wordDictionary.Add(wordToAdd);
            newWord = wordToAdd;
            return newWord;
        }
    }
}
