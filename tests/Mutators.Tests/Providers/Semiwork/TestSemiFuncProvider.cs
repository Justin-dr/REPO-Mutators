using Mutators.Providers.Semiwork;

namespace Mutators.Tests.Providers.Semiwork
{
    public class TestSemiFuncProvider : ISemiFuncProvider
    {
        private readonly Queue<int> _moonLevelQueue = new Queue<int>();
        private readonly Queue<bool> _isMultiplayerQueue = new Queue<bool>();
        
        public int MoonLevel()
        {
            return _moonLevelQueue.Dequeue();
        }

        public bool IsMultiplayer()
        {
            return _isMultiplayerQueue.Dequeue();
        }
        
        public void QueueMoonLevel(int moonLevel)
        {
            _moonLevelQueue.Enqueue(moonLevel);
        }
        
        public void QueueIsMultiplayer(bool isMultiplayer)
        {
            _isMultiplayerQueue.Enqueue(isMultiplayer);
        }
    }
}