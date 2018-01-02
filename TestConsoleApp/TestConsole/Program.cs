using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sqo;

namespace TestConsole
{
    public class Company
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int OID { get; set; }
    }
    public class Employee
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime HireDate { get; set; }
        public int Age { get; set; }
        public Company Employer { get; set; }
        public int OID { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            string root_path = @"D:\morecraf\temp\bjorn";
            var db_list = new List<Siaqodb>();
            if (!Directory.Exists(root_path))
            {
                Directory.CreateDirectory(root_path);
            }

            //-- Generate and open some database files
            for (int i = 0; i < 100; i++)
            {
                string db_dir = Path.Combine(root_path, i.ToString("0000"));
                Directory.CreateDirectory(db_dir);
                var d = new Siaqodb(db_dir, 1024 * 1024 * 50, 50);
                db_list.Add(d);
            }

            //Company company = new Company();
            //company.Name = "MyCompany";
            //Employee employee = new Employee();
            //employee.FirstName = "John";
            //employee.LastName = "Walter";
            //employee.Age = 31;
            //employee.HireDate = new DateTime(2008, 10, 12);
            //employee.Employer = company;
            //db.StoreObject(employee);

            //db.DropAllTypes();
        }
    }
}
