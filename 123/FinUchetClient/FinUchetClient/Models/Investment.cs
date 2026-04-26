using System;

namespace FinUchetClient.Models
{
    public class InvestmentModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "stock";
        public double Amount { get; set; }
        public double PurchasePrice { get; set; }
        public double CurrentPrice { get; set; }
        public double Quantity { get; set; }
        public DateTime PurchaseDate { get; set; } = DateTime.Now;
        public string Currency { get; set; } = "RUB";
        public string Notes { get; set; } = string.Empty;
        public int UserId { get; set; }

        public double CurrentValue => Quantity * CurrentPrice;
        public double TotalInvested => Quantity * PurchasePrice;
        public double ProfitLoss => CurrentValue - TotalInvested;
        public double ProfitLossPercent => TotalInvested > 0 ? (ProfitLoss / TotalInvested) * 100 : 0;

        public string TypeDisplay
        {
            get
            {
                return Type switch
                {
                    "stock" => "📈 Акции",
                    "bond" => "📉 Облигации",
                    "crypto" => "₿ Криптовалюта",
                    "realty" => "🏠 Недвижимость",
                    "deposit" => "🏦 Депозит",
                    _ => "💰 Другое"
                };
            }
        }
    }
}