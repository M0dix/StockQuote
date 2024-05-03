using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

class Program
{ 
    static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        Console.WriteLine("Введите тикер:");
        var ticker = Console.ReadLine();

        try
        {
            await GetStockQuotes(ticker, token)
                .ContinueWith(t => Console.WriteLine($"Средний объем торгов за последнюю неделю:{CalculateAverageWeeklyTradingVolume(t.Result)}"));
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Получение котировок было отменено");
        }
    }

    private static async Task<IEnumerable<StockQuote>> GetStockQuotes(string ticker, CancellationToken token)
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "StockPrices_Small.csv");

        var quotesTask = await ReadStockQuotesFromCsvAsync(filePath);
        var quotes = quotesTask.Where(q => q.Ticker == ticker);

        foreach (var quote in quotes)
        {
            //await SimulateSomeWorkAsync(token);

            token.ThrowIfCancellationRequested();

            Console.WriteLine($"Дата: {quote.TradeDate}, Тикер: {quote.Ticker}, Объём: {quote.Volume}, Изменение: {quote.Change}, Изменение(%): {quote.ChangePercent}");
        }

        return quotes;
    }

    private static async Task<IEnumerable<StockQuote>> ReadStockQuotesFromCsvAsync(string filePath)
    {
        var quotes = new List<StockQuote>();

        using (var reader = new StreamReader(filePath))
        {
            reader.ReadLine(); 

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var values = line.Split(',');

                quotes.Add(new StockQuote
                {
                    Ticker = values[0],
                    TradeDate = DateTime.ParseExact(values[1], "M/d/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture, DateTimeStyles.None),
                    Volume = long.Parse(values[6]),
                    Change = float.Parse(values[7], CultureInfo.InvariantCulture),
                    ChangePercent = float.Parse(values[8], CultureInfo.InvariantCulture)
                });
            }
        }
        return quotes;
    }

    private static async Task SimulateSomeWorkAsync(CancellationToken token)
    {
        await Task.Delay(1000, token);
    }

    private static long CalculateAverageWeeklyTradingVolume(IEnumerable<StockQuote> quotes)
    {
        var currentWeekQuotes = quotes.TakeLast(7);
        return currentWeekQuotes.Sum(q => q.Volume) / 7;
    }
}

public class StockQuote
{
    public string? Ticker { get; set; }
    public DateTime TradeDate { get; set; }
    public long Volume { get; set;}
    public float Change { get; set;}
    public float ChangePercent { get; set;}
}


