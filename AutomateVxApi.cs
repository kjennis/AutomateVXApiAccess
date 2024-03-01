using Crestron.SimplSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace AutomateVXApiAccess
{
    enum LogType
    {
        Error, warn, Notice
    }
    internal class ResponseResult
    {
        public string status { get; set; }
        public string token { get; set; }
        public string message { get; set; }
        public string err { get; set; }
    }
    public class AutomateVxApi
    {
        string _ip;
        int _port;
        string _username;
        string _password;
        bool _debug;

        public string token = "";

        public AutomateVxApi()
        {

        }

        public void Initialize(string ip, string username, string password, int port, int debug)
        {
            _ip = ip;
            _username = username;
            _password = password;
            _port = port;
            _debug = debug == 0 ? false: true;
        }

        internal void trace(LogType logType, string message)
        {
            if(_debug)
            {
                switch (logType)
                {
                    case LogType.Error:
                        ErrorLog.Error($"AutomateVX API: {message}");
                        break;
                    case LogType.warn:
                        ErrorLog.Warn($"AutomateVX API: {message}");
                        break;
                    case LogType.Notice:
                        ErrorLog.Notice($"AutomateVX API: {message}");
                        break;
                    default:
                        break;
                }
            }
        }

        public void SendCommand(string command)
        {
            if(token == "") 
            { 
                token = getToken();
            }
            if (token != "")
            {
                trace(LogType.Notice, $"sending command {command}");
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
                    var uri = $"http://{_ip}:{_port}/api/{command}";
                    var response = client.PostAsync(uri, null).GetAwaiter().GetResult();
                    var contentString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var result = JsonConvert.DeserializeObject<ResponseResult>(contentString);
                    if (result.status == "OK")
                    {
                        trace(LogType.Notice, $"{result.message}");
                    }
                    else
                    {
                        trace(LogType.Error, $"{result.err}");
                    }
                }
            }
        }

        string getToken()
        {
            trace(LogType.Notice, "Getting token");
            var token = "";
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var usernameAndPasswordBaseConversion = System.Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", usernameAndPasswordBaseConversion);
                var uri = $"http://{_ip}:{_port}/get-token";
                var response = client.PostAsync(uri, null).GetAwaiter().GetResult();
                var contentString = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var result = JsonConvert.DeserializeObject<ResponseResult>(contentString);
                if (result.status == "OK")
                {
                    trace(LogType.Notice, $"{result.token}");
                    token = result.token;
                }
                else
                {
                    trace(LogType.Error, $"{result.err}");
                }
            }
            return token;
        }
    }
}
