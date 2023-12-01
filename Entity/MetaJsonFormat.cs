using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WillysReceiptParser.Entity
{

        public class AmountPriceData
        {
            [JsonPropertyName("currencyIso")]
            public string currencyIso { get; set; }

            [JsonPropertyName("value")]
            public double? value { get; set; }

            [JsonPropertyName("priceType")]
            public string priceType { get; set; }

            [JsonPropertyName("formattedValue")]
            public string formattedValue { get; set; }

            [JsonPropertyName("minQuantity")]
            public object minQuantity { get; set; }

            [JsonPropertyName("maxQuantity")]
            public object maxQuantity { get; set; }
        }

        public class BonusAppliableAmountPriceData
        {
            [JsonPropertyName("currencyIso")]
            public string currencyIso { get; set; }

            [JsonPropertyName("value")]
            public double? value { get; set; }

            [JsonPropertyName("priceType")]
            public string priceType { get; set; }

            [JsonPropertyName("formattedValue")]
            public string formattedValue { get; set; }

            [JsonPropertyName("minQuantity")]
            public object minQuantity { get; set; }

            [JsonPropertyName("maxQuantity")]
            public object maxQuantity { get; set; }
        }

        public class GeneralDiscountPriceData
        {
            [JsonPropertyName("currencyIso")]
            public string currencyIso { get; set; }

            [JsonPropertyName("value")]
            public double? value { get; set; }

            [JsonPropertyName("priceType")]
            public string priceType { get; set; }

            [JsonPropertyName("formattedValue")]
            public string formattedValue { get; set; }

            [JsonPropertyName("minQuantity")]
            public object minQuantity { get; set; }

            [JsonPropertyName("maxQuantity")]
            public object maxQuantity { get; set; }
        }

        public class MemberDiscountPriceData
        {
            [JsonPropertyName("currencyIso")]
            public string currencyIso { get; set; }

            [JsonPropertyName("value")]
            public decimal? value { get; set; }

            [JsonPropertyName("priceType")]
            public string priceType { get; set; }

            [JsonPropertyName("formattedValue")]
            public string formattedValue { get; set; }

            [JsonPropertyName("minQuantity")]
            public object minQuantity { get; set; }

            [JsonPropertyName("maxQuantity")]
            public object maxQuantity { get; set; }
        }

        public class MetaJsonFormat
        {
            [JsonPropertyName("bonusAppliableAmount")]
            public double? bonusAppliableAmount { get; set; }

            [JsonPropertyName("bonusAppliableAmountPriceData")]
            public BonusAppliableAmountPriceData bonusAppliableAmountPriceData { get; set; }

            [JsonPropertyName("contactFirstName")]
            public string contactFirstName { get; set; }

            [JsonPropertyName("contactLastName")]
            public string contactLastName { get; set; }

            [JsonPropertyName("memberCardNumber")]
            public string memberCardNumber { get; set; }

            [JsonPropertyName("memberDiscount")]
            public decimal? memberDiscount { get; set; }

            [JsonPropertyName("memberDiscountPriceData")]
            public MemberDiscountPriceData memberDiscountPriceData { get; set; }

            [JsonPropertyName("storeCustomerId")]
            public string storeCustomerId { get; set; }

            [JsonPropertyName("storeName")]
            public string storeName { get; set; }

            [JsonPropertyName("generalDiscount")]
            public double? generalDiscount { get; set; }

            [JsonPropertyName("generalDiscountPriceData")]
            public GeneralDiscountPriceData generalDiscountPriceData { get; set; }

            [JsonPropertyName("bookingDate")]
            public long? bookingDate { get; set; }

            [JsonPropertyName("amount")]
            public decimal? amount { get; set; }

            [JsonPropertyName("amountPriceData")]
            public AmountPriceData amountPriceData { get; set; }

            [JsonPropertyName("partner")]
            public string partner { get; set; }

            [JsonPropertyName("points")]
            public string points { get; set; }

            [JsonPropertyName("product")]
            public string product { get; set; }

            [JsonPropertyName("status")]
            public string status { get; set; }

            [JsonPropertyName("transactionSubType")]
            public string transactionSubType { get; set; }

            [JsonPropertyName("transactionType")]
            public string transactionType { get; set; }

            [JsonPropertyName("productUnitOfMeasure")]
            public string productUnitOfMeasure { get; set; }

            [JsonPropertyName("deliveryMethod")]
            public string deliveryMethod { get; set; }

            [JsonPropertyName("orderNumber")]
            public string orderNumber { get; set; }

            [JsonPropertyName("digitalReceiptAvailable")]
            public bool? digitalReceiptAvailable { get; set; }

            [JsonPropertyName("digitalReceiptReference")]
            public string digitalReceiptReference { get; set; }

            [JsonPropertyName("paymentType")]
            public object paymentType { get; set; }

            [JsonPropertyName("receiptSource")]
            public string receiptSource { get; set; }

            [JsonPropertyName("id")]
            public object id { get; set; }
        }
}
