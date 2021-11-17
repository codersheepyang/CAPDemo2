using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAPTest.DTO
{
    //增加库存的DTO
    public class TransferItemResultDTO
    {
        //库存增加数量
        public int RestoreQty { get; set; }
        public string ItemCode { get; set; }
        public bool IsSuccess { get; set; }
    }
}
