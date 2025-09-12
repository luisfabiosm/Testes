using Domain.Core.Common.ResultPattern;
using Domain.Core.Common.Serialization;
using Domain.Core.Common.Transaction;
using Domain.Core.Enum;
using Domain.Core.Models.JDPI;
using Domain.Core.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pix_pagador_testes.Domain.Core.Common.Transaction
{
    public record TestTransaction : BaseTransaction<BaseReturn<TestResponse>>
    {
        public TestTransaction()
        {
            
        }

      

        public override string getTransactionSerialization()
        {
            return "TestSerialization";
        }
    }

    public record TestResponse : BaseTransactionResponse
    {
    }

   
}
