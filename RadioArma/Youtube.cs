using NAudio.Wave;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;

namespace RadioArma
{
    public class Youtube : IDisposable
    {
        private WaveOut player;
        public string id
        {
            get;
            private set;
        }

        public Youtube(string id)
        {
            this.id = id;
        }

        IntPtr _disposed = IntPtr.Zero;

        public async void Play()
        {
            var client = new YoutubeClient();
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);
            var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();

            MediaFoundationReader reader = new MediaFoundationReader(streamInfo.Url);
            player = new WaveOut();
            player.Init(reader);
            player.Play();
        }

        public void Dispose()
        {
            player.Stop();
            player.Dispose();
            if (Interlocked.Exchange(ref _disposed, (IntPtr)1) != IntPtr.Zero)
                return;
        }
    }
}
