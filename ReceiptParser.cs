using Npgsql.Replication;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using WillysReceiptParser.Entity;

namespace WillysReceiptParser
{
    public partial class ReceiptParser
    {
        private const string wordListPath = @"wordlist.txt";
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
            result.FileOrigin = _pdfFile;
            var segmentedReceipt = splitter.GetSegmentedReceipt(_pdfFile);
            Console.WriteLine($"Parsing {_pdfFile}");

            result.LineItems = ReadLineItems(segmentedReceipt).ToArray();
            FixMissingCharacters(result.LineItems);

            ReadMetaData(result);

            return result;
        }

        private void ReadMetaData(Receipt receipt)
        {
            var json = File.ReadAllText(_metaDataFile);

            var metaData = JsonSerializer.Deserialize<MetaJsonFormat>(json);

            if (metaData is null)
            {
                throw new InvalidOperationException("Could not read metadata from json.");
            }
            var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(metaData.bookingDate / 1000 ?? throw new InvalidOperationException("Bookingdate not set."));

            receipt.Date = dateTimeOffset.DateTime;
            receipt.Store = metaData?.storeName ?? throw new InvalidOperationException("Store name not set.");
            receipt.StoreId = int.Parse(metaData.storeCustomerId);
            receipt.TotalAmount = metaData.amount ?? throw new InvalidOperationException("Receipt metadata amount not set.");
        }

        private List<LineItem> ReadLineItems(SegmentedReceipt segmentedReceipt)
        {
            var lineItems = new List<LineItem>();
            for (int i = 0; i < segmentedReceipt.LineItems.Length; i++)
            {
                var item = new LineItem();
                var currentLine = segmentedReceipt.LineItems[i];

                ParseInitialLine(currentLine, item);

                // Get all additional lines belonging to an item
                i++;
                while (i < segmentedReceipt.LineItems.Length && ParseAdditionalLine(segmentedReceipt.LineItems[i], item))
                    i++;
                i--; // next for iteration will increment i

                if (item.TotalPrice == default)
                {
                    throw new InvalidOperationException("Price not set for item.");
                }

                lineItems.Add(item);
            }

            return lineItems;
        }

