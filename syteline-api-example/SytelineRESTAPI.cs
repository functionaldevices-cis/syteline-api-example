using Thinktecture.IdentityModel.Client;

namespace syteline_api_example {

    public class SytelineRestAPI {

        private int StreamingRecordCap { get; init; }

        private IONAuthManager? IONAuthManager { get; init; }

        private SytelineAuthManager? SytelineAuthManager { get; init; }

        private HttpClient HttpClient { get; init; }

        private string BaseURL {
            get {
                if (IONAuthManager != null) {
                    return IONAuthManager.BaseURL + "/IDORequestService/ido";
                } else if (SytelineAuthManager != null) {
                    return SytelineAuthManager.BaseURL + "/IDORequestService/ido";
                } else {
                    throw new Exception("Error: No base URL found.");
                }
            }
        }

        private DateTime? AccessTokenExpiration { get; set; } = null;

        private string AccessToken { get; set; } = "";

        public SytelineRestAPI(string config, IONAuthManager? IONAPICredentials = null, SytelineAuthManager? sytelineCredentials = null, int streamingRecordCap = 10000) {

            // INIT SETTINGS

            this.StreamingRecordCap = streamingRecordCap;

            // INIT CREDENTIALS

            this.IONAuthManager = IONAPICredentials;
            this.SytelineAuthManager = sytelineCredentials;

            // INIT HTTP CLIENT

            this.HttpClient = new();
            this.HttpClient.DefaultRequestHeaders.Add("X-Infor-MongooseConfig", config);

            // INJECT EXTRA DEPENDENCIES, BECAUSE SYTELINE DIRECT DOESN'T USE THE OAUTH MANAGER TO IT NEEDS TO USE HTTPCLIENT DIRECTLY

            if (this.SytelineAuthManager != null) {
                this.SytelineAuthManager.Config = config;
                this.SytelineAuthManager.HttpClient = this.HttpClient;
            }

        }

        private string GetAccessToken() {

            // CHECK TO SEE IF WE NEED A NEW TOKEN

            if ((this.AccessToken == "") || ((this.AccessTokenExpiration != null && this.AccessTokenExpiration >= DateTime.Now.AddMinutes(10)))) {

                AccessTokenResponse response;

                // DETERMINE HOW TO GET TOKEN

                if (this.IONAuthManager != null) {

                    // TRY TO GET NEW ACCESS TOKEN

                    response = this.IONAuthManager.GetToken();

                } else if (this.SytelineAuthManager != null) {

                    response = this.SytelineAuthManager.GetToken();

                } else {

                    throw new Exception("Error: No base URL found.");

                }

                // GET TO SEE IF WE SUCCEEDED

                if (!response.IsError) {

                    // UPDATE THE ACCESS/BEARER TOKEN EXPIRATION AND PUSH THE TOKEN INTO HTTP CLIENT

                    this.AccessToken = response.Token;
                    this.AccessTokenExpiration = DateTime.Now.AddSeconds(response.ExpiresIn);

                } else {

                    // THROW ERROR

                    this.AccessToken = "";
                    this.AccessTokenExpiration = null;
                    throw new Exception("Unable to get bearer token");

                }

            }

            return this.AccessToken;

        }

