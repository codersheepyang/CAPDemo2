using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAPTest.DTO
{
    //减少库存的DTO
    public class TransferItemDTO
    {
        //库存扣减数量
        public int TransferQty { get; set; }
        public string ItemCode { get; set; }
    }
}
