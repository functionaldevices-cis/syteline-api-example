using System;

namespace syteline_api_example.Models;

public class SytelineCredentialsION
{
    public string ti
    {
        get; init;
    }
    public string cn
    {
        get; init;
    }
    public string dt
    {
        get; init;
    }
    public string ci
    {
        get; init;
    }
    public string cs
    {
        get; init;
    }
    public string iu
    {
        get; init;
    }
    public string pu
    {
        get; init;
    }
    public string oa
    {
        get; init;
    }
    public string ot
    {
        get; init;
    }
    public string or
    {
        get; init;
    }
    public string ev
    {
        get; init;
    }
    public string v
    {
        get; init;
    }
    public string saak
    {
        get; init;
    }
    public string sask
    {
        get; init;
    }

    public string obtain_token_endpoint => this.pu + this.ot;

    public string revoke_token_endpoint => this.pu + this.or;

    public string authorization_endpoint => this.pu + this.oa;

    public string base_url => this.iu + "/" + this.ti + "/CSI";

    public string client_id => this.ci;

    public string client_secret => this.cs;

    public string service_account_access_key => this.saak;

    public string service_account_secret_key => this.sask;

    public SytelineCredentialsION(string ti = "", string cn = "", string dt = "", string ci = "", string cs = "", string iu = "", string pu = "", string oa = "", string ot = "", string or = "", string ev = "", string v = "", string saak = "", string sask = "")
    {

        this.ti = ti;
        this.cn = cn;
        this.dt = dt;
        this.ci = ci;
        this.cs = cs;
        this.iu = iu;
        this.pu = pu;
        this.oa = oa;
        this.ot = ot;
        this.or = or;
        this.ev = ev;
        this.v = v;
        this.saak = saak;
        this.sask = sask;

    }

}
