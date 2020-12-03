// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com
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
using System.Xml.Linq;

namespace TwoRatChat.Radio {
    class Player : IDisposable {
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
            public ShoutcastStream( string url ) {
                HttpWebResponse response;

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Headers.Clear();
                request.Headers.Add("Icy-MetaData", "1");
                request.KeepAlive = false;
                request.UserAgent = "VLC media player";
                request.ReadWriteTimeout = 2000;
                request.Timeout = 2000;

                response = (HttpWebResponse)request.GetResponse();

                metaInt = int.Parse(response.Headers["Icy-MetaInt"]);
                receivedBytes = 0;

                netStream = response.GetResponseStream();
                netStream.ReadTimeout = 5000;

                connected = true;
            }

            /// <summary>
            /// Parses the received Meta Info
            /// </summary>
            /// <param name="metaInfo"></param>
            private void ParseMetaInfo( byte[] metaInfo ) {
                string metaString = Encoding.UTF8.GetString(metaInfo);

                string newStreamTitle = Regex.Match(metaString, "(StreamTitle=')(.*)(';StreamUrl)").Groups[2].Value.Trim();
                if (!newStreamTitle.Equals(streamTitle)) {
                    streamTitle = newStreamTitle;
                    OnStreamTitleChanged();
                }
            }

