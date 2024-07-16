namespace BotDiscordCrypto.Threading
{
    public class ThreadController
    {
        public delegate void ThreadAction();
        public Task _Task;
        public bool isRunning => src != null && !src.IsCancellationRequested;
        CancellationTokenSource src;
        PauseTokenSource pauseSource;
        public bool StopTask()
        {
            if (_Task == null)
                return true;
            try
            {
                if (src == null)
                    return true;
                src.Cancel();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void TaskWait(CancellationTokenSource TokenSrouce, PauseTokenSource PauseSource)
        {
            var ct = StartTask(async () =>
            {

                while (true)
                {
                    try
                    {
                        TokenSrouce.Token.ThrowIfCancellationRequested();
                    }
                    catch
                    {
                        return;
                    }
                    await PauseSource.Token.PauseIfRequestedAsync();

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }, TokenSrouce, PauseSource);
        }

        public bool StartTask(ThreadAction action, CancellationTokenSource TokenSrouce, PauseTokenSource PauseSource)
        {
            if (_Task != null)
            {
                StopTask();
                _Task = null;
            }
            try
            {
                src = TokenSrouce;
                pauseSource = PauseSource;

                if (TokenSrouce == null)
                {
                    _Task = Task.Run(() => { action(); });
                }
                else
                {
                    _Task = Task.Run(() => { action(); }, TokenSrouce.Token);
                }



                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> PauseTask()
        {
            if (_Task == null)
                return true;

            try
            {
                await pauseSource.PauseAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ResumeTask()
        {
            if (_Task == null)
                return true;

            try
            {
                await pauseSource.ResumeAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
