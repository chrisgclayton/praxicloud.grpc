using Grpc.Core;
using praxicloud.grpc.samples.messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace praxicloud.grpc.sample.services
{
    public sealed class TestMessageService : PraxiCloudPipelineService.PraxiCloudPipelineServiceBase
    {
        public override async Task<TestResponse> Test(TestRequest request, ServerCallContext context)
        {
            var response = new TestResponse 
            { 
                Message = $"{request.Message}_tested"
            };

            return response;
        }
    }
}
