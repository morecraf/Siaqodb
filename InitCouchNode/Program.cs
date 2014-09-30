using MyCouch;
using MyCouch.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InitCouchNode
{
    class Program
    {
        static void Main(string[] args)
        {
            string logFile = args.Length == 2 ? args[1] : "";
                     
            try
            {

                if (args != null && args.Length > 0)
                {
                    if (args[0] == "init")
                    {
                        using (var client = new MyCouchServerClient(@"http://127.0.0.1:5984/"))
                        {
                            client.Databases.PutAsync("sysusers").Wait();
                            client.Databases.PutAsync("syssubusers").Wait();
                            client.Databases.PutAsync("sysstat").Wait();
                            client.Databases.PutAsync("sysnodes").Wait();
                        }
                        using (var client = new MyCouchClient(@"http://127.0.0.1:5984/sysusers"))
                        {
                            client.Documents.PostAsync(GetSysUsersViews()).Wait();
                            client.Documents.PostAsync(GetDefaultUser()).Wait();
                        }
                        using (var client = new MyCouchClient(@"http://127.0.0.1:5984/syssubusers"))
                        {
                            client.Documents.PostAsync(GetSysSubUsersViews()).Wait();
                            client.Documents.PostAsync(GetDefaultSubUser()).Wait();
                        }
                        using (var client = new MyCouchClient(@"http://127.0.0.1:5984/sysstat"))
                        {
                            client.Documents.PostAsync(GetSysStatViews()).Wait();
                        }
                    }


                    else if (args[0] == "repl")
                    {
                         
                        List<string> dbs = GetAllDatabases();
                        List<Node> nodes = GetNodes();
                        foreach (Node node in nodes)
                        {
                            foreach (Node sNode in nodes)
                            {
                                if (sNode._id == node._id)
                                {
                                    continue;
                                }
                                using (var client = new MyCouchServerClient("http://" + node.ip))
                                {
                                    foreach (string db in dbs)
                                    {
                                        try
                                        {
                                            client.Databases.PutAsync(db).Wait();

                                            string replicationId = node.name + "(" + node.ip + ")-" + db + "_push_to_" + sNode.name + "(" + sNode.ip + ")-" + db;
                                            
                                            var request = new ReplicateDatabaseRequest(replicationId, @"http://" + node.ip + "/" + db, @"http://" + sNode.ip + "/" + db);
                                            request.Continuous = true;

                                            var result = client.Replicator.ReplicateAsync(request).Result;
                                            if (result.Error != null)
                                            {
                                                Console.WriteLine("Error while start repl:" + result.Error);
                                                Logger.Log(DateTime.Now.ToString()+ " Error while start repl:" + result.Error, logFile);
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            Logger.Log(DateTime.Now.ToString()+ ex.ToString(), logFile);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(DateTime.Now.ToString()+ ex.ToString(), logFile);
            }
            Console.WriteLine("finished");
            
        }

        
        private static string GetSysUsersViews()
        {
            return @"{""_id"":""_design/views"",""language"":""javascript"",""views"":{""getuserbyemail"":{""map"":""function(doc) {\n emit(doc.email, doc);\n}""},""get_user_by_activation_hash"":{""map"":""function(doc) {\nemit(doc.activationHash, doc);\n}""},""get_user_by_reset_code"":{""map"":""function(doc) {\n emit(doc.resetCode, doc);\n}""}}}";
        }
        private static string GetSysSubUsersViews()
        {
            return @"{""_id"":""_design/views"",""language"":""javascript"",""views"":{""get_by_admin"":{""map"":""function(doc) {\n emit(doc.admin, doc);\n\n}""},""get_bucket_users_rights"":{""map"":""function(doc) {\n   var keys = function(obj){\n     var keys = [];\n     for(var key in obj){\n       keys.push(key);\n    }\n    return keys.length?keys:[\""-\""];\n  }(doc.rights);\n\n  keys.forEach(function(key) {\n    emit(doc.admin+\""_\""+key, {id:doc._id, right:doc.rights[key]});\n  });\n  \n}""}}}";
        }
        private static string GetSysStatViews()
        {
            return @"{""_id"":""_design/views"",""language"":""javascript"",""views"":{""get_log_by_admin"":{""map"":""function(doc) {\n   emit(doc.admin, doc);\n}""}}}";
        }
        private static string GetDefaultUser()
        {
            return @"{""_id"":""cristi"",""$doctype"":""user"",""userName"":""cristi"",""firstName"":""Cristi"",""lastName"":""Ursachi"",""company"":""Dotissi"",""activated"":true,""email"":""cristiursachi@siaqodb.com"",""passwordHash"":""ABdlyDBCBWuVXZm1otl7blwnBQbiCVXwHKy93Lr1RpcWzpVZm+2SZbU/oxOm+3uftg==""}";
        }
        private static string GetDefaultSubUser()
        {
            return @"{""_id"":""9bbaae526db72073e5f23963d1008003"",""$doctype"":""subUser"",""rights"":{""cristi_test3"":""None""},""password"":""FRswjDioAT"",""admin"":""cristi""}";
        }
        private static List<string> GetAllDatabases()
        {
            List<string> dbToReplicate = new List<string>();
            using (var client = new MyCouchServerClient(@"http://127.0.0.1:5984/"))
            {
                var request = new MyCouch.Net.HttpRequest(HttpMethod.Get, "_all_dbs");
                var response = client.Connection.SendAsync(request).Result;
                var dataBases = response.Content.ReadAsStringAsync().Result;
                string[] dbArray = JsonConvert.DeserializeObject<string[]>(dataBases);
                foreach (string db in dbArray)
                {
                    if (db.StartsWith("_"))
                        continue;
                    dbToReplicate.Add(db);
                }
            }
            return dbToReplicate;
        }
        private static List<Node> GetNodes()
        {
            List<Node> nodes = new List<Node>();
            using (var client = new MyCouchClient(@"http://127.0.0.1:5984/sysnodes"))
            {

                var query = new QueryViewRequest("_all_docs");
                query.Configure(q => q.IncludeDocs(true));
                var response = client.Views.QueryAsync(query).Result;
                if (response.Rows != null)
                {
                    foreach (var row in response.Rows)
                    {
                        Node co = client.Serializer.Deserialize<Node>(row.IncludedDoc);
                        if (co._id.StartsWith("_design/"))
                            continue;
                        nodes.Add(co);
                    }
                }

            }
            return nodes;
        }

    }
    public class Node
    {
        public string _id { get; set; }
        public string ip { get; set; }
        public string name { get; set; }

    }
}
