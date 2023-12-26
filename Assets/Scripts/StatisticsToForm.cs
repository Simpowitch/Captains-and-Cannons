using System.IO;
using System.Collections.Specialized;
using System.Net;
using UnityEngine;

public static class StatisticsToForm
{
    const string STATISTICSHOOK = "https://discordapp.com/api/webhooks/708284052389625857/OdTrju9peZrsGraxU-Tvv7gr2WtFL0lnKCmOxbJfhxcO0LKhvqxLxvuhM6OFHxRw9KTr";
    const string FEEDBACKHOOK = "https://discordapp.com/api/webhooks/709770987633246208/pDRXg9O9SNa_Y1ieLsRxfjKw1_bmRW8XktAN5TA3uw4wMmdYTLsXqwp03QBanegyqUId";
    public static void SendMessage(string filePath)
    {
        if (Http.InternetAccess())
        {
            Http.UploadFile(STATISTICSHOOK, filePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
    public static void SendFeedback(string username, string content)
    {
        if (Http.InternetAccess())
        {
            Http.SendFeedback(FEEDBACKHOOK, new NameValueCollection() { { "username", username }, { "content", content } });
        }
    }
}
class Http
{
    public static byte[] UploadFile(string url, string filePath)
    {
        using (WebClient webClient = new WebClient())
        {
            return webClient.UploadFile(url, filePath);
        }
    }
    public static byte[] SendFeedback(string url, NameValueCollection pairs)
    {
        using (WebClient webClient = new WebClient())
        {
            return webClient.UploadValues(url, pairs);
        }
    }

    public static bool InternetAccess()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}
