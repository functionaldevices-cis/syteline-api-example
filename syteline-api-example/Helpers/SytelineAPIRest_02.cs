using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using syteline_api_example.Models;

namespace syteline_api_example.Helpers;

public class SytelineAPIRest_02
{
    private int StreamingRecordCap
    {
        get; init;
    }

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

    public SytelineAPIRest_02(SytelineConnection connection)
    {

        this.SytelineConnection = connection;

        // INIT SETTINGS

        this.StreamingRecordCap = 200;

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

                }
                else
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

    public APILoadCollectionResponse LoadCollection(string idoName, List<string> properties, string? filter = null, List<OrderByProperty>? orderBy = null, int? recordCap = null, bool? distinct = null, string? clm = null, List<string>? clmParam = null, string? pqc = null, bool? readOnly = null, Action<ActionStatus>? onStartCallback = null, Action<ActionStatus>? onProgressCallback = null, Action<ActionStatus>? onCompleteCallback = null)
    {

        // PARSE INPUT PARAMS

        string? orderByString = orderBy == null ? null : string.Join(", ", (orderBy ?? []).Select(property => property.OrderBy));

        // INIT GENERAL VARS

        APILoadCollectionResponse parsedResponseContent;
        List<Dictionary<string, object?>> data = [];

        // INIT PAGINATION VARS

        int requestCap = recordCap != null ? Math.Min((int)recordCap, this.StreamingRecordCap) : this.StreamingRecordCap;
        bool haveToPaginate = recordCap == null || recordCap >= this.StreamingRecordCap;
        bool moreRowsExist;
        bool totalCapNotMet = true;
        string? bookmark = null;
        int? totalToRetrieve = null;

        // IF WE ARE PAGINATING AND WE ARE USING SOAP, WE HAVE TO ADD THE ORDERBY AND PAGING PROPERTIES TO THE QUERY

        onStartCallback?.Invoke(new(
            CountTotal: totalToRetrieve ?? -1,
            Status: "Initializing",
            Success: true
        ));

        do
        {

            // INSERT PAGINATION FILTERS AND ORDERBY

            parsedResponseContent = LoadCollectionBatch(
                idoName: idoName,
                properties: properties,
                filter: filter,
                orderBy: orderByString,
                requestCap: requestCap,
                distinct: distinct,
                clm: clm,
                clmParam: clmParam,
                bookmark: bookmark,
                pqc: pqc,
                readOnly: readOnly
            );

            if (parsedResponseContent.Success == false)
            {
                break;
            }

            // UPDATE LOOP VARS

            bookmark = parsedResponseContent.Bookmark;
            moreRowsExist = parsedResponseContent.MoreRowsExist;

            data = data.Concat(parsedResponseContent.Items).ToList();

            onProgressCallback?.Invoke(new(
                CountTotal: totalToRetrieve ?? -1,
                CountCompleted: data.Count,
                Status: "Loading",
                Success: true
            ));

            if (recordCap != null)
            {
                if (recordCap <= data.Count)
                {
                    totalCapNotMet = false;
                }
            }

        } while (moreRowsExist && totalCapNotMet);

        parsedResponseContent.Items = data;
        parsedResponseContent.Config = this.SytelineConnection.Config;

        onCompleteCallback?.Invoke(new(
            CountTotal: totalToRetrieve ?? -1,
            CountCompleted: data.Count,
            Status: "Loading",
            Success: parsedResponseContent.Success
        ));

        return parsedResponseContent;

    }

    private APILoadCollectionResponse LoadCollectionBatch(string idoName, List<string> properties, string? filter, string? orderBy = null, int? requestCap = 0, bool? distinct = null, string? clm = null, List<string>? clmParam = null, string? bookmark = null, string? pqc = null, bool? readOnly = null)
    {

        // BUILD THE REQUEST

        string requestURL = this.SytelineConnection.BaseURL + "/IDORequestService/ido/load/" + idoName + "?" +  BuildLoadCollectionParametersString(
            properties: properties,
            filter: filter,
            orderBy: orderBy,
            recordCap: requestCap,
            distinct: distinct,
            clm: clm,
            clmParam: clmParam,
            bookmark: bookmark,
            pqc: pqc,
            readOnly: readOnly
        );

        HttpRequestMessage request = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(requestURL),
            Headers =
            {
                { "Accept", "application/json" },
                { "X-Infor-MongooseConfig", this.SytelineConnection.Config }
            }
        };
        request.Headers.TryAddWithoutValidation("Authorization", this.GetAccessToken().Token);

