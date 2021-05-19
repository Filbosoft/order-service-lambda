using Conditus.Trader.Domain.Models;

namespace Acceptance.Seeds
{
    public static class ExchangeSeeds
    {
        public static readonly Exchange DKK_EXCHANGE = new Exchange
        {
            MIC = "f81594b9-f9ca-41d1-8045-a7087a544d8f",
            Acronym = "DKEX",
            Name = "DK Exchange"
        };
        public static readonly Exchange USD_EXCHANGE = new Exchange
        {
            MIC = "1052f566-17c1-442c-8f92-3d4abfe21119",
            Acronym = "USEX",
            Name = "US Exchange"
        };
        public static readonly Exchange EMPTY_EXCHANGE = new Exchange
        {
            MIC = "feea60cc-cb60-4ff0-90e5-19b7f7fcddad",
            Acronym = "EMTYEX",
            Name = "Empty Exchange"
        };
    }
}