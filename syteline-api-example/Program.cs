namespace syteline_api_example {

    internal class Program {

        static void Main() {

            // INITIALIZE THE RESTv2 API THROUGH ION, USING THE CREDENTIALS THAT YOU DOWNLOAD AFTER CREATING AN AUTHORIZED APP AND SERVICE ACCOUNT

            SytelineRestAPI sytelineRestAPIThroughION = new(
                IONAPICredentials: new IONAuthManager(
                    ti: "",
                    cn: "",
                    dt: "",
                    ci: "",
                    cs: "",
                    iu: "",
                    pu: "",
                    oa: "",
                    ot: "",
                    or: "",
                    ev: "",
                    v: "",
                    saak: "",
                    sask: ""
                ),
                config: ""
            );

            // INITIALIZE THE RESTv2 API DIRECTLY, USING YOUR REGULAR SYTELINE ACCOUNT CREDENTIALS

            SytelineRestAPI sytelineRestAPIDirect = new(
                sytelineCredentials: new SytelineAuthManager(
                    username: "",
                    password: "",
                    baseURL: ""
                ),
                config: ""
            );

            // LOAD A SAMPLE REQUEST

            LoadCollectionResponse response = sytelineRestAPIThroughION.LoadCollection(
                idoName: "SLCustomers",
                properties: new List<string>() {
                    "CustNum",
                    "CustSeq",
                    "Name",
                    "Addr_1",
                    "Addr_2",
                    "Addr_3",
                    "Addr_4",
                    "City",
                    "StateCode",
                    "Country"
                },
                filter: "CustNum = 'C000001'"
            );

            Console.WriteLine($"The request retrieved {response.Items.Count} items.");
            Console.ReadLine();

        }

    }

}