        private void ParseInitialLine(string line, LineItem item)
        {
            var singleLineWithQuantityMatch = singleLineWithQuantityItemRegex().Match(line);
            var singleLineMatch = singleLineItemRegex().Match(line);
            var singleLineNameOnlyMatch = singleLineItemNameOnlyRegex().Match(line);

            // Order matters here

            if (singleLineWithQuantityMatch?.Success ?? false)
            {
                item.Name = singleLineWithQuantityMatch.Groups.Values.ElementAt(1).Value.Trim();
                item.Quantity = int.Parse(singleLineWithQuantityMatch.Groups.Values.ElementAt(2).Value);
                item.UnitPrice = decimal.Parse(singleLineWithQuantityMatch.Groups.Values.ElementAt(3).Value);
                item.TotalPrice = decimal.Parse(singleLineWithQuantityMatch.Groups.Values.ElementAt(4).Value);

                return;
            }

            else if (singleLineMatch?.Success ?? false)
            {
                item.Name = singleLineMatch.Groups.Values.ElementAt(1).Value.Trim();
                item.Quantity = 1;
                item.UnitPrice = decimal.Parse(singleLineMatch.Groups.Values.ElementAt(2).Value);
                item.TotalPrice = decimal.Parse(singleLineMatch.Groups.Values.ElementAt(2).Value);

                return;
            }
            else if (singleLineNameOnlyMatch?.Success ?? false)
            {
                item.Name = singleLineNameOnlyMatch.Groups.Values.ElementAt(1).Value.Trim();

                return;
            }

            throw new InvalidOperationException($"{line} does not match any Initial line pattern.");
        }
        private bool ParseAdditionalLine(string line, LineItem item)
        {
            var discountLineMatch = discountLineItemRegex().Match(line);
            var priceByWeightMatch = priceByWeightLine().Match(line);
            var unitPriceOnlyPriceByWeightLineMatch = unitPriceOnlyPriceByWeightLine().Match(line);

            if (discountLineMatch?.Success ?? false)
            {
                item.Discount = decimal.Parse(discountLineMatch.Groups.Values.ElementAt(1).Value);

                return true;
            }

            else if (priceByWeightMatch?.Success ?? false)
            {
                item.Quantity = decimal.Parse(priceByWeightMatch.Groups.Values.ElementAt(1).Value);
                item.UnitPrice = decimal.Parse(priceByWeightMatch.Groups.Values.ElementAt(2).Value);
                item.TotalPrice = decimal.Parse(priceByWeightMatch.Groups.Values.ElementAt(3).Value);

                return true;
            }

            else if (unitPriceOnlyPriceByWeightLineMatch?.Success ?? false)
            {
                item.Quantity = decimal.Parse(unitPriceOnlyPriceByWeightLineMatch.Groups.Values.ElementAt(1).Value);
                item.UnitPrice = decimal.Parse(unitPriceOnlyPriceByWeightLineMatch.Groups.Values.ElementAt(2).Value);

                return true;
            }

            else if (extraprisWithNoInfo().IsMatch(line))
            {
                return true;
            }
            else if (rabattWithNoInfo().IsMatch(line))
            {
                return true;
            }
            else if (clearanceWithNoInfo().IsMatch(line))
            {
                return true;
            }
            else if (discontinuedWithNoInfo().IsMatch(line))
            {
                return true;
            }
            else if (willysPlusWithNoInfo().IsMatch(line))
            {
                return true;
            }
            return false;
        }


        private static void FixMissingCharacters(LineItem[] items)
        {
            var wordDictionary = items.Where(i =>
            {
                return
                i.Name.ToLower().Contains('å')
                || i.Name.ToLower().Contains('ä')
                || i.Name.ToLower().Contains('ö');
            }
            ).Distinct().Select(i => i.Name).ToHashSet();

            var savedWords = File.ReadAllLines(wordListPath);

            foreach (var w in savedWords)
            {
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

        [GeneratedRegex(@"(.{19})\s+(\d+,\d\d)")]
        private static partial Regex singleLineItemRegex();

        [GeneratedRegex(@"(.+)\s?")]
        private static partial Regex singleLineItemNameOnlyRegex();

        [GeneratedRegex(@"(.{19})\s+(\d+)st.(\d+,\d\d)\s+(\d+,\d\d)")]
        private static partial Regex singleLineWithQuantityItemRegex();

        [GeneratedRegex(@".?(-\d+,\d\d)")]
        private static partial Regex discountLineItemRegex();

        [GeneratedRegex(@".xtrapris\s?")]
        private static partial Regex extraprisWithNoInfo();

        [GeneratedRegex(@".*[Rr]abatt.*")]
        private static partial Regex rabattWithNoInfo();

        [GeneratedRegex(@"[Uu]tf.rs.l.*")] // Utförsäljning
        private static partial Regex clearanceWithNoInfo();


        [GeneratedRegex(@"[Uu]tg.ende.*")] // utg?ende
        private static partial Regex discontinuedWithNoInfo();

        [GeneratedRegex(@"[Ww]illys.[Pp]lus.*")] // willys Plus:
        private static partial Regex willysPlusWithNoInfo();

        [GeneratedRegex(@"\s(\d+,\d+)kg.?(\d+,\d+)kr.kg\s+(\d+,\d+)")]
        private static partial Regex priceByWeightLine();

        // 0,724kg 99,00kr/kg
        [GeneratedRegex(@"\s*(\d+,\d+)kg.(\d+,\d+)kr.kg\s*")]
        private static partial Regex unitPriceOnlyPriceByWeightLine();
    }
}
