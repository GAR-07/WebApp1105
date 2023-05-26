using WebApp1105.API.Data.Models;

namespace WebApp1105.Data.Interfaces
{
    public interface IAllAccounts
    {
        IEnumerable<Account> Accounts { get; }
    }
}