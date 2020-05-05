using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CandleStickMachineLearning.Services
{
    interface IYahooFinanceService
    {
        Task<List<Models.Bar>> GetBars(string symbol, DateTime startDateUtc, DateTime endDateUtc, string interval);
    }
}
