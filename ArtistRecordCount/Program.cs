using ArtistRecordCount.Model;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Collections.Specialized;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using ArtistRecordCount.Interfaces;

IConfiguration config = new ConfigurationBuilder()
.AddJsonFile("appsetting.json")
.AddEnvironmentVariables()
.Build();
AppConfig settings = config.GetRequiredSection("Settings").Get<AppConfig>();
Console.WriteLine("Please enter the name of the Artist");
string ArtistName = Console.ReadLine();
Execute(ArtistName.ToLower(), settings).Wait();

static async Task Execute(string name, AppConfig _settings)
{
    Dictionary<string, string> globalRequestHeaders = new Dictionary<string, string>();
    globalRequestHeaders.Add("Content-Type", "application/json");
    Dictionary<string, string> queryParams = new Dictionary<string, string>();
    //  queryParams.Add("format", "json");
    queryParams.Add(_settings.FilterParam, name);
    string hostname = _settings.HostUri + name;
    dynamic client = new ClientHttp(host: hostname, requestHeaders: globalRequestHeaders);

    dynamic response = await client.get();
    JObject songSearch = JObject.Parse(response.Body.ReadAsStringAsync().Result);

    trackDetails trackDetail = new trackDetails();

    IList<JToken> results = songSearch["message"]["body"]["track_list"].Children().ToList();

    if (results.Count == 0) { Console.WriteLine("No record find"); return; }
    List<int> WordCountList = new List<int>();
    ICalculateWord calculateWord = new CalculateWord();
    foreach (JToken resultsItem in results)
    {
        var trackname = resultsItem.SelectToken("track.track_name");
     //   Console.WriteLine(trackname.ToString());
        string host = _settings.LyricsUri + trackname.ToString() + "/" + name;

        dynamic clientLyrics = new ClientHttp(host: host, requestHeaders: globalRequestHeaders);
        try
        {
            dynamic responseLyrics = await clientLyrics.get();

            if (responseLyrics.Body.ReadAsStringAsync().Result.Contains("NotFound"))
            { }// to implement 
            else
            {
                Songs? songs = JsonSerializer.Deserialize<Songs>(responseLyrics.Body.ReadAsStringAsync().Result);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    int wordCount = calculateWord.CalculateWord(songs.lyrics);
                    WordCountList.Add(wordCount);
                    Console.WriteLine(wordCount);
                }
            }

        }
        catch (Exception ex)
        {
            //to implement
        }

    }
    Console.WriteLine($"The Average words in the {name}'s songs are " + calculateWord.CalculateAverage(WordCountList));
}
