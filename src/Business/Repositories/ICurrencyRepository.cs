using System.Threading.Tasks;
using Conditus.Trader.Domain.Models;

namespace Business
{
    public interface ICurrencyRepository
    {
        Task<decimal> ConvertCurrency(string fromSymbol, string toSymbol, decimal amount);
    }
}