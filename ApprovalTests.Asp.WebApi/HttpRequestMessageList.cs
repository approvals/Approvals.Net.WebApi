using System.Collections.Generic;
using System.Net.Http;

namespace ApprovalTests.Asp.WebApi
{
    public class HttpRequestMessageList : List<HttpRequestMessage>
    {
        public void Add(string uri, HttpMethod httpMethod)
        {
            this.Add(new HttpRequestMessage(httpMethod, uri));
        }
    }
}