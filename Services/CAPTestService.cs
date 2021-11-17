using DotNetCore.CAP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAPTest.Services
{
    //实现ICapSubscribe，ICapSubscribe为一个标记接口，告诉CAP这里有订阅方法
    //使用CapSubscribe标记方法作为订阅者
    //将服务在容器中注入
    public class CAPTestService : ICAPTestService, ICapSubscribe
    {
        //CAP发布对象
        private readonly ICapPublisher _capBus;


        public CAPTestService(ICapPublisher capBus)
        {
            _capBus = capBus;
        }

        [CapSubscribe("testForCAPTestService")]
        public void ReceiveMessageFromKafkaByCAP(string message)
        {
            Console.WriteLine($"在CAPTestService类的ReceiveMessageFromKafkaByCAP方法接收了消息:{message}");
        }


        public void PublishMessageInService()
        {
            _capBus.Publish("testForCAPTestService", "发布者在服务层.");
        }
    }
}
