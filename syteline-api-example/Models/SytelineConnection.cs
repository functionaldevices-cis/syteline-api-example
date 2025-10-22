using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace syteline_api_example.Models;
public class SytelineConnection
{

    public string APIType { get; set; } = "Direct"; // "Direct"|"ION"

    public string Config { get; set; }

    public SytelineCredentialsDirect CredentialsDirect { get; set; }

    public SytelineCredentialsION CredentialsION { get; set; }

    public string BaseURL => this.APIType == "Direct" ? this.CredentialsDirect.BaseURL : this.CredentialsION.base_url;
    
    // CONSTRUCTOR

    public SytelineConnection(string APIType, string? Config = null, SytelineCredentialsDirect? CredentialsDirect = null, SytelineCredentialsION? CredentialsION = null)
    {
        this.APIType = APIType;
        this.Config = Config ?? "";
        this.CredentialsDirect = CredentialsDirect ?? new();
        this.CredentialsION = CredentialsION ?? new();
    }

}
