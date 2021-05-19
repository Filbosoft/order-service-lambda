using System;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Models;

namespace Acceptance.Seeds
{
    public static class CurrencySeeds
    {
        public static readonly Currency DKK = new Currency
        {
            Code = "DKK",
            Name = "Danish Krone",
            Symbol = "Kr"
        };

        public static readonly Currency USD = new Currency
        {
            Code = "USD",
            Name = "United States Dollar",
            Symbol = "$"
        };
    }
}