        public LoadCollectionResponse LoadCollection(string idoName, List<string> properties, string? filter = null, string? orderBy = null, int? recordCap = null, bool? distinct = null, string? clm = null, List<string>? clmParam = null, string? pqc = null, bool? readOnly = null) {

            // INIT VARS

            string requestURL;
            int requestCap;
            bool moreRowsExist;
            bool totalCapNotMet = true;
            string? bookmark = null;
            HttpResponseMessage httpResponse;
            LoadCollectionResponse parsedResponseContent;
            List<Dictionary<string, string>> data = new();
            this.HttpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", this.GetAccessToken());

            if (recordCap != null) {
                requestCap = Math.Min((int)recordCap, this.StreamingRecordCap);
            } else {
                requestCap = this.StreamingRecordCap;
            }

            do {

                // BUILD THE REQUEST URL

                requestURL = BuildRequestURL(
                    baseURL: this.BaseURL,
                    idoName: idoName,
                    properties: properties,
                    filter: filter,
                    orderBy: orderBy,
                    recordCap: requestCap,
                    distinct: distinct,
                    clm: clm,
                    clmParam: clmParam,
                    loadType: "NEXT",
                    bookmark: bookmark,
                    pqc: pqc,
                    readOnly: readOnly
                );

                // LOAD THE REQUEST

                httpResponse = this.HttpClient.GetAsync(requestURL).Result;

                // PARSE THE REQUEST

                parsedResponseContent = System.Text.Json.JsonSerializer.Deserialize<LoadCollectionResponse>(httpResponse.Content.ReadAsStringAsync().Result) ?? throw new Exception("Unable to parse response.");

                // UPDATE LOOP VARS

                bookmark = parsedResponseContent.Bookmark;
                moreRowsExist = parsedResponseContent.MoreRowsExist;
                data = data.Concat(parsedResponseContent.Items).ToList();

                if (recordCap != null) {
                    if (recordCap <= data.Count) {
                        totalCapNotMet = false;
                    }
                }

            } while (moreRowsExist && totalCapNotMet);

            parsedResponseContent.Items = data;

            return parsedResponseContent;

        }

        private static string BuildRequestURL(string baseURL, string idoName, List<string> properties, string? filter = null, string? orderBy = null, int? recordCap = null, bool? distinct = null, string? clm = null, List<string>? clmParam = null, string? loadType = null, string? bookmark = null, string? pqc = null, bool? readOnly = null) {

            // CREATE LIST OF PARAMTERS TO STRINGY

            List<string> lQueryPrameters = new() {
                { "properties=" + String.Join(",", properties) }
            };

            if (filter != null) {
                lQueryPrameters.Add("filter=" +  EncodeValue(filter));
            }

            if (orderBy != null) {
                lQueryPrameters.Add("orderBy=" +  EncodeValue(orderBy));
            }

            if (recordCap != null) {
                lQueryPrameters.Add("recordCap=" + EncodeValue(recordCap));
            }

            if (distinct != null) {
                lQueryPrameters.Add("distinct=" + EncodeValue(distinct));
            }

            if (clm != null) {
                lQueryPrameters.Add("clm=" + clm);
            }

            if (clmParam != null) {
                lQueryPrameters.Add("clmParam=" + EncodeValue(clmParam));
            }

            if (loadType != null) {
                lQueryPrameters.Add("loadType=" + EncodeValue(loadType));
            }

            if (bookmark != null) {
                lQueryPrameters.Add("bookmark=" + EncodeValue(bookmark));
            }

            if (pqc != null) {
                lQueryPrameters.Add("pqc=" + EncodeValue(pqc));
            }

            if (readOnly != null) {
                lQueryPrameters.Add("readOnly=" + EncodeValue(readOnly));
            }

            // BUILD THE REQUEST URL

            return baseURL + "/load/" + idoName + "?" + String.Join("&", lQueryPrameters);

        }

        private static string EncodeValue(object value) {

            return Uri.EscapeDataString(ConvertToString(value));

        }

        private static string ConvertToString(object rawValue) {

            System.Globalization.CultureInfo cultureInfo = System.Globalization.CultureInfo.InvariantCulture;
            if (rawValue is Enum) {
                string? name = Enum.GetName(rawValue.GetType(), rawValue);
                if (name != null) {
                    System.Reflection.FieldInfo? field = System.Reflection.IntrospectionExtensions.GetTypeInfo(rawValue.GetType()).GetDeclaredField(name);
                    if (field != null) {
                        System.Runtime.Serialization.EnumMemberAttribute? attribute = System.Reflection.CustomAttributeExtensions.GetCustomAttribute(field, typeof(System.Runtime.Serialization.EnumMemberAttribute)) as System.Runtime.Serialization.EnumMemberAttribute;
                        if (attribute != null) {
                            return attribute.Value ?? name;
                        }
                    }
                }
            } else if (rawValue is bool) {
                return (Convert.ToString(rawValue, cultureInfo) ?? "true").ToLowerInvariant();
            } else if (rawValue is byte[]) {
                return Convert.ToBase64String((byte[])rawValue);
            } else if (rawValue != null && rawValue.GetType().IsArray) {
                var array = Enumerable.OfType<object>((Array)rawValue);
                return string.Join(",", Enumerable.Select(array, o => ConvertToString(o)));
            }

            return Convert.ToString(rawValue, cultureInfo) ?? "";

        }

    }

