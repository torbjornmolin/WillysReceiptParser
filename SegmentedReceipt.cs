using System.Text.RegularExpressions;

namespace WillysReceiptParser
{
    internal class SegmentedReceipt
    {
        private readonly string _segmentDivider = "------------------------------------------";
        public string[] Header { get; set; }
        public string[] LineItems { get; private set; }
        public string[] PaymentInfo { get; private set; }

        public SegmentedReceipt(List<string> receiptLines)
        {
            int i = 0;
            i = GetHeader(receiptLines, i);
            i = GetLines(receiptLines, i);
            GetPaymentInfo(receiptLines, i);
        }

        private int GetHeader(List<string> receiptLines, int i)
        {
            var head = new List<string>();

            while (!receiptLines[i].Contains(_segmentDivider))
            {
                head.Add(receiptLines[i].Trim());
                i++;
            }
            Header = head.ToArray();
            return i;
        }

        private int GetLines(List<string> receiptLines, int i)
        {
            i++; // skip segment divider
            var lines = new List<string>();

            while (!receiptLines[i].Contains(_segmentDivider))
            {
                if (receiptLines[i].StartsWith("==="))
                {
                    i++;
                    continue;
                }
                if (Regex.IsMatch(receiptLines[i].ToLower(), ".+sj.lvs.anning.+"))
                {
                    i++;
                    continue;
                }

                lines.Add(receiptLines[i]);
                i++;
            }
            LineItems = lines.ToArray();
            return i;
        }

        private int GetPaymentInfo(List<string> receiptLines, int i)
        {
            i++; // skip segment divider
            var paymentInfo = new List<string>();

            while (!receiptLines[i].Contains(_segmentDivider))
            {
                paymentInfo.Add(receiptLines[i].Trim());
                i++;
            }
            PaymentInfo = paymentInfo.ToArray();
            return i;
        }
    }
}
