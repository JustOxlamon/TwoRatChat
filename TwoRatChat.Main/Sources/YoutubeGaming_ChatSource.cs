// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows.Threading;
using System.Xml.Linq;
using TwoRatChat.Controls;

namespace TwoRatChat.Main.Sources {

    // https://gaming.youtube.com/youtubei/v1/guide?alt=json&key=AIzaSyBkrqhJHeXPQPMF1qvaduxKBXxYjHdAvok	HTTP/2	POST	200	application/json		77,87 мс	XMLHttpRequest

    //  POST URL-адрес запроса: https://gaming.youtube.com/youtubei/v1/live_chat/get_live_chat?alt=json&key=AIzaSyBkrqhJHeXPQPMF1qvaduxKBXxYjHdAvok
    // {"context":{"client":{"clientName":"WEB_GAMING","clientVersion":"1.92","hl":"ru","gl":"RU","experimentIds":[],"theme":"GAMING"},"capabilities":{},"request":{"internalExperimentFlags":[{"key":"optimistically_create_transport_client","value":"true"},{"key":"live_chat_top_chat_window_length_sec","value":"5"},{"key":"channel_about_page_gadgets","value":"true"},{"key":"log_js_exceptions_fraction","value":"1"},{"key":"custom_emoji_main_app","value":"true"},{"key":"enable_gaming_new_logo","value":"true"},{"key":"live_chat_use_new_default_filter_mode","value":"true"},{"key":"enable_youtubei_innertube","value":"true"},{"key":"kevlar_enable_vetracker","value":"true"},{"key":"live_chat_flagging_reasons","value":"true"},{"key":"youtubei_for_web","value":"true"},{"key":"lact_local_listeners","value":"true"},{"key":"polymer_live_chat","value":"true"},{"key":"live_chat_inline_moderation","value":"true"},{"key":"custom_emoji_super_chat","value":"true"},{"key":"third_party_integration","value":"true"},{"key":"live_chat_message_sampling_method","value":""},{"key":"spaces_desktop","value":"true"},{"key":"polymer_page_data_load_refactoring","value":"true"},{"key":"enable_gaming_comments_sponsor_badge","value":"true"},{"key":"live_chat_busyness_sampling_steepness","value":"1"},{"key":"use_push_for_desktop_live_chat","value":"true"},{"key":"live_chat_replay_milliqps_threshold","value":"5000"},{"key":"log_window_onerror_fraction","value":"1"},{"key":"custom_emoji_desktop","value":"true"},{"key":"custom_emoji_creator","value":"true"},{"key":"remove_web_visibility_batching","value":"true"},{"key":"live_chat_busyness_sampling_center","value":"14"},{"key":"chat_smoothing_animations","value":"0"},{"key":"live_chat_unhide_on_channel","value":"true"},{"key":"live_chat_busyness_sampling_ceil","value":"4"},{"key":"live_chat_top_chat_entry_threshold","value":"0"},{"key":"interaction_logging_on_gel_web","value":"true"},{"key":"live_chat_busyness_sampling_floor","value":"2"},{"key":"live_chat_busyness_enabled","value":"true"},{"key":"live_chat_top_chat_split","value":"0.5"},{"key":"live_chat_message_sampling_rate","value":"1"},{"key":"live_fresca_v2","value":"true"},{"key":"custom_emoji_legacy","value":"true"},{"key":"debug_forced_promo_id","value":""}]}},"continuation":"0ofMyAN5GjRDaU1TSVFvWVZVTlVVVmxGYjNsc1VVaFpSMVpVTjBwdFRqUTFObTVSRWdVdmJHbDJaU0FCKNKF5vn6_dgCMAA4AEACShUIARAAGAAgADAAOgkI2Y-qxfr92AJQjIOp-vr92AJY8qrQrcn92AJoBHABggEECAQQAA%3D%3D","isInvalidationTimeoutRequest":"true"}

    public class YoutubeGaming_ChatSource: TwoRatChat.Model.ChatSource {
        /// <summary>
        /// Тут я его должен убрать из исходников, но в общем кто знает
        /// </summary>
        string YOUTUBE_APIKEY = "AIzaSyBkrqhJHeXPQPMF1qvaduxKBXxYjHdAvok";// AIzaSyDWmvsh7PCyrV614EnBsUxzAB7oZkb7iZQ";

