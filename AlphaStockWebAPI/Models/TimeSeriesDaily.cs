using System.Numerics;

namespace AlphaStockWebAPI.Models
{
    public partial class TimeSeriesDaily
    {
        public DateTime Date { get; set; } // comes from Alpha Vantage API, this
        public string Symbol { get; set; } = ""; // comes from UI
        public decimal Open { get; set; } // comes from Alpha Vantage API
        public decimal High { get; set; } // comes from Alpha Vantage API
        public decimal Low { get; set; } // comes from Alpha Vantage API
        public decimal Close { get; set; } // comes from Alpha Vantage API
        public long Volume { get; set; } // comes from Alpha Vantage API
        public decimal DayOnDayChangePercent { get; set; } = 0;
        public decimal OneMonthTrailing { get; set; } = 0;
        public decimal ChangeFromOpenPercent { get; set; } = 0;
        public decimal averageDbVolume { get; set; } = 0;
    }
    
}