using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HiveProfitSwitcher
{
    internal class Program
    {
        static string hiveApiKey = "";
        static string farmId = "";

        static void Main(string[] args)
        {
            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["HiveOSApiKey"]) || String.IsNullOrEmpty(ConfigurationManager.AppSettings["HiveFarmId"]))
            {
                Console.WriteLine("Hive API Key and Hive Farm ID are required. Please check the config file.");
            }
            else
            {
                hiveApiKey = ConfigurationManager.AppSettings["HiveOSApiKey"];
                farmId = ConfigurationManager.AppSettings["HiveFarmId"];
                List<String> configuredRigs = new List<string>();
                var config = (ProfitSwitchingConfig)ConfigurationManager.GetSection("profitSwitching");
                if (config != null && config.Workers != null && config.Workers.Count > 0)
                {
                    foreach (WorkerElement item in config.Workers)
                    {
                        if (!configuredRigs.Contains(item.Name))
                        {
                            configuredRigs.Add(item.Name);
                        }
                    }
                    RestClient client = new RestClient("https://api2.hiveos.farm/api/v2");
                    RestRequest request = new RestRequest(String.Format("/farms/{0}/workers", farmId));
                    request.AddHeader("Authorization", "Bearer " + hiveApiKey);
                    var response = client.Get(request);
                    dynamic responseContent = JsonConvert.DeserializeObject(response.Content);
                    foreach (var item in responseContent.data)
                    {
                        string workerName = item?.name?.Value;
                        if (configuredRigs.Contains(workerName))
                        {
                            foreach (WorkerElement worker in config.Workers)
                            {
                                if (worker.Name == workerName)
                                {
                                    HandleRig(item, worker);
                                }
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No Workers configured. Please check the config file.");
                }  
            }
        }

        static string DetermineCurrentCoin(dynamic item, WorkerElement worker)
        {
            string workerName = item?.name?.Value?.ToString();
            string result = item?.flight_sheet?.name?.Value?.ToString().ToUpper().Replace(String.Format("{0}_", workerName.ToUpper().Replace(" ", "_")), String.Empty);
            var config = (ProfitSwitchingConfig)ConfigurationManager.GetSection("profitSwitching");
            if (config != null && config.Workers != null && config.Workers.Count > 0)
            {
                foreach (WorkerElement workerData in config.Workers)
                {
                    if (worker.Name == workerData.Name)
                    {
                        if (workerData.EnabledCoins != null && workerData.EnabledCoins.Count > 0)
                        {
                            foreach (ConfiguredCoinElement enabledCoin in workerData.EnabledCoins)
                            {
                                if (item?.flight_sheet?.name?.Value?.ToString().ToUpper() == enabledCoin.FlightSheetName.ToUpper())
                                {
                                    result = enabledCoin.CoinTicker;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        static void HandleRig(dynamic item, WorkerElement worker)
        {
            string currentFlighSheetId = item?.flight_sheet?.id?.Value?.ToString();
            string workerName = item?.name?.Value?.ToString();
            string currentCoin = DetermineCurrentCoin(item, worker);
            RestClient client = new RestClient(worker.WTMEndpoint);
            RestRequest request = new RestRequest("");
            var response = client.Get(request);
            dynamic whatToMineResponseContent = JsonConvert.DeserializeObject(response.Content);
            Dictionary<string, double> coins = new Dictionary<string, double>();
            Dictionary<String,String> configuredCoins = new Dictionary<string,String>();
            foreach (ConfiguredCoinElement enabledCoin in worker.EnabledCoins)
            {
                if (!configuredCoins.ContainsKey(enabledCoin.CoinTicker))
                {
                    configuredCoins.Add(enabledCoin.CoinTicker, enabledCoin.FlightSheetName);
                }
            }
            foreach (var coin in whatToMineResponseContent.coins)
            {
                if (configuredCoins.ContainsKey(coin?.First?.tag?.Value))
                {
                    coins.Add(coin?.First?.tag?.Value, Convert.ToDouble(coin?.First?.btc_revenue.Value));
                }
            }
            var currentCoinPrice = coins.Where(x => x.Key == currentCoin).FirstOrDefault().Value;
            var newCoinBestPrice = coins.Values.Max();
            var newTopCoinTicker = coins.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            if (newCoinBestPrice > (currentCoinPrice * .05))
            {
                string newFlightSheeId = currentFlighSheetId;
                newFlightSheeId = GetFlightsheetID(configuredCoins[newTopCoinTicker]);
                if (!newFlightSheeId.Equals(currentFlighSheetId))
                {
                    UpdateFlightSheetID(item?.id?.Value?.ToString(), newFlightSheeId);
                }
            }
        }

        static string GetFlightsheetID(string flightSheetName)
        {
            string result = "";
            RestClient client = new RestClient("https://api2.hiveos.farm/api/v2");
            RestRequest request = new RestRequest(String.Format("/farms/{0}/fs", farmId));
            request.AddHeader("Authorization", "Bearer " + hiveApiKey);
            var response = client.Get(request);
            dynamic responseContent = JsonConvert.DeserializeObject(response.Content);
            foreach (var item in responseContent.data)
            {
                var fsName = item.name;
                if (!String.IsNullOrEmpty(fsName?.Value) && fsName?.Value == flightSheetName)
                {
                    result = item?.id?.ToString();
                }
            }
            return result;
        }

        static void UpdateFlightSheetID(string workerId, string flightSheetId)
        {
            RestClient client = new RestClient("https://api2.hiveos.farm/api/v2");
            RestRequest request = new RestRequest(String.Format("/farms/{0}/workers/{1}", farmId, workerId));
            request.AddHeader("Authorization", "Bearer " + hiveApiKey);
            var requestBody = new WorkerPatchRequest() { fs_id = flightSheetId };
            request.AddJsonBody(requestBody);
            var response = client.Patch(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine("Flightsheet Updated");
            }
            else
            {
                Console.WriteLine("Flightsheet Failed to Update");
            }
        }
    }
}