        const string API_getViewerCount = "https://www.googleapis.com/youtube/v3/videos?part=liveStreamingDetails&id={0}&key={1}";
        const string API_getMessages = "https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId={0}&part=snippet%2CauthorDetails&key={1}";

        class YoutubeMessage {
            public DateTime publishedAt;
            public string displayMessage;
        }

        class YoutubeAuthor {
            public string displayName;
            public bool isVerified;
            public bool isChatOwner;
            public bool isChatSponsor;
            public bool isChatModerator;
            public string profileImageUrl;
        }

        class YoutubeMessageContainer {
            public string kind;
            public string etag;
            public string id;
            public YoutubeMessage snippet;
            public YoutubeAuthor authorDetails;

            [JsonIgnore]
            public int NeedToDelete;
        }

        class YoutubeLiveMessages {
            public string nextPageToken;
            public int pollingIntervalMillis;
            public YoutubeMessageContainer[] items;
        }

        string _liveChatId;
       // Timer _retriveTimer;
        //List<YoutubeMessageContainer> _cache;
        string _id;
        DateTime _last;
        bool _showComments = false;

        public override string Id { get { return "youtube"; } }

        public YoutubeGaming_ChatSource( Dispatcher dispatcher )
            : base(dispatcher) {
        }

        public override void UpdateViewerCount() {
        }

        private class MyWebClient : WebClient {
            public MyWebClient(): base() {
                this.Headers.Add( HttpRequestHeader.Referer, "https://gaming.youtube.com/watch?v=sct_XRIRQ8Q" );
            }

            protected override WebRequest GetWebRequest( Uri uri ) {
                WebRequest w = base.GetWebRequest(uri);
                w.Timeout = 1000;
                return w;
            }
        }

        //YoutubeMessageContainer getById( string id ) {
        //    for (int j = 0; j < _cache.Count; ++j)
        //        if (_cache[j].id == id)
        //            return _cache[j];
        //    return null;
        //}

        Uri[] getBages( YoutubeMessageContainer b ) {
            if( Properties.Settings.Default.youtube_ShowIcons ) {
                return new System.Uri[] { new Uri( b.authorDetails.profileImageUrl, UriKind.Absolute ) };
            }
            return new System.Uri[0];
        }

        private void readMessages( string activeLiveChatId ) {
            
        }

        //eWRei_9cEO8
        public override void Create( string streamerUri, string id ) {
            this.Label = this._id = id;
            this.Uri = this.SetKeywords( streamerUri, false );
            this.Tooltip = "youtube";

            if( !string.IsNullOrEmpty( Properties.Settings.Default.youTubeAPIKey ) )
                this.YOUTUBE_APIKEY = Properties.Settings.Default.youTubeAPIKey;

            _showComments = Properties.Settings.Default.Chat_LoadHistory;

            //_cache = new List<YoutubeMessageContainer>();
            _youtubeChannelId = "";
            _last = DateTime.Now.AddDays( -10 );
            _chatThread = new System.Threading.Thread( chatThreadFunc );
            _chatThread.Start();
        }

        bool _abortRequested = false;
        System.Threading.Thread _chatThread;

        dynamic api( string url ) {
            MyWebClient mwc = new MyWebClient();
            try {
                byte[] data = mwc.DownloadData( url );
                return Newtonsoft.Json.JsonConvert.DeserializeObject( Encoding.UTF8.GetString( data ) );
            } catch( Exception er ) {
                return null;
            }
        }

        string _youtubeChannelId;
        string _youtubeLiveVideoId;

