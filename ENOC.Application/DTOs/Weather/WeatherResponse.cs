namespace ENOC.Application.DTOs.Weather;

public class WeatherResponse
{
    public decimal Temp1 { get; set; }
    public int RelHumidity { get; set; }
    public decimal HeatIndex { get; set; }
    public decimal DewPoint { get; set; }
    public decimal WindChill { get; set; }
    public int RawWindDir { get; set; }
    public decimal WindSpeed { get; set; }
    public decimal ThreeSecRollAvgWindSpeed { get; set; }
    public int ThreeSecRollAvgWindDir { get; set; }
    public decimal TwoMinRollAvgWindSpeed { get; set; }
    public int TwoMinRollAvgWindDir { get; set; }
    public decimal TenMinRollAvgWindSpeed { get; set; }
    public int TenMinRollAvgWindDir { get; set; }
    public decimal TenMinWindGustSpeed { get; set; }
    public int TenMinWindGustDir { get; set; }
    public decimal SixtyMinWindGustSpeed { get; set; }
    public int SixtyMinWindGustDir { get; set; }
    public decimal AdjBaromPress { get; set; }
    public decimal RainToday { get; set; }
    public DateTime Timestamp { get; set; }
}
