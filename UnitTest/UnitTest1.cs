using ArtistRecordCount.Interfaces;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace UnitTest
{
    public class MockClient : ClientHttp
    {
        public MockClient(string host, Dictionary<string, string> requestHeaders = null, string urlPath = null) : base(host, requestHeaders,  urlPath)
        {
        }

        public override Task<HttpResponse> MakeRequest(HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {

                HttpResponseMessage response = new HttpResponseMessage();
                response.Content = new StringContent("{'test': 'test_content'}", Encoding.UTF8, "application/json");
                response.StatusCode = HttpStatusCode.OK;

                cancellationToken.ThrowIfCancellationRequested();

                return new HttpResponse(response.StatusCode, response.Content, response.Headers);
            }, cancellationToken);
        }
    }
    [TestClass]
    public class TestClient
    {
        [TestMethod]
        public void TestInitialization()
        {
            var host = "http://api.test.com";
            Dictionary<String, String> requestHeaders = new Dictionary<String, String>();
            var version = "v3";
            var urlPath = "/test/url/path";
            var test_client = new MockClient(host: host, requestHeaders: requestHeaders, urlPath: urlPath);
            requestHeaders.Add("Authorization", "Bearer SG.XXXX");
            requestHeaders.Add("Content-Type", "application/json");
            requestHeaders.Add("X-TEST", "test");
            Assert.IsNotNull(test_client);
            Assert.AreEqual(host, test_client.Host);
            Assert.AreEqual(requestHeaders, test_client.RequestHeaders);

            Assert.AreEqual(urlPath, test_client.UrlPath);
        }
        [TestClass]
        public class WordCountTest1
        {
            [TestMethod]
            public void WordCountTest()
            {
                ICalculateWord CalculateWord = new CalculateWord();
                string testSring = "Hi how are you?";
                int expected = 4;
                int actual = CalculateWord.CalculateWord(testSring);
                Assert.AreEqual(expected, actual);
            }


            [TestMethod]
            public void WordCountEmptyStringTest()
            {
                ICalculateWord CalculateWord = new CalculateWord();
                string testSring = "";
                int expected = 0;
                int actual = CalculateWord.CalculateWord(testSring);
                Assert.AreEqual(expected, actual);
            }


            [TestMethod]
            public void WordAvergeCountTest()
            {
                ICalculateWord CalculateWord = new CalculateWord();
                List<int> testList = new List<int>(){5,6,8,10,9,8};
                double expected = 7.666666666666667;
                double actual = CalculateWord.CalculateAverage(testList);
                Assert.AreEqual(expected, actual);
            }
        }
    }
}