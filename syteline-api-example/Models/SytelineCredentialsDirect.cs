using System;

namespace syteline_api_example.Models;

public class SytelineCredentialsDirect
{

    public string Username { get; init; }

    public string Password { get; init; }

    public string BaseURL { get; init; }

    public SytelineCredentialsDirect(string Username = "", string Password = "", string BaseURL = "")
    {
        this.Username = Username;
        this.Password = Password;
        this.BaseURL = BaseURL;
    }

}