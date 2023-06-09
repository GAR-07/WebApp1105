using WebApp1105.Data.Interfaces;
using WebApp1105.API.Data.Models;
using WebApp1105.Models;

namespace WebApp1105.Data.Repository
{
	public class AccountRepository : IAllAccounts
    {
		private readonly ApplicationDbContext ApplicationDbContext;

		public AccountRepository(ApplicationDbContext ApplicationDbContext) {this.ApplicationDbContext = ApplicationDbContext; }
	
		public IEnumerable<Account> Accounts => ApplicationDbContext.Accounts;
	}
}