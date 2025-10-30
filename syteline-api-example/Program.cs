using syteline_api_example.Helpers;
using syteline_api_example.Models;

namespace syteline_api_example {

    internal class Program {

        static void Main() {

            /*********************************************************************************************/
            /* API GUIDE - PART 2 - AUTHENTICATION
            /*********************************************************************************************/

            // INITIALIZE THE RESTv2 API THROUGH ION, USING THE CREDENTIALS THAT YOU DOWNLOAD AFTER CREATING AN AUTHORIZED APP AND SERVICE ACCOUNT

            SytelineAPIRest_00 sytelineAPI_00_ION = new(
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

            SytelineAPIRest_00 sytelineAPI_00_Direct = new(
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

            /*********************************************************************************************/
            /* API GUIDE - PART 3 - LOADING RECORDS - EXAMPLE 1: LOADING SIMPLE IDO, NO PAGINATION
            /*********************************************************************************************/

            // INITIALIZE THE RESTv2 API THROUGH ION, USING THE CREDENTIALS THAT YOU DOWNLOAD AFTER CREATING AN AUTHORIZED APP AND SERVICE ACCOUNT
            // ( FEEL FREE TO USE DIRECT CREDENTIALS HERE INSTEAD)
 
            SytelineAPIRest_01 sytelineAPI_01 = new(
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

            // LOAD A SAMPLE REQUEST

            APILoadCollectionResponse response_01 = sytelineAPI_01.LoadCollection(
                idoName: "SLCustomers",
                properties: [
                    "CustNum",
                    "CustSeq",
                    "Name",
                ],
                filter: "CustNum = 'C000079'",
                readOnly: true
            );

            Console.WriteLine($"The request retrieved {response_01.Items.Count} items.");

            /*********************************************************************************************/
            /* API GUIDE - PART 3 - LOADING RECORDS - EXAMPLE 1: LOADING SIMPLE IDO, NO PAGINATION
            /*********************************************************************************************/

            // INITIALIZE THE RESTv2 API THROUGH ION, USING THE CREDENTIALS THAT YOU DOWNLOAD AFTER CREATING AN AUTHORIZED APP AND SERVICE ACCOUNT
            // ( FEEL FREE TO USE DIRECT CREDENTIALS HERE INSTEAD)

            SytelineAPIRest_02 sytelineAPI_02 = new(
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

            // LOAD A SAMPLE REQUEST

            APILoadCollectionResponse response_02 = sytelineAPI_02.LoadCollection(
                idoName: "SLCustomers",
                properties: [
                    "CustNum",
                    "CustSeq",
                    "Name",
                ],
                filter: "CustNum = 'C000079'",
                readOnly: true
            );

            Console.WriteLine($"The request retrieved {response_02.Items.Count} items.");

        }

    }

}