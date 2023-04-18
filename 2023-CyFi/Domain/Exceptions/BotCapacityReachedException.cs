using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Exceptions
{
    public class BotCapacityReachedException : Exception
    {
        public BotCapacityReachedException() 
        {
        }

        public BotCapacityReachedException(string message) : base(message)
        {
        }

        public BotCapacityReachedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
