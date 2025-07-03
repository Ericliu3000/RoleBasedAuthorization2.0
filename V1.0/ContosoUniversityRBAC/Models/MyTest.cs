using System.Security.Claims;

namespace ContosoUniversityRBAC.Models
{
    public class MyTest
    {
        public IEnumerable<Claim> Claims { get;  set; }
        public IEnumerable<string> Roles { get;  set; }

    }
}
