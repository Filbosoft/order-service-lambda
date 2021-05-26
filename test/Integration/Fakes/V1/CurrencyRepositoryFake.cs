using System.Collections.Generic;
using System.Threading.Tasks;
using Business;
using Conditus.Trader.Domain.Models;

using static Integration.Seeds.V1.CurrencySeeds;

namespace Integration.Fakes.V1
{
    public class CurrencyRepositoryFake : ICurrencyRepository
    {
        private static List<Currency> Currencies = new List<Currency>
        {
            DKK,
            USD
        };

        public async Task<decimal> ConvertCurrency(string fromSymbol, string toSymbol, decimal amount)
        {
            return amount * COVERSION_RATE;
        }
    }
}