namespace ENOC.Dashboard.Models;

public class Weather
{
    public decimal Temp1 { get; set; }
    public int RelHumidity { get; set; }
    public decimal HeatIndex { get; set; }
    public decimal DewPoint { get; set; }
    public decimal WindChill { get; set; }
    public int RawWindDir { get; set; }
    public decimal WindSpeed { get; set; }
    public int RawGustDir { get; set; }
    public decimal WindGust { get; set; }
    public decimal BaromPress { get; set; }
    public decimal DailyRain { get; set; }
    public decimal MonthlyRain { get; set; }
    public decimal YearlyRain { get; set; }
    public decimal RainRate { get; set; }
    public decimal TodaysRain { get; set; }
    public decimal Last24HrRain { get; set; }
    public decimal Last1HrRain { get; set; }
    public decimal AvgWindSpeed2Min { get; set; }
    public decimal AvgWindSpeed10Min { get; set; }
    public decimal WindGust10Min { get; set; }
    public int WindDir10Min { get; set; }
    public decimal TempOut { get; set; }
    public decimal TempIn { get; set; }
    public DateTime Timestamp { get; set; }
}
