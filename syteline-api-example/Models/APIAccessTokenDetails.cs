using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace syteline_api_example.Models;
public class APIAccessTokenDetails
{

    public string Token
    {
        get; set;
    }
    public DateTime? Expiration
    {
        get; set;
    }

    public bool Valid
    {
        get; set;
    }

    public string Message
    {
        get; set;
    }

    public APIAccessTokenDetails(string token = "", DateTime? expiration = null, bool valid = false, string message = "")
    {
        this.Token = token;
        this.Expiration = expiration;
        this.Valid = valid;
        this.Message = message;
    }

}