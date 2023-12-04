using WillysReceiptParser.Helpers;
using WillysReceiptParser.Repositories;

namespace WillysReceiptParser
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            var config = GetConfig();

            var dbSettings = DbSettings.ReadConfig();
            var dataContext = new DataContext(dbSettings);
            await dataContext.Init();

            var result = new List<Receipt>();
            foreach (var file in Directory.GetFiles(config.ReceiptPath, "*.pdf"))
            {
                var parser = new ReceiptParser(file, file.Replace(".pdf", ".json"), config.WordListPath);
                result.Add(parser.Parse());
            }

            var receiptRepository = new ReceiptRepository(dataContext);
            var lineItemRepository = new LineItemRepository(dataContext);

            //foreach (var r in result)
            //{
            //    foreach (var line in r.LineItems)
            //    {
            //        Console.WriteLine(line.Name);
            //    }
            //}

            ValidateReceiptSums(result);

            await InsertIntoDb(result, receiptRepository, lineItemRepository);

        }

        private static async Task InsertIntoDb(List<Receipt> result, ReceiptRepository receiptRepository, LineItemRepository lineItemRepository)
        {
            foreach (var receipt in result)
            {
                var id = await receiptRepository.Create(receipt);
                var lineItemsWithId = receipt.LineItems.Select(lineItem =>
                {
                    lineItem.ReceiptId = id;
                    return lineItem;
                }
                    );

                foreach (var l in lineItemsWithId)
                {
                    await lineItemRepository.Create(l);
                }

            }
        }

        private static void ValidateReceiptSums(List<Receipt> result)
        {
            foreach (var receipt in result)
            {
                var receiptSum = receipt.LineItems.Sum(l => l.TotalPrice + l.Discount);
                if (receiptSum != receipt.TotalAmount)
                {
                    Console.WriteLine($"Incorrect amount found: {receiptSum}, expected {receipt.TotalAmount} for receipt: {receipt.FileOrigin}");
                }
            }
        }

        private static Config GetConfig()
        {
            var receiptPath = @"kvitton\";
            var wordListPath = "wordlist.txt";

            if (File.Exists("local.conf"))
            {
                foreach (var line in File.ReadAllLines("local.conf"))
                {
                    var parts = line.Split('=');
                    if (parts.Any())
                    {
                        var key = parts.First().Trim().ToLower();
                        var value = parts.Last().Trim();

                        switch (key)
                        {
                            case "receiptpath":
                                receiptPath = value;
                                break;
                            case "wordlistpath":
                                wordListPath = value;
                                break;
                        }
                    }
                }
            }
            return new Config(receiptPath, wordListPath);
        }
    }

    public class Config
    {
        public Config(string receiptPath, string wordListPath)
        {
            ReceiptPath = receiptPath;
            WordListPath = wordListPath;
        }

        public string ReceiptPath { get; }
        public string WordListPath { get; }
    }
}