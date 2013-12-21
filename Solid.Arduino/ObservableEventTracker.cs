using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solid.Arduino
{
    internal abstract class ObservableEventTracker<Tsource, Ttracked> : IObservable<Ttracked>, IDisposable
    {
        #region Protected Fields

        protected readonly Tsource _trackingSource;
        protected readonly List<IObserver<Ttracked>> _observers = new List<IObserver<Ttracked>>();
        protected bool _isDisposed = false;

        #endregion

        #region Constructors

        internal ObservableEventTracker(Tsource trackingSource)
        {
            _trackingSource = trackingSource;
        }

        #endregion

        #region Public Methods

        public IDisposable Subscribe(IObserver<Ttracked> observer)
        {
            _observers.Add(observer);
            return this;
        }

        public virtual void Dispose()
        {
            if (!_isDisposed)
            {
                foreach (IObserver<Ttracked> observer in _observers)
                    observer.OnCompleted();

                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        #endregion

    }
}
