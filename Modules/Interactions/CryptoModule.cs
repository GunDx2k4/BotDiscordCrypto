using BotDiscordCore.Core.Base;
using BotDiscordCrypto.Crypto;
using Discord.Interactions;
using Discord.WebSocket;
using OKX.API.Market.Enums;

namespace BotDiscordCrypto.Modules.Interactions
{
    public class CryptoModule : BaseInteractionModule
    {
        private DiscordSocketClient _client;

        public CryptoModule(DiscordSocketClient client) 
        {
            _client = client;
        }

        [SlashCommand("cryptolog","Start log price of candlestick crypto")]        
        public async Task HandleLogCryptoCommand(
            [Summary(description:"Get candlestick in demo trading")] bool isDemoTrading, 
            [Summary(description: "InstrumentId get candlestick (eg: BTC-USDT)")] string instrumentId,
            [Summary(name: "bar-size", description: "Bar size candlestick"),] BarSizeCandlestick barSize)
        {
            instrumentId = instrumentId.ToUpper();

            await RespondAsync($"Start log {instrumentId}");

            await CryptoTracker.Instance.UpdatePrice(isDemoTrading ,Context.Channel, instrumentId, barSize);
        }

        [SlashCommand("crypto", "Get price of candlestick crypto")]
        public async Task HandleCandlestickCryptoCommand(
            [Summary(description: "Get candlestick in demo trading")] bool isDemoTrading,
            [Summary(description: "InstrumentId get candlestick (eg: BTC-USDT)")] string instrumentId,
            [Summary(name: "bar-size",description: "Bar size candlestick"),] BarSizeCandlestick barSize)
        {
            instrumentId = instrumentId.ToUpper();

            var cryptoTracker = new CryptoTracker().SetPublicClientAPI(isDemoTrading);

            try
            {
                var candlesticks = await cryptoTracker.PublicClientAPI.Market.GetCandlesticksAsync(instrumentId, barSize, limit: 3);
                var data = candlesticks.Data[1];
                var dataPrev = candlesticks.Data[2];

                var baseCurrency = instrumentId.Split('-')[1];
                var currency = instrumentId.Split('-')[0];

                var embedCandlestick = await cryptoTracker.GetEmbedCandlestick(instrumentId, barSize, data, dataPrev);

                await RespondAsync(embed: embedCandlestick.Build());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            
        }
    }
}
