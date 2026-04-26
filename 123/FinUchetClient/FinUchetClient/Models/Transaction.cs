using System;
using Newtonsoft.Json;

namespace FinUchetClient.Models
{
    public class TransactionModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = "expense";

        [JsonProperty("date")]
        public DateTime Date { get; set; } = DateTime.Now;

        [JsonProperty("category_id")]
        public int CategoryId { get; set; }

        [JsonProperty("category_name")]
        public string CategoryName { get; set; } = "Разное";

        // Для совместимости - добавим свойство Category
        public string Category => CategoryName;

        public string TypeDisplay => Type == "income" ? "Доход" : "Расход";
        public string AmountDisplay => Type == "income" ? $"+{Amount:F2}" : $"-{Amount:F2}";
    }
}