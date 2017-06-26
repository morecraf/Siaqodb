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
            SiaqodbFactory.SetPath(Directory.GetCurrentDirectory());
            var db = SiaqodbFactory.GetInstance();

            Company company = new Company();
            company.Name = "MyCompany";
            Employee employee = new Employee();
            employee.FirstName = "John";
            employee.LastName = "Walter";
            employee.Age = 31;
            employee.HireDate = new DateTime(2008, 10, 12);
            employee.Employer = company;
            db.StoreObject(employee);

            db.DropAllTypes();
        }
    }
}
