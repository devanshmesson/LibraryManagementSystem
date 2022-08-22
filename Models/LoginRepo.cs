using LoginPage1.Models;
using System.Linq;

namespace LoginPage1.Models
{
    public class LoginRepo: ILoginRepo
    {
        private readonly LibraryDataContext ldc;
        public LoginRepo(LibraryDataContext ldc)
        {
            this.ldc = ldc;
        }
        public Account GetUsername(string username)
        {
            return ldc.Accounts.FirstOrDefault(u=> u.UserName==username);
        }
    }
}