        // LOAD THE REQUEST

        HttpResponseMessage httpResponse = this.HttpClient.SendAsync(request).Result;

        // PARSE THE REQUEST

        APILoadCollectionResponse parsedResponse = System.Text.Json.JsonSerializer.Deserialize<APILoadCollectionResponse>(httpResponse.Content.ReadAsStringAsync().Result) ?? throw new Exception("Unable to parse response.");

        return parsedResponse;

    }

    private static string BuildLoadCollectionParametersString(List<string> properties, string? filter = null, string? orderBy = null, int? recordCap = null, bool? distinct = null, string? clm = null, List<string>? clmParam = null, string? bookmark = null, string? pqc = null, bool? readOnly = null)
    {

        // CREATE LIST OF PARAMTERS TO STRINGY

        List<string> lQueryPrameters = [
            "properties=" + string.Join(",", properties)
        ];

        if (filter != null)
        {
            lQueryPrameters.Add("filter=" +  EncodeValue(filter));
        }

        if (orderBy != null)
        {
            lQueryPrameters.Add("orderBy=" +  EncodeValue(orderBy));
        }

        if (recordCap != null)
        {
            lQueryPrameters.Add("recordCap=" + EncodeValue(recordCap));
        }

        if (distinct != null)
        {
            lQueryPrameters.Add("distinct=" + EncodeValue(distinct));
        }

        if (clm != null)
        {
            lQueryPrameters.Add("clm=" + clm);
        }

        if (clmParam != null)
        {
            lQueryPrameters.Add("clmParam=" + EncodeValue(string.Join(",", clmParam)));
        }

        lQueryPrameters.Add("loadType=" + EncodeValue("NEXT"));

        if (bookmark != null)
        {
            lQueryPrameters.Add("bookmark=" + EncodeValue(bookmark));
        }

        if (pqc != null)
        {
            lQueryPrameters.Add("pqc=" + EncodeValue(pqc));
        }

        if (readOnly != null)
        {
            lQueryPrameters.Add("readOnly=" + EncodeValue(readOnly));
        }

        // BUILD THE REQUEST URL

        return string.Join("&", lQueryPrameters);

    }

    private static string EncodeValue(object value)
    {

        return Uri.EscapeDataString(ConvertToString(value));

    }

    private static string ConvertToString(object rawValue)
    {

        System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.InvariantCulture;
        if (rawValue is Enum)
        {
            string? name = Enum.GetName(rawValue.GetType(), rawValue);
            if (name != null)
            {
                System.Reflection.FieldInfo? field = System.Reflection.IntrospectionExtensions.GetTypeInfo(rawValue.GetType()).GetDeclaredField(name);
                if (field != null)
                {
                    System.Runtime.Serialization.EnumMemberAttribute? attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute)) as System.Runtime.Serialization.EnumMemberAttribute;
                    if (attribute != null)
                    {
                        return attribute.Value ?? name;
                    }
                }
            }
        }
        else if (rawValue is bool)
        {
            return (Convert.ToString(rawValue, cultureInfo) ?? "true").ToLowerInvariant();
        }
        else if (rawValue is byte[])
        {
            return Convert.ToBase64String((byte[])rawValue);
        }
        else if (rawValue != null && (rawValue.GetType().IsArray))
        {
            var array = Enumerable.OfType<object>((Array)rawValue);
            return string.Join(",", Enumerable.Select(array, o => ConvertToString(o)));
        }

        return Convert.ToString(rawValue, cultureInfo) ?? "";

    }

}
