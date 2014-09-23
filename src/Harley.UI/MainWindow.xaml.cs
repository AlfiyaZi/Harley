namespace Harley.UI
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CefSharp;
    using Harley.UI.Owin;

    public partial class MainWindow
    {
        public MainWindow(string title = null, string address = null)
        {
            Title = title ?? Title;

            EmbeddedBrowser.Init();
            InitializeComponent();
            Browser.Address = address;
        }

        public MainWindow(string title, Func<IDictionary<string, object>, Task> appFunc)
        {
            Title = title ?? Title;
            EmbeddedBrowser.Init(new[]
            {
                new CefCustomScheme
                {
                    SchemeHandlerFactory = new OwinSchemeHandlerFactory(appFunc),
                    SchemeName = "http"
                }
            });
            InitializeComponent();
            Browser.Address = "http://localhost";
        }
    }
}
