using System;
using Conditus.Trader.Domain.Entities;
using Conditus.Trader.Domain.Models;

namespace Integration.Seeds.V1
{
    public static class CurrencySeeds
    {
        public const decimal COVERSION_RATE = 5M;
        
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