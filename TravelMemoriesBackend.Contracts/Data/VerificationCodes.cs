using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelMemoriesBackend.Contracts.Data
{
    public class VerificationCodes
    {
        public string UserEmail { get; set; }

        public int OTP { get; set; }

        public DateTime IssuedAt { get; set; }
    }
}
