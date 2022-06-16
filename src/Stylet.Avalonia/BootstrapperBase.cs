﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Stylet.Avalonia.Xaml;
using System;
using System.Linq;

namespace Stylet.Avalonia
{
    /// <summary>
    /// Bootstrapper to be extended by applications which don't want to use StyletIoC as the IoC container.
    /// </summary>
    public abstract class BootstrapperBase : IBootstrapper, IWindowManagerConfig, IDisposable
    {
        /// <summary>
        /// Gets the current application
        /// </summary>
        public Application Application { get; private set; }

        /// <summary>
        /// Gets the command line arguments that were passed to the application from either the command prompt or the desktop.
        /// </summary>
        public string[] Args { get; private set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="BootstrapperBase"/> class
        /// </summary>
        protected BootstrapperBase()
        {
        }

        /// <summary>
        /// Called by the ApplicationLoader when this bootstrapper is loaded
        /// </summary>
        /// <remarks>
        /// If you're constructing the bootstrapper yourself, call this manully and pass in the Application
        /// (probably <see cref="Application.Current"/>). Stylet will start when <see cref="Application.Startup"/>
        /// is fired. If no Application is available, do not call this but instead call <see cref="Start(string[])"/>.
        /// (In this case, note that the <see cref="Execute"/> methods will all dispatch synchronously, unless you
        /// set <see cref="Execute.Dispatcher"/> yourself).
        /// </remarks>
        /// <param name="application">Application within which Stylet is running</param>
        public void Setup(Application application)
        {
            if (application == null)
                throw new ArgumentNullException("application");

            this.Application = application;
            // Use the current application's dispatcher for Execute
            Execute.Dispatcher = (IDispatcher)Dispatcher.UIThread;
            if(this.Application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.Startup += (o, e) => this.Start(e.Args);
                desktop.Exit += (o, e) =>
                {
                    this.OnExit(e);
                    this.Dispose();
                };
            }
            // Fetch this logger when needed. If we fetch it now, then no-one will have been given the change to enable the LogManager, and we'll get a NullLogger
            #region global exception
            // TODO: global exception
            //this.Application.DispatcherUnhandledException += (o, e) =>
            //{
            //    LogManager.GetLogger(typeof(BootstrapperBase)).Error(e.Exception, "Unhandled exception");
            //    this.OnUnhandledException(e);
            //};
            #endregion
        }

        /// <summary>
        /// Called on Application.Startup, this does everything necessary to start the application
        /// </summary>
        /// <remarks>
        /// If you're constructing the bootstrapper yourself, and aren't able to call <see cref="Setup(Application)"/>,
        /// (e.g. because an Application isn't available), you must call this yourself.
        /// </remarks>
        /// <param name="args">Command-line arguments used to start this executable</param>
        public virtual void Start(string[] args)
        {
            // Set this before anything else, so everything can use it
            this.Args = args;
            this.OnStart();

            this.ConfigureBootstrapper();

            // We allow starting without an application
            this.Application?.Resources.Add(View.ViewManagerResourceKey, this.GetInstance(typeof(IViewManager)));

            this.Configure();
            this.Launch();
            this.OnLaunch();
        }

        /// <summary>
        /// Hook called after the IoC container has been set up
        /// </summary>
        protected virtual void Configure() { }

        /// <summary>
        /// Called when the application is launched. Should display the root view using <see cref="DisplayRootView(object)"/>
        /// </summary>
        protected abstract void Launch();

        /// <summary>
        /// Launch the root view
        /// </summary>
        protected virtual void DisplayRootView(object rootViewModel)
        {
            var windowManager = (IWindowManager)this.GetInstance(typeof(IWindowManager));
            windowManager.ShowWindow(rootViewModel);
        }

        /// <summary>
        /// Returns the currently-displayed window, or null if there is none (or it can't be determined)
        /// </summary>
        /// <returns>The currently-displayed window, or null</returns>
        public virtual Window GetActiveWindow()
        {
            if (this.Application.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive) ?? desktop?.MainWindow;
            }
            return null;
        }

        /// <summary>
        /// Override to configure your IoC container, and anything else
        /// </summary>
        protected virtual void ConfigureBootstrapper() { }

        /// <summary>
        /// Given a type, use the IoC container to fetch an instance of it
        /// </summary>
        /// <param name="type">Type of instance to fetch</param>
        /// <returns>Fetched instance</returns>
        public abstract object GetInstance(Type type);

        /// <summary>
        /// Called on application startup. This occur after this.Args has been assigned, but before the IoC container has been configured
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// Called just after the root View has been displayed
        /// </summary>
        protected virtual void OnLaunch() { }

        /// <summary>
        /// Hook called on application exit
        /// </summary>
        /// <param name="e">The exit event data</param>
        protected virtual void OnExit(ControlledApplicationLifetimeExitEventArgs e) { }

        /// <summary>
        /// Hook called on an unhandled exception
        /// </summary>
        /// <param name="e">The event data</param>
        //protected virtual void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e) { }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose() { }
    }
}
