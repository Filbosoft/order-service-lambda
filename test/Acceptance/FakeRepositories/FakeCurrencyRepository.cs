using System.Collections.Generic;
using System.Threading.Tasks;
using Business;
using Conditus.Trader.Domain.Models;

using static Acceptance.Seeds.CurrencySeeds;

namespace Acceptance.FakeRepositories
{
    public class FakeCurrencyRepository : ICurrencyRepository
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