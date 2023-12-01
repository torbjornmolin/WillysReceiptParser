using System.Globalization;
using System.Text.RegularExpressions;

namespace WillysReceiptParser
{
    internal static class Program
    {
        public static async Task Main(string[] args)
        {
            const string receiptPath = @"C:\src\WillysReceiptParser\kvitton\";

            var result = new List<Receipt>();
            foreach (var file in Directory.GetFiles(receiptPath, "*.pdf"))
            {
                var parser = new ReceiptParser(file, file.Replace(".pdf", ".json"));
                result.Add(parser.Parse());
            }
        }



        private static List<LineItem> ReceiptLinesToLineItems(string[] receiptLines)
        {
            var priceCultureInfo = new CultureInfo("sv-SE");
            var result = new List<LineItem>();
            int i = 0;
            //while (!receiptLines[i].ToLower().Contains("start"))
            //while (!receiptLines[i].ToLower().Contains("start"))
            while (receiptLines[i] != "------------------------------------------")
                i++;
            i++;
            if (receiptLines[i].ToLower().Contains("start"))
                i++;
            for (; i < receiptLines.Length; i++)
            {
                string l = receiptLines[i];
                if (l.ToLower().Contains("slut"))
                {
                    break;
                }
                if (l == "------------------------------------------")
                    break;
                if (l.ToLower().Contains("rabatt"))
                {
                    // TODO
                    // result.Last().AddDiscount() kanske?
                    continue;
                }
                if (l.StartsWith(" ")) // lines that start with whitespace tend to specify the price of a past line.
                    continue;

                var parts = l.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                string priceString = parts.Last();
                if (decimal.TryParse(priceString, NumberStyles.Any, priceCultureInfo, out var totalPrice) && totalPrice > 0) // Only pick 'normal' lines for now
                {
                    // MILD YOGHURT NAT 1KG  3st 13,60      40,80
                    var item = new LineItem();
                    item.Name = l.Substring(0, "MILD YOGHURT NAT 1KG ".Length).Trim();
                    item.TotalPrice = totalPrice;
                    item.Quantity = 1;
                    item.UnitPrice = totalPrice;

                    foreach (var part in parts)
                    {
                        if (Regex.IsMatch(part, "\\d+st.+"))
                        {
                            var subParts = part.Split("st", StringSplitOptions.TrimEntries);
                            var quantity = int.Parse(subParts.First());
                            var unitPrice = decimal.Parse(priceString, NumberStyles.Any, priceCultureInfo);

                            item.Quantity = quantity;
                            item.UnitPrice = unitPrice;
                        }
                    }

                    result.Add(item);
                }
            }

            return result;
        }
    }
}