    public partial class MethodResponse {
        public bool? Success { get; set; }

        public string? Message { get; set; }


    }

    public partial class LoadCollectionResponse : MethodResponse {
        public required List<Dictionary<string, string>> Items { get; set; }

        public required string Bookmark { get; set; }

        public required bool MoreRowsExist { get; set; }

    }

    public class IONAuthManager {

        private string ti { get; init; }
        private string cn { get; init; }
        private string dt { get; init; }
        private string ci { get; init; }
        private string cs { get; init; }
        private string iu { get; init; }
        private string pu { get; init; }
        private string oa { get; init; }
        private string ot { get; init; }
        private string or { get; init; }
        private string ev { get; init; }
        private string v { get; init; }
        private string saak { get; init; }
        private string sask { get; init; }

        private OAuth2Client OAuth2 { get; init; }

        public string ObtainTokenEndpoint {
            get { return pu + ot; }
        }

        public string RevokeTokenEndpoint {
            get { return pu + or; }
        }

        public string AuthorizationEndpoint {
            get { return pu + oa; }
        }

        public string BaseURL {
            get { return iu + "/" + ti + "/CSI"; }
        }

        public string ClientID {
            get { return ci; }
        }

        public string ClientSecret {
            get { return cs; }
        }

        public string ServiceAccountAccessKey {
            get { return saak; }
        }

        public string ServiceAccountSecretKey {
            get { return sask; }
        }

        public IONAuthManager(string ti, string cn, string dt, string ci, string cs, string iu, string pu, string oa, string ot, string or, string ev, string v, string saak, string sask) {

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

            this.OAuth2 = new OAuth2Client(new Uri(this.ObtainTokenEndpoint), this.ClientID, this.ClientSecret);

        }

        public AccessTokenResponse GetToken() {

            TokenResponse parsedResponseContent = OAuth2.RequestResourceOwnerPasswordAsync(this.ServiceAccountAccessKey, this.ServiceAccountSecretKey).Result;

            return new AccessTokenResponse() {
                Token = "Bearer " + parsedResponseContent.AccessToken,
                ExpiresIn = parsedResponseContent.ExpiresIn,
                IsError = parsedResponseContent.IsError
            };

        }

    }

    public class SytelineAuthManager {

        public string Username { get; init; }
        public string Password { get; init; }
        public string BaseURL { get; init; }
        public string? Config { get; set; }
        public HttpClient? HttpClient { get; set; }

        public SytelineAuthManager(string username, string password, string baseURL) {
            this.Username = username;
            this.Password = password;
            this.BaseURL = baseURL;
        }

        public AccessTokenResponse GetToken() {

            if (this.HttpClient != null) {

                string requestURL = $"{this.BaseURL}/IDORequestService/ido/token/{this.Config}/{Uri.EscapeDataString(this.Username)}/{Uri.EscapeDataString(this.Password)}";

                HttpResponseMessage httpResponse = this.HttpClient.GetAsync(requestURL).Result;

                // PARSE THE REQUEST

                Dictionary<string, object> parsedResponseContent = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(httpResponse.Content.ReadAsStringAsync().Result) ?? throw new Exception("Unable to parse response.");

                return new AccessTokenResponse() {
                    Token = parsedResponseContent["Token"].ToString() ?? "",
                    ExpiresIn = (2 * 3600),
                    IsError = (parsedResponseContent["Success"].ToString() == "false")
                };

            } else {

                throw new Exception("Must have an HttpClient in order to get access token.");
            
            }

        }

    }

    public class AccessTokenResponse {

        public required string Token { get; init; }
        public required long ExpiresIn { get; init; }
        public required bool IsError { get; init; }

    }

}
