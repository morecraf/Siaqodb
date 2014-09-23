using MyCouch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InitCouchNode
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (var client = new MyCouchServerClient(@"http://127.0.0.1:5984/"))
                {
                    client.Databases.PutAsync("sysusers").Wait();
                    client.Databases.PutAsync("syssubusers").Wait();
                    client.Databases.PutAsync("sysstat").Wait();
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
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.WriteLine("all ok");
            Console.ReadLine();
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
            return @"{""_id"":""9bbaae526db72073e5f23963d1008003"",""_rev"":""2-7a50560c0774e93b4b46c3ec3e762ef6"",""$doctype"":""subUser"",""rights"":{""cristi_test3"":""None""},""password"":""FRswjDioAT"",""admin"":""cristi""}";
        }

    }
}
