using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Net.Http;
public class Program
{
    //Fake user agent
    private static readonly string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36";
    //Creates HttpClient
    private static readonly HttpClient httpClient = new HttpClient();

    public static async Task Main(string[] args)
    {
        // Sets a fake User-Agent header
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

        List<string> countyUrls = new List<string>
        {
            "https://www.joehaydenrealtor.com/madison-county-ky/",
            "https://www.joehaydenrealtor.com/clark-county-ky/",
            "https://www.joehaydenrealtor.com/fayette-county-ky/",
            "https://www.joehaydenrealtor.com/bourbon-county-ky/",
            "https://www.joehaydenrealtor.com/scott-county-ky/",
        };
            
        foreach (string url in countyUrls)
        {
            //Sends GET request and returns response body as string
            string html = await httpClient.GetStringAsync(url);

            //Creates new HTML doc
            HtmlDocument htmlDocument = new HtmlDocument();

            //Loads response body into HTML doc
            htmlDocument.LoadHtml(html);

            //Selects nodes by Tag from HTML doc and adds them to a node list (in this case all the URLs).
            HtmlNodeCollection allPageLinks = htmlDocument.DocumentNode.SelectNodes("//div[@data-url]");

            //Isolates actual tag Value
            List<string> homeListingLinks = allPageLinks.Select(link => link.Attributes["data-url"].Value).ToList();

            //Takes top 10 listing links and concatenates them to produce a usable link.
            List<string> topTenLinks = homeListingLinks.Take(10).Select(link => "https://www.joehaydenrealtor.com" + link).ToList();

            foreach (string link in topTenLinks)
            {
                
                httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);

                string address = await MakeRequest(httpClient, link);

                Console.WriteLine(address);

            }
        }

        httpClient.Dispose();
    }

    static async Task<string> MakeRequest(HttpClient client, string link)
    {
        
        HttpResponseMessage html2 = await client.GetAsync(link);

        string htmlContent = await html2.Content.ReadAsStringAsync();

        HtmlDocument htmlDocument2 = new HtmlDocument();

        htmlDocument2.LoadHtml(htmlContent);

        List<HtmlNode> listingInfoNodes = new List<HtmlNode>
        {
            htmlDocument2.DocumentNode.SelectSingleNode("//div[@class='address']"),
            htmlDocument2.DocumentNode.SelectSingleNode("//span[@class='si-ld-top__price']"),
            htmlDocument2.DocumentNode.SelectSingleNode("//div[@class='si-idx-disclaimer']"),
            htmlDocument2.DocumentNode.SelectSingleNode("//div[@class='si-ld-primary__info clearfix']"),
            htmlDocument2.DocumentNode.SelectSingleNode("//div[@class='si-ld-primary__info clearfix']"),
            htmlDocument2.DocumentNode.SelectSingleNode("//div[@class='si-ld-description js-listing-description']"),
        };

        List<string> listingInfoTrimmed = new List<string>();

        foreach (HtmlNode node in listingInfoNodes) 
        {
            listingInfoTrimmed.Add(node.InnerText.Trim());
        }

        string address = listingInfoTrimmed[0];
        string price = listingInfoTrimmed[1];
        string date = listingInfoTrimmed[2];
        string sqft= listingInfoTrimmed[3];
        string lot = listingInfoTrimmed[4];
        string description = listingInfoTrimmed[5];

        string targetWord = "Sq. Feet:";
        string pattern = $@"\b{Regex.Escape(targetWord)}\b(.+$)\b(.+?)\b(.+?)\b";

        string targetWord2 = "Lot Size:";
        string pattern2 = $@"\b{Regex.Escape(targetWord2)}\b(.+?)\b(.+?)\b(.+?)\b";

        Match match = Regex.Match(sqft, pattern);
        sqft = match.Groups[0].Value;

        Match match2 = Regex.Match(lot, pattern2);
        lot= match2.Groups[0].Value;

        string completeListing = (address + "\n" + price + "\n" +  sqft + "\n" + lot + "\n" + date + "\n\n"+ description + "\n\n");

        return completeListing;
    }
}