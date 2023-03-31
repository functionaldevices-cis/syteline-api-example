# Syteline RESTv2 API Example Code

This repo contains a stripped down helper class to test connections to the Syteline RESTv2 API. It works for both direct connections using a Syteline account, and connections through ION, using a Mingle Account with Service Account on top. This is the code used to assemble the REST portion of the performance benchmarks discussed here: https://forums.sytelineusernetwork.com/t/syteline-api-analysis-v2-ion-restv2-vs-direct-restv2-vs-direct-soap/3347

To try it out, clone the repo, then in the Program.cs file, enter you credentials for whichever method you want to use (direct or ION). Comment out the other option. Run the program and it will return data. The sample defaults to showing a query of SLCustomers, with `CustNum = 'C000001'`.

# Limitations:

This repo only deals with loading data. Pushing data ito Syteline as well as all of the other options are not supported in this example file, but would be trivial to extend. It also is bloated because it currently supports both ION and direct connections. A real application would choose only one, which would allow stripping out a lot of duplicate functionality.
