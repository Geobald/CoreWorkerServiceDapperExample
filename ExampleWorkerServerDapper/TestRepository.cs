using Dapper;
using ExampleWorkerServerDapper.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace ExampleWorkerServerDapper
{
    public class TestRepository : ITestRepository
    {
        private readonly string _connectionString;

        public TestRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public bool Truncate()
        {
            List<LookupModel> result;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                result = connection.Query<LookupModel>(
                    "TRUNCATE TABLE [TheTable]; " +
                    "SELECT * FROM [TheTable];"
                    ).ToList();
                connection.Close();

                return result.Count == 0 ? true : false;
            }
        }

        public bool Populate()
        {
            try
            {

                // Create required vars
                List<string> distinctStuff;
                List<LookupModel> Lookup = null;
                List<DemoModel> Demo;
                string insertStatement = "";

                // Create dictionary to pick up odd input styles for data to convert them
                Dictionary<string, string> anomalies = new Dictionary<string, string>() {

                { "A", "Val1" },
                { "B", "Val2" },
                { "C", "Val3" },
                { "D", "Val4" },
                { "E", "Val5" },
                { "F", "Val6" }
            };

                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Create new instance of the [DemoTable] table and use as reference
                    Demo = connection.Query<DemoModel>("SELECT * FROM [DemoTable]").ToList();

                    // Get a list of distinct stuff from the TestStagingTable Table
                    distinctStuff = connection.Query<string>("SELECT DISTINCT [value] FROM [TestStagingTable]").ToList();

                    // Create some logic to loop through each distinct thing
                    foreach (var item in distinctStuff)
                    {
                        // Get standard fields
                        int parseStart = item.Contains("Val1") ? 1 : 0;
                        int parseIndicatorOne = item.Contains("Val2") ? 1 : 0;
                        int parseIndicatorTwo = item.Contains("Val3") ? 1 : 0;

                        // Parse each row string from Staging item
                        int begin = parseStart == 1 ? 6 : 3; // figure out starting pos
                        int end = parseStart == 1 ? item.Length - 6 : item.Length - 3;
                        var secondArg = item.Substring(begin, end).Trim().Split(' ');
                        string demoRes = "";
                        foreach (var o in secondArg)
                        {
                            if (o.Contains("EOSIndicator"))
                            {
                                break;
                            }
                            else
                            {
                                 demoRes += $" { (anomalies.ContainsKey(o) ? anomalies[o] : o ) }";
                            }
                        }
                        demoRes = demoRes.TrimStart();

                        // Get DemoID from above
                        int demoId = (from o in Demo where o.Description.Replace(@"-", "") == demoRes select o.ID).FirstOrDefault();

                        // Add to dataModel
                        insertStatement = insertStatement +
                            "INSERT INTO [FinalTable] (Item, Id, Col1, Col2, Col3, Col4) " +
                            $"VALUES( {"'" + item + "'"}, {demoId}, 1, {parseStart}, {parseIndicatorOne}, {parseIndicatorTwo})";
                    }

                    // Do final insert here
                    Lookup = connection.Query<LookupModel>(insertStatement).ToList();

                    // Return if worker service has completed this task successfully
                    connection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