        void chatThreadFunc() {
            bool needErrorSleep = false;
            Header = "Loading 0%";
            int sleepMs = 3000;

            while ( !_abortRequested ) {
                if ( string.IsNullOrEmpty( _youtubeChannelId ) ) {
                    dynamic channelInfo = api( string.Format( "https://www.googleapis.com/youtube/v3/channels?part=id&forUsername={0}&key={1}",
                        HttpUtility.UrlEncode( this.Uri ),
                        YOUTUBE_APIKEY ) );

                    if ( channelInfo == null ) {
                        needErrorSleep = true;
                        Header = "Net error";
                    } else
                    if ( channelInfo.items.Count == 0 ) {
                        needErrorSleep = true;
                        Header = "Not found";
                    } else {
                        needErrorSleep = false;
                        _youtubeChannelId = (string)channelInfo.items[0].id;
                        Header = "Loading...";
                    }

                    if ( string.IsNullOrEmpty( _youtubeChannelId ) ) {
                        _youtubeChannelId = this.Uri;
                    }
                }

                if ( !string.IsNullOrEmpty( _youtubeChannelId ) ) {

                    if ( string.IsNullOrEmpty( _youtubeLiveVideoId ) ) {
                        dynamic liveVideo = api( string.Format( "https://www.googleapis.com/youtube/v3/search?part=id&channelId={0}&eventType=live&type=video&key={1}&rnd={2}",
                            _youtubeChannelId,
                            YOUTUBE_APIKEY, DateTime.Now.ToBinary() ) );

                        if ( liveVideo == null ) {
                            Header = "No live video";
                            _liveChatId = "";
                        } else
                        if ( liveVideo.items.Count == 0 ) {
                            Header = "No live";
                            _liveChatId = "";
                            _youtubeChannelId = "";
                        } else {
                            Header = "Live";
                            _youtubeLiveVideoId = liveVideo.items[0].id.videoId;
                            this.Tooltip = _youtubeChannelId + ": " + _youtubeLiveVideoId;
                        }
                    }
                }

                if ( !string.IsNullOrEmpty( _youtubeLiveVideoId ) ) {
                    dynamic d = api( string.Format( API_getViewerCount,
                        _youtubeLiveVideoId,
                        YOUTUBE_APIKEY ) );
                    if ( d != null ) {
                        if ( d.items[0].liveStreamingDetails.concurrentViewers == null ) {
                            this.Header = "Offline?";
                            _youtubeLiveVideoId = "";
                            _liveChatId = "";
                        } else {
                            int h = (int)d.items[0].liveStreamingDetails.concurrentViewers;
                            this.ViewersCount = h;
                            this.Header = h.ToString();
                            _liveChatId = (string)d.items[0].liveStreamingDetails.activeLiveChatId;
                        }
                    } else {
                        this.Header = "Net?";
                        _youtubeLiveVideoId = "";
                        _liveChatId = "";
                    }
                }

                if( !string.IsNullOrEmpty( _liveChatId ) ) {
                    try {
                        // _chatLoader.Headers.Add( , "" );
                        MyWebClient _chatLoader = new MyWebClient();

                        byte[] data = _chatLoader.DownloadData( string.Format( API_getMessages, _liveChatId, YOUTUBE_APIKEY ) );

                        string x = Encoding.UTF8.GetString( data );

                        YoutubeLiveMessages d = Newtonsoft.Json.JsonConvert.DeserializeObject<YoutubeLiveMessages>( x );
                        Status = true;

                        sleepMs = d.pollingIntervalMillis;

                        List<YoutubeMessageContainer> NewMessage = new List<YoutubeMessageContainer>();
                        ////////////
                        foreach ( var m in from b in d.items
                                           orderby b.snippet.publishedAt
                                           select b ) {
                            if ( m.snippet.publishedAt > _last ) {
                                NewMessage.Add( m );
                                _last = m.snippet.publishedAt;
                            }
                        }

                        if ( _showComments ) {
                            if ( NewMessage.Count > 0 ) {
                                newMessagesArrived( from b in NewMessage
                                                    orderby b.snippet.publishedAt
                                                    select new TwoRatChat.Model.ChatMessage( getBages( b ) ) {
                                                        Date = DateTime.Now,
                                                        Name = b.authorDetails.displayName,
                                                        Text = HttpUtility.HtmlDecode( b.snippet.displayMessage ),
                                                        Source = this,
                                                        Id = _id,
                                                        ToMe = this.ContainKeywords( b.authorDetails.displayName ),

                                                        //Form = 0
                                                    } );
                            }

                        }

                        _showComments = true;

                    } catch( Exception er ) {
                        Status = false;
                        Header = "ERR??";
                        _liveChatId = "";
                        _youtubeLiveVideoId = "";
                    }

                }


                Thread.Sleep( sleepMs );
            }
        }

        public override void Destroy() {
            _abortRequested = true;
            _chatThread.Abort();
        }
    }
}
