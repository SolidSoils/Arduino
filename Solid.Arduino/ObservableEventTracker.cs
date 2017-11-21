using System;
using System.Collections.Generic;

namespace Solid.Arduino
{
    internal abstract class ObservableEventTracker<TSource, TTracked> : IObservable<TTracked>, IDisposable
    {
        #region Protected Fields

        protected readonly TSource TrackingSource;
        protected readonly List<IObserver<TTracked>> Observers = new List<IObserver<TTracked>>();
        protected bool IsDisposed = false;

        #endregion

        #region Constructors

        internal ObservableEventTracker(TSource trackingSource)
        {
            TrackingSource = trackingSource;
        }

        #endregion

        #region Public Methods

        public IDisposable Subscribe(IObserver<TTracked> observer)
        {
            Observers.Add(observer);
            return this;
        }

        public virtual void Dispose()
        {
            if (!IsDisposed)
            {
                foreach (IObserver<TTracked> observer in Observers)
                    observer.OnCompleted();

                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

        #endregion

    }
}
