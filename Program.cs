using System.Globalization;
using System.Text.RegularExpressions;

namespace WillysReceiptParser
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            const string receiptPath = @"kvitton";

            var result = new List<Receipt>();
            foreach (var file in Directory.GetFiles(receiptPath, "*.pdf"))
            {
                var parser = new ReceiptParser(file, file.Replace(".pdf", ".json"));
                result.Add(parser.Parse());
            }

            foreach (var receipt in result)
            {
                var receiptSum = receipt.LineItems.Sum(l => l.TotalPrice + l.Discount);
                if (receiptSum != receipt.TotalAmount)
                {
                    Console.WriteLine($"Incorrect amount found: {receiptSum}, expected {receipt.TotalAmount} for receipt: {receipt.FileOrigin}");
                }
            }
        }
    }
}