using CAPTest.DTO;
using CAPTest.Memory_DB;
using CAPTest.Services;
using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAPTest.Controllers
{
    //控制层使用CAP发布订阅消息，注入ICapPublisher即可发布，使用CapSubscribe标记即可订阅
    [Route("api/[controller]")]
    [ApiController]
    public class CAPTestController : Controller
    {
        //CAP发布对象
        private readonly ICapPublisher _capBus;
        private readonly ICAPTestService _cAPTestService;

        
        private static readonly string SQL_SEVER_CONNECTION_STRING = "Data Source=localhost;Initial Catalog=Test;Integrated Security=SSPI;";

        //注入发布对象
        public CAPTestController(ICapPublisher capPublisher, ICAPTestService cAPTestService)
        {
            _capBus = capPublisher;
            _cAPTestService = cAPTestService;
        }

        //使用CapSubscribe标记方法作为MQ topic的订阅端，testForCAP为topic name
        [CapSubscribe("testForCAP")]
        public void ReceiveMessageFromKafkaByCAP(string message)
        {
            Console.WriteLine(message);
        }

        [HttpGet("PublishMessageToKafkaByCAP")]
        public IActionResult PublishMessageToKafkaByCAP()
        {
            //第一个参数是MQ队列名，第二个参数是消息内容，第三个参数是事务补偿方法
            //支持泛型消息，异步/同步推送
            _capBus.Publish("testForCAP", "订阅者在控制层.");
            return Ok();
        }

        [HttpGet("PublishMessageForTestSubscriberInService")]
        public IActionResult PublishMessageForTestSubscriberInService()
        {
            _capBus.Publish("testForCAPTestService", "订阅者在服务层.");
            return Ok();
        }

        [HttpGet("PublishMessageInService")]
        public IActionResult PublishMessageInService()
        {
            _cAPTestService.PublishMessageInService();
            return Ok();
        }

        //模拟分布式情况,接收到7仓的转仓请求，
        [CapSubscribe("TransferItem")]
        public TransferItemResultDTO ReceiveDecreaseInventoryRequest(TransferItemDTO decreaseInventory)
        {
            using (var connection = new SqlConnection(SQL_SEVER_CONNECTION_STRING))
            {
                //开启本地事务
                using (var transaction = connection.BeginTransaction(_capBus, true))
                {
                    if (Inventory.ItemCode.Equals(decreaseInventory.ItemCode))
                    {
                        //业务逻辑，8仓为item增加库存
                    }
                }
            }

            //由于某些原因，业务处理失败，需要还原7仓库存，将扣减的2个数量还原
            var transferItemResult = new TransferItemResultDTO()
            {
                ItemCode = decreaseInventory.ItemCode,
                RestoreQty = decreaseInventory.TransferQty,
                IsSuccess = false
            };
            return transferItemResult;
        }

        //事务补偿，当分布式情况下，某一端异常，回滚整个操作，保证原子性
        //模拟转仓操作，从7仓转移2个Item到8仓。那么此Item在7仓应该减少2个。
        [HttpGet("TestTransactionCompensation")]
        public IActionResult DecreaseInventory(string item)
        {
            using (var connection = new SqlConnection(SQL_SEVER_CONNECTION_STRING))
            {
                //开启本地事务
                using (var transaction = connection.BeginTransaction(_capBus, true))
                {
                    //假设Inventory已经是从DB取出的一个实体，Item001减少2个。
                    Inventory.InvetoryQty -= 2;

                    //构造请求
                    TransferItemDTO decreaseInventory = new TransferItemDTO()
                    {
                        ItemCode = "Item001",
                        TransferQty = 2
                    };
                    _capBus.Publish("TransferItem", decreaseInventory, "AddInventory");
                }
            }

            return Ok();
        }

        //事务补偿方法，当库存扣减整个动作，出现部分失败时，调用此方法还原库存
        [CapSubscribe("AddInventory")]
        public void AddInventory(TransferItemResultDTO request)
        {
            //IsSuccess = false，说明8仓接收转仓处理失败，7仓做回滚操作
            if (!request.IsSuccess)
            {
                using (var connection = new SqlConnection(SQL_SEVER_CONNECTION_STRING))
                {
                    //开启本地事务
                    using (var transaction = connection.BeginTransaction(_capBus, true))
                    {
                        if (request.ItemCode.Equals(Inventory.ItemCode))
                        {
                            Inventory.InvetoryQty += request.RestoreQty;
                        }
                    }
                }
            }
        }

        [HttpGet("ShowInventoryByItem/{itemCode}")]
        public IActionResult ShowInventoryByItem(string itemCode)
        {
            int inventoryQty = 0;
            if (itemCode.Equals(Inventory.ItemCode))
            {
                inventoryQty = Inventory.InvetoryQty;
            }

            return Ok(new
            {
                Item = itemCode,
                InventoryQty = inventoryQty
            });
        }
    }
}
　