            /// <summary>
            /// Fires the StreamTitleChanged event
            /// </summary>
            protected virtual void OnStreamTitleChanged() {
                if (StreamTitleChanged != null)
                    StreamTitleChanged(this, EventArgs.Empty);
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

            public override int Read( byte[] buffer, int offset, int count ) {
                try {
                    read = 0;
                    leftToRead = count;
                    thisOffset = offset;
                    bytesRead = 0;
                    bytesLeftToMeta = ((metaInt - receivedBytes) > count) ? count : (metaInt - receivedBytes);

                    while (bytesLeftToMeta > 0 && (read = netStream.Read(buffer, thisOffset, bytesLeftToMeta)) > 0) {
                        leftToRead -= read;
                        thisOffset += read;
                        bytesRead += read;
                        receivedBytes += read;
                        bytesLeftToMeta -= read;
                    }

                    // read metadata
                    if (receivedBytes == metaInt) {
                        readMetaData();
                    }

                    while (leftToRead > 0 && (read = netStream.Read(buffer, thisOffset, leftToRead)) > 0) {
                        leftToRead -= read;
                        thisOffset += read;
                        bytesRead += read;
                        receivedBytes += read;
                    }

                    return bytesRead;
                } catch (Exception) {
                    return -1;
                }
            }

            private void readMetaData() {
                metaLen = netStream.ReadByte();
                if (metaLen > 0) {
                    metaInfo = new byte[metaLen * 16];
                    int len = 0;
                    while ((len += netStream.Read(metaInfo, len, metaInfo.Length - len)) < metaInfo.Length) ;
                    ParseMetaInfo(metaInfo);
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
            public override long Seek( long offset, SeekOrigin origin ) {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Sets the length of the stream.
            /// This Method always throws a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <param name="value"></param>
            public override void SetLength( long value ) {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Writes data to the ShoutcastStream.
            /// This method is not currently supported and always throws a <see cref="NotSupportedException"/>.
            /// </summary>
            /// <param name="buffer"></param>
            /// <param name="offset"></param>
            /// <param name="count"></param>
            public override void Write( byte[] buffer, int offset, int count ) {
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
            public VbrAcmMp3FrameDecompressor( WaveFormat sourceFormat, WaveFormat destFormat ) {
                this.pcmFormat = destFormat;// 
                try {
                    conversionStream = new AcmStream(sourceFormat, pcmFormat);
                } catch (Exception) {
                    disposed = true;
                    GC.SuppressFinalize(this);
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
            public int DecompressFrame( Mp3Frame frame, byte[] dest, int destOffset ) {
                if (frame == null) {
                    throw new ArgumentNullException("frame", "You must provide a non-null Mp3Frame to decompress");
                }
                Array.Copy(frame.RawData, conversionStream.SourceBuffer, frame.FrameLength);
                int sourceBytesConverted = 0;
                int converted = conversionStream.Convert(frame.FrameLength, out sourceBytesConverted);
                if (sourceBytesConverted != frame.FrameLength) {
                    throw new InvalidOperationException(String.Format("Couldn't convert the whole MP3 frame (converted {0}/{1})",
                        sourceBytesConverted, frame.FrameLength));
                }
                Array.Copy(conversionStream.DestBuffer, 0, dest, destOffset, converted);
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
                if (!disposed) {
                    disposed = true;
                    if (conversionStream != null)
                        conversionStream.Dispose();
                    GC.SuppressFinalize(this);
                }
            }

            /// <summary>
            /// Finalizer ensuring that resources get released properly
            /// </summary>
            ~VbrAcmMp3FrameDecompressor() {
                System.Diagnostics.Debug.Assert(false, "AcmMp3FrameDecompressor Dispose was not called");
                Dispose();
            }
        }

        class StreamPlayer {
            enum StreamingPlaybackState {
                Stopped,
                Playing,
                Buffering
            }

            const string ApiUri = "http://ws.audioscrobbler.com/2.0/";
            const string ApiKey = "6284df76097653f7affc4a39c004a087";

            void Search( object Track ) {
                try {
                    XElement x = XElement.Load(ApiUri + "?method=track.search&track=" + Track + "&limit=1&api_key=" + ApiKey);
                    CurrentTrack = new TrackInfo(x);
                } catch {
                    CurrentTrack = null;
                }

                try {
                    if (CurrentTrack != null && string.IsNullOrEmpty(CurrentTrack.IconUri)) {
                        XElement x = XElement.Load(ApiUri + "?method=artist.getinfo&artist=" + CurrentTrack.Artist + "&limit=1&api_key=" + ApiKey);
                        foreach (var img in x.Element("artist").Elements("image"))
                            if (img.Attribute("size").Value == "medium") {
                                CurrentTrack.IconUri = img.Value;
                                break;
                            }
                    }
                } catch {
                }

                string trk = Track.ToString();

                if (CurrentTrack == null && !string.IsNullOrEmpty(trk)) {
                    CurrentTrack = new TrackInfo(trk);
                }

                if (OnNewTrack != null)
                    OnNewTrack(this, CurrentTrack);
            }

            public TrackInfo CurrentTrack { get; private set; }

            string currentTrack = null;
            volatile StreamingPlaybackState playbackState;
            BufferedWaveProvider bufferedWaveProvider;
            VolumeSampleProvider volumeProvider;
            float MasterVolume = 1.0f;
            float LocalVolume = 0.0f;

            public StreamPlayer() {

            }

            public void Play( string Uri ) {
                if (playbackState == StreamingPlaybackState.Stopped) {
                    playbackState = StreamingPlaybackState.Buffering;
                    this.bufferedWaveProvider = null;
                    ThreadPool.QueueUserWorkItem(new WaitCallback(StreamMP3_New), Uri);
                }
            }

            object waveOutLock = new object();

            public void Stop() {
                if (currentTrack != string.Empty) {
                    currentTrack = string.Empty;
                    CurrentTrack = null;
                    if (OnNewTrack != null)
                        OnNewTrack(this, null);
                }
                if (this.volumeProvider != null)
                    this.volumeProvider.Volume = 0.0f;
                this.playbackState = StreamingPlaybackState.Stopped;

            }

          

            private void StreamMP3_New( object state ) {
                Thread.CurrentThread.Name = state.ToString();
                string url = (string)state;
                byte[] buffer = new byte[16384 * 4];

                Dictionary<int, IMp3FrameDecompressor> Decompressors = new Dictionary<int, IMp3FrameDecompressor>();
                WaveFormat outputFormat = new WaveFormat(44100, 16, 2);

                this.bufferedWaveProvider = new BufferedWaveProvider(outputFormat);
                this.bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(2);

//                WaveToSampleProvider wav2sample = new WaveToSampleProvider(this.bufferedWaveProvider);

                ISampleProvider sampleProvider = new Pcm16BitToSampleProvider(this.bufferedWaveProvider);

                SampleAggregator sa = new SampleAggregator(128);
                sa.NotificationCount = 882;
                sa.PerformFFT = true;
                sa.FftCalculated += sa_FftCalculated;
                NotifyingSampleProvider notifyProvider = new NotifyingSampleProvider(sampleProvider);
                notifyProvider.Sample += ( a, b ) => sa.Add(b.Left);

                volumeProvider = new VolumeSampleProvider(notifyProvider);
                //volumeProvider = new SampleChannel(this.bufferedWaveProvider, true);
                volumeProvider.Volume = 0.0f;
                //volumeProvider.PreVolumeMeter += waveChannel_PreVolumeMeter;

                for (int j = 0; j < 5; ++j) {

                    try {
                        using (IWavePlayer waveOut = new WaveOut()) {

                            waveOut.PlaybackStopped += waveOut_PlaybackStopped;
                            waveOut.Init(volumeProvider);
                         
                            using (var readFullyStream = new ShoutcastStream(url)) {
                                waveOut.Play();
                                if (OnStartPlay != null)
                                    OnStartPlay(this);

                                do {
                                    if (bufferedWaveProvider != null && bufferedWaveProvider.BufferLength - bufferedWaveProvider.BufferedBytes < bufferedWaveProvider.WaveFormat.AverageBytesPerSecond / 4) {
                                        int x = 0;
                                        while (playbackState != StreamingPlaybackState.Stopped && x < 5) {
                                            x++;
                                            Thread.Sleep(50);
                                        }
                                    } else {
                                        Mp3Frame frame = Mp3Frame.LoadFromStream(readFullyStream, true);

                                        if (currentTrack != readFullyStream.StreamTitle) {
                                            currentTrack = readFullyStream.StreamTitle;
                                            if (!string.IsNullOrEmpty(currentTrack)) {
                                                ThreadPool.QueueUserWorkItem(Search, currentTrack);
                                            } else {
                                                CurrentTrack = null;
                                                if (OnNewTrack != null)
                                                    OnNewTrack(this, null);
                                            }
                                        }

                                        IMp3FrameDecompressor dec;
                                        if (!Decompressors.TryGetValue(frame.SampleRate, out dec)) {

                                            WaveFormat waveFormat = new Mp3WaveFormat(frame.SampleRate,
                                                  frame.ChannelMode == ChannelMode.Mono ? 1 : 2,
                                                  frame.FrameLength, frame.BitRate);

                                            var suggFromat = AcmStream.SuggestPcmFormat(waveFormat);

                                            dec = new VbrAcmMp3FrameDecompressor(waveFormat, outputFormat);
                                            Decompressors[frame.SampleRate] = dec;
                                        }


                                        int decompressed = dec.DecompressFrame(frame, buffer, 0);
                                        bufferedWaveProvider.AddSamples(buffer, 0, decompressed);
                                    }

                                } while (playbackState != StreamingPlaybackState.Stopped);

                                waveOut.Stop();
                            }
                        }

                        return;

                    } catch (Exception exe) {
                        int x = 0;
                        while (playbackState != StreamingPlaybackState.Stopped && x < 20) {
                            x++;
                            Thread.Sleep(50);
                        }
                        if (playbackState == StreamingPlaybackState.Stopped)
                            return;
                    } finally {

                        foreach (var dc in Decompressors)
                            dc.Value.Dispose();

                        Decompressors.Clear();
                    }
                }

                if (OnError != null)
                    OnError(this);
            }

            void sa_FftCalculated( object sender, FftEventArgs e ) {
                if (OnFftCalculated != null)
                    OnFftCalculated(sender, e);
            }

            void waveOut_PlaybackStopped( object sender, StoppedEventArgs e ) {
                if (OnStoped != null)
                    OnStoped(this);
            }

            public double Volume {
                get { return MasterVolume; }
                set {
                    MasterVolume = (float)value;
                    if (volumeProvider != null)
                        volumeProvider.Volume = MasterVolume * LocalVolume;
                }
            }

            public void VolumeDown() {
                ThreadPool.QueueUserWorkItem(new WaitCallback(( a ) => {
                    StreamPlayer ip = a as StreamPlayer;
                    for (float v = LocalVolume; v > 0.0f; v -= 0.01f) {
                        ip.volumeProvider.Volume = MasterVolume * v;
                        LocalVolume = v;
                        Thread.Sleep(15);
                    }
                    ip.Stop();
                }), this);
            }

            public void VolumeUp() {
                ThreadPool.QueueUserWorkItem(new WaitCallback(( a ) => {
                    StreamPlayer ip = a as StreamPlayer;
                    for (float v = 0; v < 1.0f; v += 0.01f) {
                        ip.volumeProvider.Volume = MasterVolume * v;
                        LocalVolume = v;
                        Thread.Sleep(15);
                    }
                }), this);
            }

            //public event Action<StreamPlayer, float> OnVolumePeak;
            public event Action<StreamPlayer> OnStoped;
            public event Action<StreamPlayer> OnStartPlay;
            public event Action<StreamPlayer> OnError;
            public event Action<StreamPlayer, TrackInfo> OnNewTrack;
            public event EventHandler<FftEventArgs> OnFftCalculated;

        }

        double MasterVolume = 1.0;
        StreamPlayer TempUpPlayer;
        StreamPlayer VolumeDownPlayer;
        StreamPlayer VolumeUpPlayer;
        Object LockObject = new object();

        public Player() {
        }

        public double Volume {
            get { return MasterVolume; }
            set {
                lock (LockObject) {
                    MasterVolume = value;
                    if (TempUpPlayer != null)
                        TempUpPlayer.Volume = value;
                    if (VolumeUpPlayer != null)
                        VolumeUpPlayer.Volume = value;
                    if (VolumeDownPlayer != null)
                        VolumeDownPlayer.Volume = value;
                }
            }
        }

        public bool IsPlaying { get; private set; }

        public bool PlayUri( string RadioUri ) {
            try {
                lock (LockObject) {
                    if (TempUpPlayer != null) {
                        lock (TempUpPlayer) {
                            TempUpPlayer.OnStartPlay -= TempUpPlayer_OnStartPlay;
                            TempUpPlayer.OnStoped -= TempUpPlayer_OnStoped;
                            TempUpPlayer.OnNewTrack -= TempUpPlayer_OnNewTrack;
                            TempUpPlayer.OnError -= TempUpPlayer_OnError;
                            TempUpPlayer.OnFftCalculated -= TempUpPlayer_OnFftCalculated;
                            TempUpPlayer.Stop();
                        }
                    }

                    TempUpPlayer = new StreamPlayer();
                    TempUpPlayer.OnStartPlay += TempUpPlayer_OnStartPlay;
                    TempUpPlayer.OnStoped += TempUpPlayer_OnStoped;
                    TempUpPlayer.OnNewTrack += TempUpPlayer_OnNewTrack;
                    TempUpPlayer.OnError += TempUpPlayer_OnError;
                    TempUpPlayer.OnFftCalculated += TempUpPlayer_OnFftCalculated;
                    TempUpPlayer.Volume = MasterVolume;
                    TempUpPlayer.Play(RadioUri);
                    IsPlaying = true;
                }
             
                return true;
            } catch (Exception e) {
               // Console.WriteLine(e.Message);
                return false;
            }
        }

        void TempUpPlayer_OnFftCalculated( object sender, FftEventArgs e ) {
            if (OnFFT != null)
                OnFFT(e.Result);
        }

        void TempUpPlayer_OnError( StreamPlayer obj ) {
            lock (LockObject) {
                if (VolumeDownPlayer == null) {
                    if (VolumeUpPlayer != null) {
                        VolumeDownPlayer = VolumeUpPlayer;
                        VolumeDownPlayer.VolumeDown();
                    }
                } else {
                    if (VolumeUpPlayer != null) {
                        VolumeUpPlayer.Stop();
                        //VolumeUpPlayer.Dispose();
                        VolumeUpPlayer = null;
                    }
                }

                VolumeUpPlayer = null;
                TempUpPlayer = null;
            }

            IsPlaying = false;

            if (OnError != null)
                OnError();

        }

        public void Stop() {
            lock (LockObject) {
                if (TempUpPlayer != null) {
                    lock (TempUpPlayer) {
                        TempUpPlayer.OnStartPlay -= TempUpPlayer_OnStartPlay;
                        TempUpPlayer.OnStoped -= TempUpPlayer_OnStoped;
                        TempUpPlayer.OnNewTrack -= TempUpPlayer_OnNewTrack;
                        TempUpPlayer.OnError -= TempUpPlayer_OnError;
                        TempUpPlayer.OnFftCalculated -= TempUpPlayer_OnFftCalculated;
                        TempUpPlayer.Stop();
                        TempUpPlayer = null;
                    }
                }

                if (VolumeUpPlayer != null) {
                    VolumeUpPlayer.VolumeDown();
                }

                IsPlaying = false;
            }
        }

        void TempUpPlayer_OnNewTrack( StreamPlayer arg1, TrackInfo arg2 ) {
            if (OnTrack != null)
                OnTrack(arg2);
        }

        void TempUpPlayer_OnStartPlay( StreamPlayer obj ) {
            lock (LockObject) {
                if (VolumeDownPlayer == null) {
                    if (VolumeUpPlayer != null) {
                        VolumeDownPlayer = VolumeUpPlayer;
                        VolumeDownPlayer.VolumeDown();
                    }
                } else {
                    if (VolumeUpPlayer != null) {
                        VolumeUpPlayer.Stop();
                        //VolumeUpPlayer.Dispose();
                        VolumeUpPlayer = null;
                    }
                }

                VolumeUpPlayer = obj;
                VolumeUpPlayer.VolumeUp();
                TempUpPlayer = null;
            }

            if (OnStart != null)
                OnStart();
        }

        void TempUpPlayer_OnStoped( StreamPlayer obj ) {
            lock (LockObject) {
                if (VolumeDownPlayer == obj) {
                    VolumeDownPlayer = null;
                }

                if (VolumeUpPlayer == obj) {
                    VolumeUpPlayer = null;
                }
            }
        }

        public void Dispose() {
         
        }

        public Action OnError;
        public Action<TrackInfo> OnTrack;
        public Action OnStart;
        public Action<NAudio.Dsp.Complex[]> OnFFT;
    }
}
