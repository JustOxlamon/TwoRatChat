using NAudio.Wave;
using NAudio.Wave.Compression;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace TwoRatChat.Commands.Radio {
 
    class StreamMp3Player {
        class ShoutcastStream : Stream {
            private int metaInt;
            private int receivedBytes;
            private Stream netStream;
            private bool connected = false;
            private string streamTitle;

            private int read;
            private int leftToRead;
            private int thisOffset;
            private int bytesRead;
            private int bytesLeftToMeta;
            private int metaLen;
            private byte[] metaInfo;

            /// <summary>
            /// Is fired, when a new StreamTitle is received
            /// </summary>
            public event EventHandler StreamTitleChanged;

            /// <summary>
            /// Creates a new ShoutcastStream and connects to the specified Url
            /// </summary>
            /// <param name="url">Url of the Shoutcast stream</param>
            public ShoutcastStream(string url) {
                HttpWebResponse response;

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create( url );
                request.Headers.Clear();
                request.Headers.Add( "Icy-MetaData", "1" );
                request.KeepAlive = false;
                request.UserAgent = "VLC media player";
                request.ReadWriteTimeout = 2000;
                request.Timeout = 2000;

                response = (HttpWebResponse)request.GetResponse();

                metaInt = int.Parse( response.Headers["Icy-MetaInt"] );
                receivedBytes = 0;

                netStream = response.GetResponseStream();
                netStream.ReadTimeout = 5000;

                connected = true;
            }

            /// <summary>
            /// Parses the received Meta Info
            /// </summary>
            /// <param name="metaInfo"></param>
            private void ParseMetaInfo(byte[] metaInfo) {
                string metaString = Encoding.UTF8.GetString( metaInfo );

                string newStreamTitle = Regex.Match( metaString, "(StreamTitle=')(.*)(';StreamUrl)" ).Groups[2].Value.Trim();
                if ( !newStreamTitle.Equals( streamTitle ) ) {
                    streamTitle = newStreamTitle;
                    OnStreamTitleChanged();
                }
            }

            /// <summary>
            /// Fires the StreamTitleChanged event
            /// </summary>
            protected virtual void OnStreamTitleChanged() {
                if ( StreamTitleChanged != null )
                    StreamTitleChanged( this, EventArgs.Empty );
            }

            /// <summary>
            /// Gets a value that indicates whether the ShoutcastStream supports reading.
            /// </summary>
            public override bool CanRead {
                get { return connected; }
            }

            /// <summary>
            /// Gets a value that indicates whether the ShoutcastStream supports seeking.
            /// This property will always be false.
            /// </summary>
            public override bool CanSeek {
                get { return false; }
            }

            /// <summary>
            /// Gets a value that indicates whether the ShoutcastStream supports writing.
            /// This property will always be false.
            /// </summary>
            public override bool CanWrite {
                get { return false; }
            }

            /// <summary>
            /// Gets the title of the stream
            /// </summary>
            public string StreamTitle {
                get { return streamTitle; }
            }

            /// <summary>
            /// Flushes data from the stream.
            /// This method is currently not supported
            /// </summary>
            public override void Flush() {
                return;
            }

            /// <summary>
            /// Gets the length of the data available on the Stream.
            /// This property is not currently supported and always thows a <see cref="NotSupportedException"/>.
            /// </summary>
            public override long Length {
                get { throw new NotSupportedException(); }
            }

            /// <summary>
            /// Gets or sets the current position in the stream.
            /// This property is not currently supported and always thows a <see cref="NotSupportedException"/>.
            /// </summary>
            public override long Position {
                get {
                    throw new NotSupportedException();
                }
                set {
                    throw new NotSupportedException();
                }
            }

            /// <summary>
            /// Reads data from the ShoutcastStream.
            /// </summary>
            /// <param name="buffer">An array of bytes to store the received data from the ShoutcastStream.</param>
            /// <param name="offset">The location in the buffer to begin storing the data to.</param>
            /// <param name="count">The number of bytes to read from the ShoutcastStream.</param>
            /// <returns>The number of bytes read from the ShoutcastStream.</returns>
            //public override int Read( byte[] buffer, int offset, int count ) {
            //    try {
            //        if (receivedBytes == metaInt) {
            //            int metaLen = netStream.ReadByte();
            //            if (metaLen > 0) {
            //                byte[] metaInfo = new byte[metaLen * 16];
            //                int len = 0;
            //                while ((len += netStream.Read(metaInfo, len, metaInfo.Length - len)) < metaInfo.Length) ;
            //                ParseMetaInfo(metaInfo);
            //            }
            //            receivedBytes = 0;
            //        }

            //        int bytesLeft = ((metaInt - receivedBytes) > count) ? count : (metaInt - receivedBytes);
            //        int result = netStream.Read(buffer, offset, bytesLeft);
            //        receivedBytes += result;
            //        return result;
            //    } catch (Exception e) {
            //        connected = false;
            //        Console.WriteLine(e.Message);
            //        return -1;
            //    }
            //}

            public override int Read(byte[] buffer, int offset, int count) {
                try {
                    read = 0;
                    leftToRead = count;
                    thisOffset = offset;
                    bytesRead = 0;
                    bytesLeftToMeta = ((metaInt - receivedBytes) > count) ? count : (metaInt - receivedBytes);

                    while ( bytesLeftToMeta > 0 && (read = netStream.Read( buffer, thisOffset, bytesLeftToMeta )) > 0 ) {
                        leftToRead -= read;
                        thisOffset += read;
                        bytesRead += read;
                        receivedBytes += read;
                        bytesLeftToMeta -= read;
                    }

                    // read metadata
                    if ( receivedBytes == metaInt ) {
                        readMetaData();
                    }

                    while ( leftToRead > 0 && (read = netStream.Read( buffer, thisOffset, leftToRead )) > 0 ) {
                        leftToRead -= read;
                        thisOffset += read;
                        bytesRead += read;
                        receivedBytes += read;
                    }

                    return bytesRead;
                } catch ( Exception ) {
                    return -1;
                }
            }

            private void readMetaData() {
                metaLen = netStream.ReadByte();
                if ( metaLen > 0 ) {
                    metaInfo = new byte[metaLen * 16];
                    int len = 0;
                    while ( (len += netStream.Read( metaInfo, len, metaInfo.Length - len )) < metaInfo.Length ) ;
                    ParseMetaInfo( metaInfo );
                }
                receivedBytes = 0;
            }


            /// <summary>
            /// Closes the ShoutcastStream.
            /// </summary>
            public override void Close() {
                connected = false;
                netStream.Close();
            }

            /// <summary>
            /// Sets the current position of the stream to the given value.
            /// This Method is not currently supported and always throws a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <param name="offset"></param>
            /// <param name="origin"></param>
            /// <returns></returns>
            public override long Seek(long offset, SeekOrigin origin) {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Sets the length of the stream.
            /// This Method always throws a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <param name="value"></param>
            public override void SetLength(long value) {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Writes data to the ShoutcastStream.
            /// This method is not currently supported and always throws a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="offset"></param>
            /// <param name="count"></param>
            public override void Write(byte[] buffer, int offset, int count) {
                throw new NotSupportedException();
            }
        }

        class VbrAcmMp3FrameDecompressor : IMp3FrameDecompressor {
            private readonly AcmStream conversionStream;
            private readonly WaveFormat pcmFormat;
            private bool disposed;

            /// <summary>
            /// Creates a new ACM frame decompressor
            /// </summary>
            /// <param name="sourceFormat">The MP3 source format</param>
            public VbrAcmMp3FrameDecompressor(WaveFormat sourceFormat, WaveFormat destFormat) {
                this.pcmFormat = destFormat;// 
                try {
                    conversionStream = new AcmStream( sourceFormat, pcmFormat );
                } catch ( Exception ) {
                    disposed = true;
                    GC.SuppressFinalize( this );
                    throw;
                }
            }

            /// <summary>
            /// Output format (PCM)
            /// </summary>
            public WaveFormat OutputFormat { get { return pcmFormat; } }

            /// <summary>
            /// Decompresses a frame
            /// </summary>
            /// <param name="frame">The MP3 frame</param>
            /// <param name="dest">destination buffer</param>
            /// <param name="destOffset">Offset within destination buffer</param>
            /// <returns>Bytes written into destination buffer</returns>
            public int DecompressFrame(Mp3Frame frame, byte[] dest, int destOffset) {
                if ( frame == null ) {
                    throw new ArgumentNullException( "frame", "You must provide a non-null Mp3Frame to decompress" );
                }
                Array.Copy( frame.RawData, conversionStream.SourceBuffer, frame.FrameLength );
                int sourceBytesConverted = 0;
                int converted = conversionStream.Convert( frame.FrameLength, out sourceBytesConverted );
                if ( sourceBytesConverted != frame.FrameLength ) {
                    throw new InvalidOperationException( String.Format( "Couldn't convert the whole MP3 frame (converted {0}/{1})",
                        sourceBytesConverted, frame.FrameLength ) );
                }
                Array.Copy( conversionStream.DestBuffer, 0, dest, destOffset, converted );
                return converted;
            }

            /// <summary>
            /// Resets the MP3 Frame Decompressor after a reposition operation
            /// </summary>
            public void Reset() {
                conversionStream.Reposition();
            }

            /// <summary>
            /// Disposes of this MP3 frame decompressor
            /// </summary>
            public void Dispose() {
                if ( !disposed ) {
                    disposed = true;
                    if ( conversionStream != null )
                        conversionStream.Dispose();
                    GC.SuppressFinalize( this );
                }
            }

            /// <summary>
            /// Finalizer ensuring that resources get released properly
            /// </summary>
            ~VbrAcmMp3FrameDecompressor() {
                System.Diagnostics.Debug.Assert( false, "AcmMp3FrameDecompressor Dispose was not called" );
                Dispose();
            }
        }

        class StreamPlayer {
            enum StreamingPlaybackState {
                Stopped,
                Playing,
                Buffering,

                Deleted
            }

            public bool IsDeleted {
                get {
                    return playbackState == StreamingPlaybackState.Deleted;
                }
            }

            volatile StreamingPlaybackState playbackState;
            // BufferedWaveProvider bufferedWaveProvider;
            VolumeSampleProvider volumeProvider;
            float LocalVolume = 0.0f;

            StreamMp3Player _owner;

            public StreamPlayer(StreamMp3Player owner) {
                _owner = owner;
            }

            public void Play(Stream stream) {
                if ( playbackState == StreamingPlaybackState.Stopped ) {
                    playbackState = StreamingPlaybackState.Buffering;
                    ThreadPool.QueueUserWorkItem(
                        new WaitCallback( StreamMP3_New ),
                        stream );
                }
            }

            public void Stop() {
                if ( this.volumeProvider != null )
                    this.volumeProvider.Volume = 0.0f;
                this.playbackState = StreamingPlaybackState.Stopped;
            }

            private static IMp3FrameDecompressor createFrameDecompressor(Mp3Frame frame) {
                WaveFormat waveFormat = new Mp3WaveFormat(
                    frame.SampleRate,
                    frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                    frame.FrameLength,
                    frame.BitRate );
                return new AcmMp3FrameDecompressor( waveFormat );
            }

            private void StreamMP3_New(object state) {
                byte[] buffer = new byte[16384 * 4];

                var readFullyStream = state as System.IO.Stream;
                BufferedWaveProvider bwp = null;
                IMp3FrameDecompressor dec = null;

              //  var wh = new System.Threading.ManualResetEvent( false );
             //   wh.Reset();

                try {
                    using ( IWavePlayer waveOut = new WaveOut() ) {
                       // waveOut.PlaybackStopped += waveOut_PlaybackStopped;
                       // waveOut.PlaybackStopped += (a, b) => { wh.Set(); };
                        do {
                            Mp3Frame frame = Mp3Frame.LoadFromStream( readFullyStream, true );
                            if ( frame == null )
                                break;
                            if ( bwp == null ) {
                                // первый раз, создаем выходной буфер
                                dec = createFrameDecompressor( frame );
                                bwp = new BufferedWaveProvider( dec.OutputFormat );
                                bwp.BufferDuration = TimeSpan.FromSeconds( 3 );

                                volumeProvider = new VolumeSampleProvider( new Pcm16BitToSampleProvider( bwp ) );
                                volumeProvider.Volume = 0.0f;

                                waveOut.Init( volumeProvider );
                                waveOut.Play();
                                if ( OnStartPlay != null )
                                    OnStartPlay( this );
                            }



                            int decompressed = dec.DecompressFrame( frame, buffer, 0 );
                            bwp.AddSamples( buffer, 0, decompressed );

                            // Спим и ждем пока буффер просрется
                            if ( bwp.BufferLength - bwp.BufferedBytes < bwp.WaveFormat.AverageBytesPerSecond / 4 ) {
                                int x = 0;
                                while ( playbackState != StreamingPlaybackState.Stopped && x < 5 ) {
                                    x++;
                                    Thread.Sleep( 50 );
                                }
                            }
                        } while ( playbackState != StreamingPlaybackState.Stopped );
                        // 

                        waveOut_PlaybackStopped( null, null );

                        if ( playbackState != StreamingPlaybackState.Stopped ) {
                            while ( bwp.BufferedDuration.TotalSeconds > 0 ) {
                                Thread.Sleep( 50 );
                            }
                        }

                        waveOut.Stop();
                       // wh.WaitOne();

                      
                    }
                } catch ( Exception exe ) {
                    //Console.lo
                    if ( OnError != null )
                        OnError( this );
                } finally {
                    if ( dec != null )
                        dec.Dispose();

                    readFullyStream.Close();

                    playbackState = StreamingPlaybackState.Deleted;
                }
            }

            void waveOut_PlaybackStopped(object sender, StoppedEventArgs e) {
                if ( OnStoped != null )
                    OnStoped( this );
            }

            public void VolumeDownAndStop() {
                ThreadPool.QueueUserWorkItem( new WaitCallback( (a) => {
                    StreamPlayer ip = a as StreamPlayer;
                    for ( float v = LocalVolume; v > 0.0f; v -= 0.01f ) {
                        if ( ip.volumeProvider != null )
                            ip.volumeProvider.Volume = (float)CommandFactory.GlobalVolume * v;
                        LocalVolume = v;
                        Thread.Sleep( 15 );
                    }
                    ip.Stop();
                } ), this );
            }

            public void VolumeUp() {
                ThreadPool.QueueUserWorkItem( new WaitCallback( (a) => {
                    StreamPlayer ip = a as StreamPlayer;
                    for ( float v = 0; v < 1.0f; v += 0.01f ) {
                        if ( ip.volumeProvider != null )
                            ip.volumeProvider.Volume = (float)CommandFactory.GlobalVolume * v;
                        LocalVolume = v;
                        Thread.Sleep( 15 );
                    }
                } ), this );
            }

            //public event Action<StreamPlayer, float> OnVolumePeak;
            public event Action<StreamPlayer> OnStoped;
            public event Action<StreamPlayer> OnStartPlay;
            public event Action<StreamPlayer> OnError;

            internal void UpdateVolume() {
                if ( volumeProvider != null )
                    volumeProvider.Volume = (float)CommandFactory.GlobalVolume * LocalVolume;
            }
        }

        class PlayFileTask {
            public string NickName;
            public string Provider;
            public string Code;
        }


        Object _lockObject = new object();

        //class ReadFullyStream : Stream {
        //    private readonly Stream sourceStream;
        //    private long pos; // psuedo-position
        //    private readonly byte[] readAheadBuffer;
        //    private int readAheadLength;
        //    private int readAheadOffset;

        //    public ReadFullyStream(Stream sourceStream) {
        //        this.sourceStream = sourceStream;
        //        readAheadBuffer = new byte[4096];
        //    }
        //    public override bool CanRead {
        //        get { return true; }
        //    }

        //    public override bool CanSeek {
        //        get { return false; }
        //    }

        //    public override bool CanWrite {
        //        get { return false; }
        //    }

        //    public override void Flush() {
        //        throw new InvalidOperationException();
        //    }

        //    public override long Length {
        //        get { return pos; }
        //    }

        //    public override long Position {
        //        get {
        //            return pos;
        //        }
        //        set {
        //            throw new InvalidOperationException();
        //        }
        //    }


        //    public override int Read(byte[] buffer, int offset, int count) {
        //        int bytesRead = 0;
        //        while ( bytesRead < count ) {
        //            int readAheadAvailableBytes = readAheadLength - readAheadOffset;
        //            int bytesRequired = count - bytesRead;
        //            if ( readAheadAvailableBytes > 0 ) {
        //                int toCopy = Math.Min( readAheadAvailableBytes, bytesRequired );
        //                Array.Copy( readAheadBuffer, readAheadOffset, buffer, offset + bytesRead, toCopy );
        //                bytesRead += toCopy;
        //                readAheadOffset += toCopy;
        //            } else {
        //                readAheadOffset = 0;
        //                readAheadLength = sourceStream.Read( readAheadBuffer, 0, readAheadBuffer.Length );
        //                //Debug.WriteLine(String.Format("Read {0} bytes (requested {1})", readAheadLength, readAheadBuffer.Length));
        //                if ( readAheadLength == 0 ) {
        //                    break;
        //                }
        //            }
        //        }
        //        pos += bytesRead;
        //        return bytesRead;
        //    }

        //    public override long Seek(long offset, SeekOrigin origin) {
        //        throw new InvalidOperationException();
        //    }

        //    public override void SetLength(long value) {
        //        throw new InvalidOperationException();
        //    }

        //    public override void Write(byte[] buffer, int offset, int count) {
        //        throw new InvalidOperationException();
        //    }
        //}

        StreamPlayer _temp = null;
        StreamPlayer _oldPlayer = null;

        StreamPlayer _filePlayer = null;
        string _radioUrl;

        List<PlayFileTask> _tasks = new List<PlayFileTask>();

        public void UpdateVolume() {
            lock ( _lockObject ) {
                if ( _temp != null )
                    _temp.UpdateVolume();
                if ( _oldPlayer != null )
                    _oldPlayer.UpdateVolume();
                if ( _filePlayer != null )
                    _filePlayer.UpdateVolume();
            }
        }

        public StreamMp3Player() {
        }

        public void EnqueueYoutubeFile( string code ) {
            lock ( _tasks ) {
                foreach ( var pft in _tasks )
                    if ( pft.Code == code )
                        return;
                _tasks.Add( new PlayFileTask() {
                    Code = code,
                    NickName = "",
                    Provider = "youtube"
                } );
            }

            nextFile();
        }

        bool nextFile() {
            if ( _filePlayer == null ) {
                PlayFileTask next = null;
                lock ( _tasks ) {
                    if ( _tasks.Count > 0 ) {
                        next = _tasks[0];
                        _tasks.RemoveAt( 0 );
                    } else
                        return true;
                }
            }

            return false;
        }

        //void DowloadAndPlayYoutube(string code) {
        //    MemoryStream ms = new MemoryStream();
        //    VideoInfo audio;
        //    try {
        //        string link = "http://www.youtube.com/watch?v=" + code;
        //        IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls( link, false );

        //        audio = (from b in videoInfos
        //                           orderby b.AudioBitrate descending
        //                           where b.CanExtractAudio
        //                           select b).First();

        //        if ( audio.RequiresDecryption )
        //            DownloadUrlResolver.DecryptDownloadUrl( audio );

        //        var downloader = new MemoryAudioDownloader( audio, ms );
        //        downloader.Execute();
        //        // ms.Position = 0;

        //        ms = new MemoryStream( ms.ToArray() );
        //    } catch ( Exception erer) {
        //        _currentYoutubeThread = null;

        //        nextFile();
        //        return;
        //    }

        //    lock ( _lockObject ) {
        //        if ( _filePlayer != null ) {
        //            _filePlayer.VolumeDownAndStop();
        //            _filePlayer = null;
        //        }

        //        _filePlayer = new StreamPlayer( this );
        //        _filePlayer.OnError += _filePlayer_OnStoped;
        //        _filePlayer.OnStartPlay += _filePlayer_OnStartPlay;
        //        _filePlayer.OnStoped += _filePlayer_OnStoped;

        //        if ( OnTrack != null )
        //            OnTrack( audio.Title );
        //        _filePlayer.Play( ms );
        //    }
        //}

        public void CancelCurrentTrack() {
            if ( _filePlayer != null ) {
                _filePlayer.VolumeDownAndStop();
                _filePlayer = null;
            }
        }

        public void PlayFile(string filePath) {
            lock( _lockObject ) {
                if ( _filePlayer != null ) {
                    _filePlayer.VolumeDownAndStop();
                    _filePlayer = null;
                }

                _filePlayer = new StreamPlayer( this );
                _filePlayer.OnError += _filePlayer_OnStoped;
                _filePlayer.OnStartPlay += _filePlayer_OnStartPlay;
                _filePlayer.OnStoped += _filePlayer_OnStoped;

                _filePlayer.Play( new FileStream( filePath, FileMode.Open, FileAccess.Read ) );
            }
        }

        private void _filePlayer_OnStoped(StreamPlayer obj) {
            lock ( _lockObject ) {
                if ( _filePlayer != null ) {
                    _filePlayer.VolumeDownAndStop();
                    _filePlayer = null;
                }
                // check next mp3*

                if( nextFile() )
                    PlayRadio( _radioUrl );
            }
        }

        private void _filePlayer_OnStartPlay(StreamPlayer obj) {
            lock ( _lockObject ) {
                StopRadio();
                obj.VolumeUp();
            }
        }

        public void PlayRadio(string url) {
            if ( _oldPlayer != null ) {
                _oldPlayer.VolumeDownAndStop();
                _oldPlayer = null;
            }

            if ( !string.IsNullOrEmpty( url ) ) {
                _oldPlayer = _temp;

                _temp = new StreamPlayer( this );
                _temp.OnError += _temp_OnError;
                _temp.OnStartPlay += _temp_OnStartPlay;
                _temp.OnStoped += _temp_OnStoped;
                ShoutcastStream s = new ShoutcastStream( url );
                s.StreamTitleChanged += (a, b) => {
                    if ( OnTrack != null )
                        OnTrack( ((ShoutcastStream)a).StreamTitle );
                };
                _temp.Play( s );

                //OnTrack
            }
            _radioUrl = url;
        }

        public void StopRadio() {
            if ( _oldPlayer != null ) {
                _oldPlayer.VolumeDownAndStop();
                _oldPlayer = null;
            }

            if ( _temp != null ) {
                _temp.VolumeDownAndStop();
                _temp = null;
            }
        }

        private void StreamMp3Player_StreamTitleChanged(object sender, EventArgs e) {
        }

        private void _temp_OnStoped(StreamPlayer obj) {
        }

        private void _temp_OnStartPlay(StreamPlayer obj) {
            if ( _oldPlayer != null ) {
                _oldPlayer.VolumeDownAndStop();
                _oldPlayer = null;
            }

            obj.VolumeUp();
        }

        private void _temp_OnError(StreamPlayer obj) {
            _temp = null;
            PlayRadio( _radioUrl );
        }

        public Action<string> OnTrack;

        bool isDeleted( StreamPlayer sp) {
            if ( sp == null )
                return true;
            return sp.IsDeleted;
        }

        public void Destroy() {
            if( _temp != null ) {
                _temp.VolumeDownAndStop();
            }

            if ( _oldPlayer != null ) {
                _oldPlayer.VolumeDownAndStop();
            }

            if ( _filePlayer != null ) {
                _filePlayer.VolumeDownAndStop();
            }

            while ( !isDeleted( _temp ) || !isDeleted( _oldPlayer ) || !isDeleted( _filePlayer ) )
                Thread.Sleep( 100 );
        }
    }
}
