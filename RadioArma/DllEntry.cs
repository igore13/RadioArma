using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using RGiesecke.DllExport;
using YoutubeExplode;
using YoutubeSearch;

namespace RadioArma
{
    public class DllEntry
    {
        #region API Arma
        private static DllEntry _instance;

        [DllExport("_RVExtension@12", CallingConvention = CallingConvention.Winapi)]
        public static void RVExtension(StringBuilder output, int outputSize, string function)
        {
            if (_instance == null)
                _instance = new DllEntry();
            function = function.Replace(@"\n", "\n");
            output.Append(_instance.Invoke(function));
        }

        [DllExport("_RVExtensionVersion@8", CallingConvention = CallingConvention.Winapi)]
        public static void RvExtensionVersion(StringBuilder output, int outputSize)
        {
            output.Append("Radio Arma v1.0");
        }
        #endregion

        #region Variables
        private Radio radio;
        private Audio audio;
        private Youtube youtube;
        private float volume = 50;
        private VolumeWaveProvider16 volumeProvider;
        private bool isPlayingRadio;
        private bool isPlayingYouTube;
        #endregion

        #region Config Web Radio
        string[][] French = {
           new string[] { "Skyrock", "http://icecast.skyrock.net/s/natio_mp3_128k" },
           new string[] { "NRJ", "http://cdn.nrjaudio.fm/audio1/fr/30001/mp3_128.mp3?origine=fluxradios" },
           new string[] { "Nostalgie", "http://cdn.nrjaudio.fm/audio1/fr/30601/mp3_128.mp3?origine=fluxradios" },
           new string[] { "CherieFM", "http://cdn.nrjaudio.fm/audio1/fr/30201/mp3_128.mp3?origine=fluxradios" },
           new string[] { "RireEtChansons", "http://cdn.nrjaudio.fm/audio1/fr/30401/mp3_128.mp3?origine=fluxradios" },
           new string[] { "FunRadio", "http://streaming.radio.funradio.fr/fun-1-48-192" },
           new string[] { "RadioFG", "http://radiofg.impek.com/fg" }
        };
        #endregion

        #region Invoke
        private string Invoke(string function)
        {
            var lines = function.Split('\n');
            var cmd = lines[0];
            string data0;
            string data1;
            string url;

            try
            {
                data0 = lines[1];
                data1 = lines[2];
            }
            catch
            {
                data0 = "";
                data1 = "";
            }
            var response = "Error on Invoke Function in Extension";

            switch (cmd)
            {
                case "RADIO_PLAY":
                    if (data0 == "") break;
                    if (data1 == "") break;

                    url = SelectRadio(data0, data1);

                    if (url == "") return "Error on Choice Radio";

                    Play(url);
                    response = "Done";
                    break;
                case "SOUND_VOLUME":
                    Volume(data0);
                    response = "Done";
                    break;
                case "YOUTUBE_SEARCH":
                    int querypages = Int32.Parse(data1);
                    string search = SearchYoutube(data0, querypages);

                    response = search;
                    break;
                case "YOUTUBE_PLAY":
                    PlayYoutube(data0);

                    response = "Done";
                    break;
                case "STOP":
                    if (isPlayingYouTube)
                    {
                        StopYoutube(false, "");
                    }
                    if (isPlayingRadio)
                    {
                        Stop(false, "");
                    }

                    response = "Done";
                    break;
            }

            return response;
        }
        #endregion

        #region Volume Sound
        private void Volume(string data)
        {
            try
            {
                volume = ParseInt(data, 0, 100);
            }
            catch
            {
                return;
            }

            if (!isPlayingRadio && !isPlayingYouTube) return;
            try
            {
                volumeProvider.Volume = volume / 100;
            }
            catch
            {
                // ignored
            }
        }

        public int ParseInt(string str, int min, int max)
        {
            int number;
            try
            {
                number = Convert.ToInt16(str);
                if (number > max || number < min)
                    throw new Exception("Volume out of range");
            }
            catch
            {
                throw;
            }
            return number;
        }
        #endregion

        #region Sound Radio
        private void Play(string data)
        {
            Task.Factory.StartNew(() =>
            {
                if (isPlayingRadio)
                {
                    Stop(true, data);
                    return;
                }
                if (isPlayingYouTube)
                {
                    StopYoutube(false, "");
                    return;
                }
                radio = new Radio(data);
                audio = new Audio();
                audio.onWaveProviderCreated += audio_onWaveProviderCreated;
                radio.OnStreamUpdate += radio_OnStreamUpdate;
                radio.Start();
                isPlayingRadio = true;
            });
        }

        void radio_OnStreamUpdate(object sender, StreamUpdateEventArgs e)
        {
            audio.OnStreamUpdate(sender, e);
        }

        IWaveProvider audio_onWaveProviderCreated(IWaveProvider provider)
        {
            volumeProvider = new VolumeWaveProvider16(provider) { Volume = volume / 100 };
            return volumeProvider;
        }

        private void Stop(bool returnPlay, string data)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    radio.Dispose();
                    audio.Dispose();
                }
                catch
                {
                    // ignored
                }

                isPlayingRadio = false;

                if (returnPlay) Play(data);
            });
        }
        #endregion

        #region Function Radio
        public string SelectRadio(string language, string radioChoice)
        {
            string url = "";
            string[][] LanguageChoice = { };
            bool languageFound = false;

            switch (language)
            {
                case "French":
                    LanguageChoice = French;
                    languageFound = true;
                    break;

                default:
                    break;
            }

            if (!languageFound) return url;

            foreach (string[] radioEach in LanguageChoice)
            {
                if (radioEach[0] == radioChoice)
                {
                    url = radioEach[1];
                    break;
                }
            };
            return url;
        }
        #endregion

        #region Function Youtube
        public void PlayYoutube(string id)
        {
            Task.Factory.StartNew(() =>
            {
                if (isPlayingYouTube)
                {
                    StopYoutube(true, id);
                    return;
                }
                if (isPlayingRadio)
                {
                    Stop(false, "");
                    return;
                }
                youtube = new Youtube(id);
                youtube.Play();

                isPlayingYouTube = true;
            });
        }

        public void StopYoutube(bool returnPlay, string id)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    youtube.Dispose();
                }
                catch
                {
                    // ignored
                }

                isPlayingYouTube = false;

                if (returnPlay) PlayYoutube(id);
            });
        }

        public string SearchYoutube(string querystring, int querypages)
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

            Console.WriteLine(searchParam);

            return searchParam;
        }
        #endregion
    }
}
