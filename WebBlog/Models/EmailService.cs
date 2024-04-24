using RestSharp;
using RestSharp.Authenticators;

public class EmailService
{
    private string apiKey = "19806d14-2f1898b6";
    private string domain = "sandbox7ce17962ad5946f099fcc83962e64c2e.mailgun.org";

    public RestResponse SendSimpleMessage(string toEmail, string subject, string message)
    {
        var options = new RestClientOptions("https://api.mailgun.net/v3")
        {
            Authenticator = new HttpBasicAuthenticator("api", apiKey)
        };

        var client = new RestClient(options);

        var request = new RestRequest();
        request.AddParameter("domain", domain, ParameterType.UrlSegment);
        request.Resource = "{domain}/messages";

        request.AddParameter("from", "Excited User <mailgun@" + domain + ">");
        request.AddParameter("to", toEmail);
        request.AddParameter("subject", subject);
        request.AddParameter("text", message);

        request.Method = Method.Post;

        return client.Execute(request);
    }
}