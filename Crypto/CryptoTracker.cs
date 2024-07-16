using BotDiscordCrypto.Threading;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using OKX.API;
using OKX.API.Extensions;
using OKX.API.Market.Converters;
using OKX.API.Market.Enums;
using OKX.API.Market.Models;

namespace BotDiscordCrypto.Crypto
{
    public class CryptoTracker : ThreadController
    {
        public RestClientAPI PrivateClientAPI { get; set; }
        public RestClientAPI PublicClientAPI { get; private set; }

        private static CryptoTracker _instance;

        public static CryptoTracker Instance { get { return _instance ??= new CryptoTracker(); } }

        public CryptoTracker SetRestClientAPI(bool isDemoTrading, string keyAPI = "", string secretKey = "", string passPhrase = "")
        {
            if (isRunning) throw new Exception("Thread running ...");
            if (!string.IsNullOrEmpty(keyAPI))
                PrivateClientAPI = new RestClientAPI(keyAPI, secretKey, passPhrase, isDemoTrading);
            return this;
        }

        public CryptoTracker SetPublicClientAPI(bool isDemoTrading)
        {
            PublicClientAPI = new RestClientAPI(isDemoTrading);
            return this;
        }

        public async Task UpdatePrice(bool isDemoTrading, ISocketMessageChannel channel, string instrumentId, BarSizeCandlestick barSize)
        {
            if (PublicClientAPI == null)
                SetPublicClientAPI(isDemoTrading);

            StopTask();

            CancellationTokenSource sourceToken = new CancellationTokenSource();
            PauseTokenSource pauseToken = new PauseTokenSource();

            StartTask(async () =>
            {

                await channel.SendMessageAsync($"Started log {instrumentId} {JsonConvert.SerializeObject(barSize, new BarSizeCandlestickConverter(false))}");
                await Task.Delay(1000);

                try
                {
                    var candlesticks = await PublicClientAPI.Market.GetCandlesticksAsync(instrumentId, barSize, limit: 3);
                    var data = candlesticks.Data[1];
                    var dataPrev = candlesticks.Data[2];
                    long timeResponse = candlesticks.Data[0].Timestamp;
                    var embedCandlestick = await GetEmbedCandlestick(instrumentId, barSize, data, dataPrev);

                    await channel.SendMessageAsync(embed: embedCandlestick.Build());

                    while (!sourceToken.IsCancellationRequested)
                    {
                        try
                        {
                            candlesticks = await PublicClientAPI.Market.GetCandlesticksAsync(instrumentId, barSize, limit: 3);
                            data = candlesticks.Data[1];
                            dataPrev = candlesticks.Data[2];

                            if (timeResponse == data.Timestamp)
                            {
                                timeResponse = candlesticks.Data[0].Timestamp;
                                embedCandlestick = await GetEmbedCandlestick(instrumentId, barSize, data, dataPrev);

                                await channel.SendMessageAsync(embed: embedCandlestick.Build());
                            }
                            await Task.Delay(1000);
                        }
                        catch (Exception e)
                        {
                            await channel.SendMessageAsync($"ERROR UpdatePrice : {e.Message}");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    await channel.SendMessageAsync($"ERROR UpdatePrice : {ex.Message}");
                }

                
            }, sourceToken, pauseToken);
        }

        public async Task<EmbedBuilder> GetEmbedCandlestick(string instrumentId, BarSizeCandlestick barSize, CandlestickResponse data, CandlestickResponse dataPrev)
        {
            var currency = instrumentId.Split('-')[0];
            var baseCurrency = instrumentId.Split('-')[1];

            var inconCurrency = $"https://static.okx.com/cdn/oksupport/asset/currency/icon/{currency.ToLower()}.png";

            var author = new EmbedAuthorBuilder
            {
                IconUrl = inconCurrency,
                Name = $"{instrumentId}",
                Url = "https://okx.com/"
            };

            var change = data.Close - dataPrev.Close;
            var RateChange = Math.Round(change / dataPrev.Close * 100, 2);

            return new EmbedBuilder
            {
                Color = change > 0 ? Color.Green : Color.Red,
                Author = author,
                Title = $"Price of candlestick {JsonConvert.SerializeObject(barSize, new BarSizeCandlestickConverter(false))} on {data.Time.ToString("dd-MM-yyyy HH:mm:ss")}",
                ThumbnailUrl = inconCurrency,
                Fields =
                {
                    new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = "Open price",
                        Value = $"{data.Open} {baseCurrency}"
                    },
                    new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = "Close price",
                        Value = $"{data.Close} {baseCurrency}"
                    },
                    new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = "Change (%)",
                        Value = $"{RateChange.ToOKXFormattedNumber()} %"
                    },
                    new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = "Highest price",
                        Value = $"{data.High}  {baseCurrency}"
                    },
                    new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = "Lowest price",
                        Value = $"{data.Low}  {baseCurrency}"
                    },
                    new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = $"Change ({baseCurrency})",
                        Value = $"{change.ToOKXFormattedNumber()} {baseCurrency}"
                    },
                    new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = $"Trading volume ({baseCurrency})",
                        Value = $"{data.BaseVolume.ToOKXString()} {baseCurrency}"
                    },
                    new EmbedFieldBuilder
                    {
                        IsInline = true,
                        Name = $"Trading volume ({currency})",
                        Value = $"{data.TradingVolume.ToOKXString()} {currency}"
                    },
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Bot OKX [2024]",
                    IconUrl = "https://static.okx.com/cdn/assets/imgs/226/EB771F0EE8994DD5.png",
                },
                Timestamp = DateTimeOffset.Now
            };
        }
    }
}
