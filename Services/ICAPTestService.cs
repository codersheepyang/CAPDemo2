using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAPTest.Services
{
    public interface ICAPTestService
    {
        void ReceiveMessageFromKafkaByCAP(string message);
        void PublishMessageInService();
    }
}
