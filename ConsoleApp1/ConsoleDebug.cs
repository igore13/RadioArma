using NAudio.Wave;
using System;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using YoutubeSearch;

namespace ConsoleApp1
{
    class ConsoleDebug
    {
        static void SoundYoutube(string url)
        {
            Console.WriteLine(url);
            Task.Factory.StartNew(() =>
            {
                MediaFoundationReader reader = new MediaFoundationReader(url);
                WaveOut player = new WaveOut();
                player.Init(reader);
                player.Play();
            });
        }
        static void PlayYoutube(string url)
        {
            Task.Factory.StartNew(async () =>
            {
                string id = YoutubeClient.ParseVideoId(url);

                var client = new YoutubeClient();
                var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);
                var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();

                SoundYoutube(streamInfo.Url);
            });
        }

        static string SearchYoutube(string querystring, int querypages)
        {
            var items = new VideoSearch();

            string searchParam = "[";
            
            foreach (var item in items.SearchQuery(querystring, querypages))
            {
                string id = YoutubeClient.ParseVideoId(item.Url);
                string itemParam = string.Format("['{0}', '{1}', '{2}', '{3}']", item.Title, item.Duration, item.Author, id);
                searchParam = searchParam + itemParam + ",";
            }
            
            searchParam = searchParam + "[]]";

            return searchParam;
        }

        static void Main(string[] args)
        {
            //string url = SearchYoutube("salut",1);
            PlayYoutube("https://www.youtube.com/watch?v=zgBEVbDzuu4");
            System.Threading.Thread.Sleep(500000);

        }
    }
}
