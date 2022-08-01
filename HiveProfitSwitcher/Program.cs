using Ionic.Zip;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace HiveProfitSwitcher
{
    internal class Program
    {
        static string hiveApiKey = "";
        static string farmId = "";
        static double coinThreshold = Convert.ToDouble(0.05);

        static void Main(string[] args)
        {
            new AppUpdater().HandleUpdate();

            if (String.IsNullOrEmpty(ConfigurationManager.AppSettings["HiveOSApiKey"]) || String.IsNullOrEmpty(ConfigurationManager.AppSettings["HiveFarmId"]))
            {
                Console.WriteLine("Hive API Key and Hive Farm ID are required. Please check the config file.");
            }
            else
            {
                hiveApiKey = ConfigurationManager.AppSettings["HiveOSApiKey"];
                farmId = ConfigurationManager.AppSettings["HiveFarmId"];
                List<String> configuredRigs = new List<string>();

                if (ConfigurationManager.AppSettings["CoinDifferenceThreshold"] != null)
                {
                    coinThreshold = Convert.ToDouble(ConfigurationManager.AppSettings["CoinDifferenceThreshold"]);
                }

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
            var powerPrice = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[cost]");
            var btcPrice = 0.01;
            var coinDeskApiUrl = ConfigurationManager.AppSettings["CoinDeskApi"] ?? "https://api.coindesk.com/v1/bpi/currentprice.json";
            RestClient btcRestClient = new RestClient(coinDeskApiUrl);
            RestRequest btcRestRequest = new RestRequest("");
            dynamic btcMarketData = JsonConvert.DeserializeObject(btcRestClient.Get(btcRestRequest).Content);
            btcPrice = Convert.ToDouble(btcMarketData?.bpi?.USD?.rate?.Value ?? 0.01);

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
                    var btcRevenue = coin?.First?.btc_revenue?.Value ?? "0.00";
                    var algorithm = coin?.First?.algorithm?.Value;
                    var powerConsumption = "0.00";
                    switch (algorithm.ToUpper())
                    {
                        case "AUTOLYKOS":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[al_p]");
                            break;
                        case "BEAMHASHIII":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[eqb_p]");
                            break;
                        case "CORTEX":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[cx_p]");
                            break;
                        case "CRYPTONIGHTFASTV2":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[cnf_p]");
                            break;
                        case "CRYPTONIGHTGPU":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[cng_p]");
                            break;
                        case "CRYPTONIGHTHAVEN":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[cnh_p]");
                            break;
                        case "CUCKAROO29S":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[cr29_p]");
                            break;
                        case "CUCKATOO31":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[ct31_p]");
                            break;
                        case "CUCKATOO32":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[ct32_p]");
                            break;
                        case "CUCKOOCYCLE":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[cc_p]");
                            break;
                        case "EQUIHASH (210,9)":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[eqa_p]");
                            break;
                        case "EQUIHASHZERO":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[eqz_p]");
                            break;
                        case "ETCHASH":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[e4g_p]");
                            break;
                        case "ETHASH":
                            if (coin?.First?.tag?.Value == "ETH" || coin?.First?.tag?.Value == "NICEHASH")
                            {
                                powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[eth_p]");
                            }
                            else
                            {
                                powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[e4g_p]");
                            }
                            break;
                        case "FIROPOW":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[fpw_p]");
                            break;
                        case "KAWPOW":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[kpw_p]");
                            break;
                        case "NEOSCRYPT":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[ns_p]");
                            break;
                        case "OCTOPUS":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[ops_p]");
                            break;
                        case "PROGPOW":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[ppw_p]");
                            break;
                        case "PROGPOWZ":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[ppw_p]");
                            break;
                        case "RANDOMX":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[rmx_p]");
                            break;
                        case "UBQHASH":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[e4g_p]");
                            break;
                        case "VERTHASH":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[vh_p]");
                            break;
                        case "X25X":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[x25x_p]");
                            break;
                        case "ZELHASH":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[zlh_p]");
                            break;
                        case "ZHASH":
                            powerConsumption = HttpUtility.ParseQueryString(new Uri(HttpUtility.UrlDecode(worker.WTMEndpoint)).Query).Get("factor[zh_p]");
                            break;
                        default:
                            break;
                    }
                    var dailyPowerCost = 24 * ((Convert.ToDouble(powerConsumption) / 1000) * Convert.ToDouble(powerPrice));
                    var dailyRevenue = Convert.ToDouble(btcRevenue) * Convert.ToDouble(btcPrice);
                    var dailyProfit = dailyRevenue - dailyPowerCost;
                    coins.Add(coin?.First?.tag?.Value, dailyProfit);
                }
            }
            var currentCoinPrice = coins.Where(x => x.Key == currentCoin).FirstOrDefault().Value;
            var newCoinBestPrice = coins.Values.Max();
            var newTopCoinTicker = coins.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            if (newCoinBestPrice > (currentCoinPrice + (currentCoinPrice * coinThreshold)))
            {
                string newFlightSheeId = currentFlighSheetId;
                newFlightSheeId = GetFlightsheetID(configuredCoins[newTopCoinTicker]);
                if (!newFlightSheeId.Equals(currentFlighSheetId))
                {
                    UpdateFlightSheetID(item?.id?.Value?.ToString(), newFlightSheeId, configuredCoins[newTopCoinTicker], newCoinBestPrice.ToString());
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

        static void UpdateFlightSheetID(string workerId, string flightSheetId, string flightSheeName, string profit)
        {
            RestClient client = new RestClient("https://api2.hiveos.farm/api/v2");
            RestRequest request = new RestRequest(String.Format("/farms/{0}/workers/{1}", farmId, workerId));
            request.AddHeader("Authorization", "Bearer " + hiveApiKey);
            var requestBody = new WorkerPatchRequest() { fs_id = flightSheetId };
            request.AddJsonBody(requestBody);
            var response = client.Patch(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine(String.Format("Flightsheet Updated to {0}. Estimated Current Profit: ${1}", flightSheeName, Math.Round(Convert.ToDouble(profit),2)));
            }
            else
            {
                Console.WriteLine("Flightsheet Failed to Update");
            }
        }
    }
}
