using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using syteline_api_example.Models;

namespace syteline_api_example.Helpers;

public class SytelineAPIRest_00
{
    private HttpClient HttpClient
    {
        get; init;
    }

    public SytelineConnection SytelineConnection
    {
        get; set;
    }

    private APIAccessTokenDetails AccessTokenDetails {
        get; set;
    } = new();

    public SytelineAPIRest_00(SytelineConnection connection)
    {

        this.SytelineConnection = connection;

        // INIT HTTP CLIENT

        this.HttpClient = new();
        this.HttpClient.DefaultRequestHeaders.Add("X-Infor-MongooseConfig", "");

    }

    public APIAccessTokenDetails GetAccessToken()
    {

        // CHECK TO SEE IF WE NEED A NEW TOKEN

        if ((this.AccessTokenDetails.Token == "") || ((this.AccessTokenDetails.Expiration != null && this.AccessTokenDetails.Expiration >= DateTime.Now.AddMinutes(10))))
        {

            // TRY TO GET THE TOKEN

            try
            {

                if (this.SytelineConnection.APIType == "Direct")
                {
                    // LOAD THE REQUEST

                    HttpResponseMessage httpResponse = this.HttpClient.SendAsync(new HttpRequestMessage()
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri($"{this.SytelineConnection.BaseURL}/IDORequestService/ido/token/{this.SytelineConnection.Config}"),
                        Headers =
                        {
                            { "username", this.SytelineConnection.CredentialsDirect.Username },
                            { "password", this.SytelineConnection.CredentialsDirect.Password }
                        }
                    }).Result;

                    Dictionary<string, object> parsedResponseContent = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(httpResponse.Content.ReadAsStringAsync().Result) ?? throw new Exception("Unable to parse response.");

                    this.AccessTokenDetails.Token = (parsedResponseContent["Token"] ?? "").ToString() ?? "";
                    this.AccessTokenDetails.Expiration = DateTime.Now.AddSeconds((7200));
                    this.AccessTokenDetails.Valid = (parsedResponseContent["Success"].ToString() == "True");

                    this.AccessTokenDetails.Message = this.AccessTokenDetails.Valid ? "Successfully connected and authenticated." : "Unable to load access token. Please check credentials.";

                } else
                {


                    HttpResponseMessage httpResponse = this.HttpClient.SendAsync(new HttpRequestMessage()
                    {
                        Method = HttpMethod.Post,
                        RequestUri = new Uri(this.SytelineConnection.CredentialsION.obtain_token_endpoint),
                        Content = new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "client_id", this.SytelineConnection.CredentialsION.client_id },
                            { "client_secret", this.SytelineConnection.CredentialsION.client_secret },
                            { "grant_type", "password" },
                            { "username", this.SytelineConnection.CredentialsION.service_account_access_key },
                            { "password", this.SytelineConnection.CredentialsION.service_account_secret_key }
                        })
                    }).Result;

                    if (httpResponse.IsSuccessStatusCode)
                    {

                        Dictionary<string, object> parsedResponseContent = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(httpResponse.Content.ReadAsStringAsync().Result) ?? throw new Exception("Unable to parse response.");

                        this.AccessTokenDetails.Token = "Bearer " + (parsedResponseContent["access_token"] ?? "").ToString() ?? "";
                        this.AccessTokenDetails.Expiration = DateTime.Now.AddSeconds(int.Parse(parsedResponseContent["expires_in"].ToString() ?? "7200"));
                        this.AccessTokenDetails.Valid = true;

                        this.AccessTokenDetails.Message = this.AccessTokenDetails.Valid ? "Successfully connected and authenticated." : "Unable to load access token. Please check credentials.";

                    }

                }
            }
            catch (Exception ex)
            {
                this.AccessTokenDetails.Token = "";
                this.AccessTokenDetails.Expiration = null;
                this.AccessTokenDetails.Valid = false;
                this.AccessTokenDetails.Message = ex.Message;

            }

        }

        return this.AccessTokenDetails;

    }

}
