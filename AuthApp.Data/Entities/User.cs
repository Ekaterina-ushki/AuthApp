using System.Collections.Generic;
using AuthApp.Data.Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace AuthApp.Data.Entities
{
    public class User : IdentityUser<int>, IEntity
    {
        public virtual ICollection<IdentityUserRole<int>> Roles { get; set; }
    }
}