using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TwoRatChat {
    public sealed class StartArgs : EventArgs {
        /// <summary>
        /// Состояние потока до вызова метода Start()
        /// </summary>
        public bool WasStarted { get; private set; }

        public StartArgs( bool wasStarted ) {
            WasStarted = wasStarted;
        }
    }
    public sealed class StopArgs : EventArgs {
        /// <summary>
        /// Состояние потока до вызова метода Start()
        /// </summary>
        public bool WasStarted { get; private set; }

        /// <summary>
        /// Флаг прерывания потока исполнения по таймауту
        /// </summary>
        public bool ExitByTimeout { get; private set; }

        public StopArgs( bool wasStarted, bool exitByTimeout ) {
            WasStarted = wasStarted;
            ExitByTimeout = exitByTimeout;
        }
    }

    public abstract class DisposableTemplate : IDisposable {
        #region Implementation of IDisposable (with finalizer)

        ~DisposableTemplate() {
            Dispose(false);
        }
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose( bool disposing ) {
            if (!IsDisposed) {
                if (disposing)
                    DisposeManagedResources();

                DisposeUnmanagedResources();

                IsDisposed = true;
            }
        }

        #endregion

        #region For overrides

        protected abstract void DisposeManagedResources();
        protected virtual void DisposeUnmanagedResources() { }

        #endregion

        #region Property

        public bool IsDisposed { get; private set; }

        #endregion
    }

    public abstract class ThreadWorkerTemplate : DisposableTemplate {
        #region Private

        private int _threadFlag;
        private readonly Thread _thread;
        private void ThreadHandler() {
            // Здесь реализуем пользовательскую задержку, вместо Thread.Sleep(), 
            //      так как она позволит отреагировать на команду завершения работы
            // Используем захват переменной lastExecuted
            DateTime lastExecuted = DateTime.MinValue;
            Func<bool> isAllowed =
                () => {
                    DateTime now = DateTime.Now;

                    bool result = (MinimalIterationTimeout == -1 || (now - lastExecuted).TotalMilliseconds > MinimalIterationTimeout);

                    if (result)
                        lastExecuted = now;

                    // Допустим и 0, то тогда в диспетчере будет показываться 100% загрузка. При этом приложение будет отзываться
                    // 1 идентична интервалу в 10
                    if (MinimalIterationTimeout != -1)
                        Thread.Sleep(1);

                    return result;
                };

            // в цикле осуществляется вызов бизнес-функции 
            while (Interlocked.CompareExchange(ref _threadFlag, 0, 0) == 1) {
                if (isAllowed())
                    try {
                        WorkFunction();
                    } catch (Exception e) {
                        if (OnWorkFunctionException(e))
                            break;
                    }
            }
        }

        #endregion

        #region .ctor/dispose/finalize

        protected ThreadWorkerTemplate( int timeout ) : this(timeout, new TimeSpan(0, 0, 0, 10)) { }
        protected ThreadWorkerTemplate( int timeout, TimeSpan joinTimeout, ApartmentState apartmentState = ApartmentState.Unknown ) {
            MinimalIterationTimeout = timeout;
            JoinTimeout = joinTimeout;

            _threadFlag = 0;
            _thread = new Thread(ThreadHandler);
            _thread.SetApartmentState(apartmentState);
        }

        #endregion

        #region Invocators

        private void InvokeOnStart( StartArgs e ) {
            EventHandler<StartArgs> handler = OnStart;
            if (handler != null) handler(this, e);
        }

        private void InvokeOnStop( StopArgs e ) {
            EventHandler<StopArgs> handler = OnStop;
            if (handler != null) handler(this, e);
        }

        #endregion

        #region Events

        public event EventHandler<StartArgs> OnStart;
        public event EventHandler<StopArgs> OnStop;

        #endregion

        #region Management

        public void Start() {
            bool oldState = IsStarted;

            // стартуем с одновременным измененем флага
            if (Interlocked.CompareExchange(ref _threadFlag, 1, 0) == 0)
                _thread.Start();

            InvokeOnStart(new StartArgs(oldState));
        }

        public void Stop() {
            bool oldState = IsStarted;
            bool exitBytimeout = false;

            // стопаем с одновременным измененем флага
            if (Interlocked.CompareExchange( ref _threadFlag, 0, 1 ) == 1) {
                exitBytimeout = !_thread.Join( JoinTimeout );
                _thread.Abort();
            }

            InvokeOnStop(new StopArgs(oldState, exitBytimeout));
        }

        #endregion

        #region For overrite

        protected abstract void WorkFunction();

        protected virtual bool OnWorkFunctionException( Exception e ) {
            return true;
        }

        #endregion

        #region Properties

        public bool IsStarted { get { return Interlocked.CompareExchange(ref _threadFlag, 0, 0) == 1; } }
        public TimeSpan JoinTimeout { get; private set; }
        public int MinimalIterationTimeout { get; private set; }

        #endregion
    }
}
