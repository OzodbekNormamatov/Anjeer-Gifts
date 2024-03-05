using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anjeer_Gifts.Models.Commons;

namespace Anjeer_Gifts.Models.Users;

public class User : Auditable
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string PhoneNumber { get; set; }

    public string RegionName { get; set; }
}
