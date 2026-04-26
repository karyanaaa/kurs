using Newtonsoft.Json;

namespace FinUchetClient.Models
{
    public class CategoryModel
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = "expense"; // income или expense

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        public string TypeDisplay => Type == "income" ? "Доход" : "Расход";
    }
}