using Avalonia;
using Avalonia.Threading;
using System;
using System.Windows;

namespace Stylet.Avalonia
{
    /// <summary>
    /// Generalised dispatcher, which can post and send.
    /// Used by <see cref="Execute"/>.
    /// </summary>
    public interface IDispatcher
    {
        /// <summary>
        /// Execute asynchronously
        /// </summary>
        /// <param name="action">Action to execute</param>
        void Post(Action action);

        /// <summary>
        /// Execute synchronously
        /// </summary>
        /// <param name="action">Action to execute</param>
        void Send(Action action);

        /// <summary>
        /// Gets a value indicating whether the current thread is the thread being dispatched to
        /// </summary>
        bool IsCurrent { get; }
    }

    /// <summary>
    /// <see cref="IDispatcher"/> implementation which can dispatch using <see cref="Dispatcher"/>
    /// </summary>
    public class ApplicationDispatcher : IDispatcher
    {
        private readonly Dispatcher dispatcher;

        /// <summary>
        /// Initialises a new instance of the <see cref="ApplicationDispatcher"/> class with the given <see cref="Dispatcher"/>
        /// </summary>
        public ApplicationDispatcher()
        {
            this.dispatcher = Dispatcher.UIThread;
        }

        /// <inheritdoc/>
        public void Post(Action action)
        {
            this.dispatcher.Post(action); //.BeginInvoke(action);
        }

        /// <inheritdoc/>
        public void Send(Action action)
        {
            this.dispatcher.InvokeAsync(action);
        }

        /// <inheritdoc/>
        public bool IsCurrent
        {
            get { return this.dispatcher.CheckAccess(); }
        }
    }

    /// <summary>
    /// <see cref="IDispatcher"/> implementation whcih dispatches synchronously.
    /// Usually used for unit testing.
    /// </summary>
    public class SynchronousDispatcher : IDispatcher
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="SynchronousDispatcher"/>
        /// </summary>
        public static SynchronousDispatcher Instance { get; } = new SynchronousDispatcher();
        private SynchronousDispatcher() { }

        /// <inheritdoc/>
        public void Post(Action action)
        {
            action();
        }

        /// <inheritdoc/>
        public void Send(Action action)
        {
            action();
        }

        /// <inheritdoc/>
        public bool IsCurrent
        {
            get { return true; }
        }
    }
}
