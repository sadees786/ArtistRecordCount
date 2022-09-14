using System.Dynamic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;

public class HttpResponse
{
    private HttpStatusCode statusCode;
    private HttpContent body;
    private HttpResponseHeaders headers;

    public HttpStatusCode StatusCode { get => statusCode; set => statusCode = value; }
    public HttpContent Body { get => body; set => body = value; }
    public HttpResponseHeaders Headers { get => headers; set => headers = value; }

    public HttpResponse(HttpStatusCode statusCode, HttpContent responseBody, HttpResponseHeaders responseHeaders)
    {
        StatusCode = statusCode;
        Body = responseBody;
        Headers = responseHeaders;
    }
}
public class ClientHttp : DynamicObject
{
    private string host;
    private Dictionary<string, string> requestHeaders;
    private string urlPath;
    private string mediaType;

    public string Host { get => host; set => host = value; }
    public Dictionary<string, string> RequestHeaders { get => requestHeaders; set => requestHeaders = value; }
    public string UrlPath { get => urlPath; set => urlPath = value; }
    public string MediaType { get => mediaType; set => mediaType = value; }

    public enum Methods
    {
        GET
    }
    public ClientHttp(string host, Dictionary<string, string>? requestHeaders = null, string? urlPath = null)
    {
        Host = host;
        if (requestHeaders != null)
        {
            AddRequestHeader(requestHeaders);
        }

        UrlPath = (urlPath != null) ? urlPath : null;

    }
    public void AddRequestHeader(Dictionary<string, string> requestHeaders)
    {
        RequestHeaders = (RequestHeaders != null)
                ? RequestHeaders.Union(requestHeaders).ToDictionary(pair => pair.Key, pair => pair.Value) : requestHeaders;
    }


    private string BuildUrl(string queryParams = null)
    {
        string endpoint = null;

        if (UrlPath != null)
        {
            endpoint = Host + UrlPath;
        }
        else
            endpoint = Host;

        if (queryParams != null)
        {
            var ds_query_params = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(queryParams);
            var query = HttpUtility.ParseQueryString(string.Empty);
            foreach (var pair in ds_query_params)
            {
                query[pair.Key] = pair.Value.ToString();
            }
            string queryString = query.ToString();
            endpoint = endpoint + "?" + queryString;
        }

        return endpoint;
    }

    private ClientHttp BuildClient(string name = null)
    {
        string endpoint;

        if (name != null)
        {
            endpoint = UrlPath + "/" + name;
        }
        else
        {
            endpoint = UrlPath;
        }

        UrlPath = null;
        return new ClientHttp(Host, RequestHeaders, endpoint);

    }


    public virtual AuthenticationHeaderValue AddAuthorization(KeyValuePair<string, string> header)
    {
        string[] split = header.Value.Split(new char[0]);
        return new AuthenticationHeaderValue(split[0], split[1]);
    }

    public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
        if (Enum.IsDefined(typeof(Methods), binder.Name.ToUpper()))
        {
            CancellationToken cancellationToken = CancellationToken.None;
            string queryParams = null;
            string requestBody = null;
            int i = 0;

            foreach (object obj in args)
            {
                string name = binder.CallInfo.ArgumentNames.Count > i ? binder.CallInfo.ArgumentNames[i] : null;
                if (name == "queryParams")
                {
                    queryParams = obj.ToString();
                }
                else if (name == "requestBody")
                {
                    requestBody = obj.ToString();
                }
                else if (name == "requestHeaders")
                {
                    AddRequestHeader((Dictionary<string, string>)obj);
                }
                else if (name == "cancellationToken")
                {
                    cancellationToken = (CancellationToken)obj;
                }
                i++;
            }
            result = RequestAsync(binder.Name.ToUpper(), requestBody: requestBody, queryParams: queryParams, cancellationToken: cancellationToken).ConfigureAwait(false);
            return true;
        }
        else
        {
            result = null;
            return false;
        }
    }
    
    public async virtual Task<HttpResponse> MakeRequest(HttpClient client, HttpRequestMessage request, CancellationToken cancellationToken = default(CancellationToken))
    {

        HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        return new HttpResponse(response.StatusCode, response.Content, response.Headers);
    }

    private async Task<HttpResponse> RequestAsync(string method, String requestBody = null, String queryParams = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        using (var client = new HttpClient())
        {
            try
            {
                client.BaseAddress = new Uri(Host);
                string endpoint = BuildUrl(queryParams);
                client.DefaultRequestHeaders.Accept.Clear();
                if (RequestHeaders != null)
                {
                    foreach (KeyValuePair<string, string> header in RequestHeaders)
                    {
                        if (header.Key == "Authorization")
                        {
                            client.DefaultRequestHeaders.Authorization = AddAuthorization(header);
                        }
                        else if (header.Key == "Content-Type")
                        {
                            MediaType = header.Value;
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType));
                        }
                        else
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }
                }

                StringContent content = null;
                if (requestBody != null)
                {
                    content = new StringContent(requestBody, Encoding.UTF8, MediaType);
                }

                
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = new HttpMethod(method),
                    RequestUri = new Uri(endpoint),
                    Content = content
                };
                return await MakeRequest(client, request, cancellationToken).ConfigureAwait(false);

            }
            catch (TaskCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                HttpResponseMessage response = new HttpResponseMessage();
                string message;
                message = (ex is HttpRequestException) ? ".NET HttpRequestException" : ".NET Exception";
                message += ", raw message: \n\n";
                response.Content = new StringContent(message + ex.Message);
                return new HttpResponse(response.StatusCode, response.Content, response.Headers);
            }
        }
    }
}
