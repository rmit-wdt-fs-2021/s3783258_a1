using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace s3783258_a1.model
{
    class Transaction
    {
        public int TransactionID { get; set; }
        public char TransactionType { get; set; }
        public int AccountNumber { get; set; }
        public int DestinationAccountNumber { get; set; }
        public double Amount { get; set; }
        public string Comment { get; set; }
        public DateTime TransactionTimeUtc { get; set; }

    }
}
