using System.Text.Json.Serialization;
using System;

namespace Discord
{
    public abstract class Controllable : IDisposable
    {
        protected event EventHandler OnClientUpdated;

        private IRestClient _client;
        [JsonIgnore]
        public IRestClient Client
        {
            get { return _client; }
            set
            {
                _client = value;

                OnClientUpdated?.Invoke(this, new EventArgs());
            }
        }

        public void Dispose()
        {
            _client = null;
            GC.SuppressFinalize(this);
        }
    }
}
