using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CandleStickMachineLearning.Services
{
    public class YahooFinanceService : IYahooFinanceService
    {
        private readonly ILogger<YahooFinanceService> _logger;
        private readonly IHttpClientFactory _clientFactory;
        public YahooFinanceService(ILoggerFactory loggerFactory, IHttpClientFactory clientFactory)
        {
            _logger = loggerFactory.CreateLogger<YahooFinanceService>();
            _clientFactory = clientFactory;
        }

        public async Task<List<Models.Bar>> GetBars(string symbol, DateTime startDateUtc, DateTime endDateUtc, string interval)
        {
            var quoteHistoryRoot = await GetQuoteHistoryRoot(symbol, EpochTime(startDateUtc), EpochTime(endDateUtc), interval);
            var barsList = GetBars(quoteHistoryRoot);
            return barsList;
        }

        private string historyUrlv8 = "https://query2.finance.yahoo.com/v8/finance/chart/SYMBOL?period1=PERIOD1&period2=PERIOD2&interval=INTERVAL&indicators=quote&includeTimestamps=true&includePrePost=true";
        /// <summary>
        /// Get history v8
        /// </summary>
        /// <param name="symbol">ex: AAPL</param>
        /// <param name="period1">UNIX timestammp start period</param>
        /// <param name="period2">UNIX timestammp start end</param>
        /// <param name="interval">  "1h", "1d", "5d", "1mo", "1y", "max"
        /// interval=3mo 3 months, going back until initial trading date.
        /// interval=1d 1 day, going back until initial trading date.
        /// interval=5m 5 minuets, going back 80(ish) days.
        /// interval=1m 1 minuet, going back 4-5 days.
        /// </param>
        /// <returns>Task<Models.QuoteHistoryRoot></returns>        
        protected async Task<Models.QuoteHistoryRoot> GetQuoteHistoryRoot(string symbol, long period1, long period2, string interval)
        {
            var client = _clientFactory.CreateClient();
            var requestUrl = historyUrlv8.Replace("SYMBOL", symbol).Replace("PERIOD1", period1.ToString()).Replace("PERIOD2", period2.ToString()).Replace("INTERVAL", interval);
            var httpResponseMessage = await client.GetAsync(requestUrl);
            var stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
            var quoteHistoryRoot = JsonConvert.DeserializeObject<Models.QuoteHistoryRoot>(stringResponse);
            return quoteHistoryRoot;
        }

        protected List<Models.Bar> GetBars(Models.QuoteHistoryRoot quoteHistory)
        {
            List<Models.Bar> barsList = new List<Models.Bar>();
            for (int i = 0; i < quoteHistory.chart.result[0].timestamp.Count; i++)
            {
                int gmtoffset = -14400;

                int timestamp = quoteHistory.chart.result[0].timestamp[i];
                int timestampWithOffset = timestamp + gmtoffset;
                DateTimeOffset timestampWithoutOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                DateTimeOffset timestampWitOffset = DateTimeOffset.FromUnixTimeSeconds(timestampWithOffset);
                var quote = quoteHistory.chart.result[0].indicators.quote[0];

                var bar = new Models.Bar()
                {
                    TimestampUtc = timestampWithoutOffset.DateTime
                    ,
                    TimestampOffset = timestampWitOffset.DateTime
                    ,
                    Open = quote.open[i]
                    ,
                    Close = quote.close[i]
                    ,
                    High = quote.high[i]
                    ,
                    Low = quote.low[i]
                    ,
                    Volume = quote.volume[i]
                    ,
                    EpochTimestamp = EpochTime(timestampWithoutOffset.DateTime)
                };
                if (barsList.Count > 0 && i > 0)
                {
                    bar.PreviousBar = barsList[i - 1];
                    barsList[i - 1].NextBar = bar;
                }
                barsList.Add(bar);
            }
            return barsList;
        }

        private int EpochTime(DateTime dateTime)
        {
            TimeSpan t = dateTime - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            return secondsSinceEpoch;
        }
    }
}
