using System.Net.Http;
using ApprovalTests.Reporters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampleWebApiProject;

namespace ApprovalTests.Asp.WebApi.Tests
{
    [TestClass]
    [UseReporter(typeof(DiffReporter))]
    public class RoutingTest
    {
        [TestMethod]
        public void VerifyRoutingTest()
        {
            WebApiApprovals.VerifyRouting(WebApiConfig.Register,
                new HttpRequestMessageList { 
                    { "api/Values", HttpMethod.Get },
                    {"api/Values/5", HttpMethod.Put},
                    { "api/alues/5", HttpMethod.Put },
                    { "DOESNOTEXIST", HttpMethod.Get }, 
                });
        }
    }
}
