using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using s3783258_a1.model;
using Newtonsoft.Json;

namespace s3783258_a1.utilities
{
    class RestData
    {
        private List<Customer> customers;
        private List<Login> logins;
        HttpClient http;

        public RestData()
        {
            this.http = new HttpClient();
        }

        public List<Customer> GetCustomers()
        {
            string url = "http://coreteaching01.csit.rmit.edu.au/~e87149/wdt/services/customers/";
            string customerJson = http.GetStringAsync(url).Result;
            customers = JsonConvert.DeserializeObject<List<Customer>>(customerJson, new JsonSerializerSettings { DateFormatString = "dd/MM/yyyy hh:mm:ss tt" });
            return customers;
        }

        public List<Login> GetLogins()
        {
            string url = "http://coreteaching01.csit.rmit.edu.au/~e87149/wdt/services/logins/";
            string loginJson = http.GetStringAsync(url).Result;
            logins = JsonConvert.DeserializeObject<List<Login>>(loginJson, new JsonSerializerSettings { DateFormatString = "dd/MM/yyyy" });
            return logins;
        }
    }
}
