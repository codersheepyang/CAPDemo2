using DotNetCore.CAP;
using DotNetCore.CAP.SqlServer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAPTest.Init
{
    public class SqlServerInitTable: SqlServerStorageInitializer
    {
        public SqlServerInitTable(ILogger<SqlServerStorageInitializer> logger, IOptions<SqlServerOptions> options) : base(logger, options)
        {

        }

        //重命名发布的消息存储表的名字
        public override string GetPublishedTableName()
        {
            return "CAPPublisedTable";
        }

        //重命名接受的消息存储表的名字
        public override string GetReceivedTableName()
        {
            return "CAPReceivedTable";
        }
    }
}
