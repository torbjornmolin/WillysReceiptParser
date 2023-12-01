namespace WillysReceiptParser
{
    public class Receipt
    {
        public string Store { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public LineItem[] LineItems { get; set; } = Array.Empty<LineItem>();
        public decimal TotalAmount { get; set; }
        public int StoreId { get; internal set; }
        public string FileOrigin { get; internal set; }
    }
}