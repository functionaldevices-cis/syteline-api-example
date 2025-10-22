using syteline_api_example.Helpers;
using syteline_api_example.Models;

namespace syteline_api_example {

    internal class Program {

        static void Main() {

            // INITIALIZE THE RESTv2 API THROUGH ION, USING THE CREDENTIALS THAT YOU DOWNLOAD AFTER CREATING AN AUTHORIZED APP AND SERVICE ACCOUNT

            SytelineAPIRest_01 sytelineAPI_ION = new(
                new SytelineConnection(
                    APIType: "ION",
                    Config: "",
                    CredentialsION: new(
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
                    )
                )
            );

            // INITIALIZE THE RESTv2 API DIRECTLY, USING YOUR REGULAR SYTELINE ACCOUNT CREDENTIALS

            SytelineAPIRest_01 sytelineAPI_Direct = new(
                new SytelineConnection(
                    APIType: "Direct",
                    Config: "",
                    CredentialsDirect: new(
                        Username: "",
                        Password: "",
                        BaseURL: "https://csi10c.erpsl.inforcloudsuite.com"
                    )
                )
            );

            // LOAD A SAMPLE REQUEST

            APILoadCollectionResponse response_ION = sytelineAPI_ION.LoadCollection(
                idoName: "SLCustomers",
                properties: [
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
                ],
                filter: "CustNum = 'C000001'"
            );

            // LOAD A SAMPLE REQUEST

            APILoadCollectionResponse response_Direct = sytelineAPI_Direct.LoadCollection(
                idoName: "SLCustomers",
                properties: [
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
                ],
                filter: "CustNum = 'C000001'"
            );

            Console.WriteLine($"The ION request retrieved {response_ION.Items.Count} items.");
            Console.WriteLine($"The Direct request retrieved {response_Direct.Items.Count} items.");
            Console.ReadLine();

        }

    }

}