using System.Threading.Tasks;
using Conditus.Trader.Domain.Models;

namespace Business.Repositories
{
    public interface ICurrencyRepository
    {
        Task<decimal> ConvertCurrency(string fromCode, string toCode, decimal amount);
    }
}