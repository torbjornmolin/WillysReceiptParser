using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace WillysReceiptParser
{
    internal class ReceiptSplitter
    {
        public SegmentedReceipt GetSegmentedReceipt(string file)
        {
            using PdfDocument document = PdfDocument.Open(file);
            var finalText = new List<string>();
            foreach (var page in document.GetPages())
            {
                var allWords = page.GetWords();
                finalText.AddRange(GetTextFromPdfWords(allWords));
            }
            return new SegmentedReceipt(finalText);
        }

        private static string[] GetTextFromPdfWords(IEnumerable<Word> allWords)
        {
            var coordinateWordParts = new Dictionary<double, List<string>>();
            foreach (var linePart in allWords)
            {
                var bottomCoord = linePart.BoundingBox.Bottom;
                string linePartText = OffsetCharacters(linePart.Text);
                if (coordinateWordParts.ContainsKey(bottomCoord))
                {
                    coordinateWordParts[bottomCoord].Add(linePartText);
                }
                else
                {
                    coordinateWordParts[bottomCoord] = new List<string>() { linePartText };
                }
            }

            var finalTextLines = coordinateWordParts.OrderByDescending(dictElement => dictElement.Key).Select(dictElement => string.Join(" ", dictElement.Value));

            return finalTextLines.ToArray();
        }
        private static string OffsetCharacters(string input)
        {
            var res = new List<char>();
            int i = -1;
            foreach (var c in input)
            {
                i++;
                var val = Convert.ToInt16(c);
                val = GetCorrectChar(val);
                res.Add(Convert.ToChar(val));
            }
            var result = string.Join("", res);

            result = result.Replace('Õ', 'ö');
            result = result.Replace('Ã', 'ä');
            result = result.Replace('Ä', 'ä');

            result = result.Replace('¤', 'Å');
            result = result.Replace('£', 'Ä');
            result = result.Replace('µ', 'Ö');

            result = result.Replace('ఎ', '?');
            
            return result;
        }

        private static short GetCorrectChar(short val)
        {
            if (val == 10)
            { // *
                return 42;
            }
            else
            {
                val += 29;
                return val;
            }
        }
    }
}
