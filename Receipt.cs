namespace WillysReceiptParser
{
    public class Receipt
    {
        public int Id { get; set; }
        public string Store { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public int StoreId { get; set; }
        public string FileOrigin { get; set; }
        public LineItem[] LineItems { get; set; } = Array.Empty<LineItem>();
    }
}