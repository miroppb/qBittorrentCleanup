using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace qBittorrentCleanup
{
    public class API
    {
        private string _url;
        private string _user;
        private string _pass;
        private string _cookie;

        HttpClient client = new HttpClient();

        public API(string BaseUrl, string user, string pass)
        {
            _url = BaseUrl;
            _user = user;
            _pass = pass;

            _cookie = Login("auth/login", new Dictionary<string, string>()
            {
                { "username", _user},
                { "password", _pass }
            }).Result.Split(';')[0].Split('=')[1];

            client.DefaultRequestHeaders.Add("Referer", _url);
            client.DefaultRequestHeaders.Add("Cookie", $"SID={_cookie}");
        }

        public async Task<string> Get(string apiURL, Dictionary<string, string>? values)
        {
            FormUrlEncodedContent? content = null;
            if (values != null)
                content = new FormUrlEncodedContent(values);

            HttpResponseMessage response = null;
            if (content != null)
                response = await client.GetAsync($"{_url}/api/v2/{apiURL}?" + content.ToString());
            else
                response = await client.GetAsync($"{_url}/api/v2/{apiURL}");

            //Read back the answer from server
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public async Task<string> Post(string apiURL, Dictionary<string, string>? values)
        {
            FormUrlEncodedContent? content = null;
            if (values != null)
                content = new FormUrlEncodedContent(values);

            //POST the object to the specified URI 
            var response = await client.PostAsync($"{_url}/api/v2/{apiURL}", content);

            //Read back the answer from server
            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

        public async Task<string> Login(string apiURL, Dictionary<string, string> values)
        {
            HttpClient client = new HttpClient();

            var content = new FormUrlEncodedContent(values);
            client.DefaultRequestHeaders.Add("Referer", _url);

            //POST the object to the specified URI 
            var response = await client.PostAsync($"{_url}/api/v2/{apiURL}", content);

            //Read back the answer from server
            HttpResponseHeaders headers = response.Headers;
            return headers.Where(x => x.Key == "Set-Cookie").Select(x => x.Value).FirstOrDefault()!.First();
        }
    }
}
