namespace BotDiscordCrypto.Threading
{
    public struct PauseToken
    {
        readonly PauseTokenSource _source;

        public PauseToken(PauseTokenSource source) { _source = source; }

        public Task<bool> IsPaused() { return _source.IsPaused(); }

        public Task PauseIfRequestedAsync(CancellationToken token = default(CancellationToken))
        {
            return _source.PauseIfRequestedAsync(token);
        }
    }
}
