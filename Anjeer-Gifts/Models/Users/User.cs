using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anjeer_Gifts.Models.Commons;
using Newtonsoft.Json;

namespace Anjeer_Gifts.Models.Users;

public class User : Auditable
{
    [JsonProperty("firstname")]
    public string FirstName { get; set; }
    [JsonProperty("lastname")]
    public string LastName { get; set; }
    [JsonProperty("phonenumber")]
    public string PhoneNumber { get; set; }
    [JsonProperty("regionname")]
    public string RegionName { get; set; }
}
