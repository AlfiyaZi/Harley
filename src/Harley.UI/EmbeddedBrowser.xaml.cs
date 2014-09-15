namespace Harley.UI
{
    using System.Windows.Input;
    using CefSharp;
    using CefSharp.Wpf;

    public partial class EmbeddedBrowser
    {
        private readonly WebView _webView;

        public EmbeddedBrowser()
        {
            InitializeComponent();

            var browserSettings = new BrowserSettings
            {
                ApplicationCacheDisabled = true,
                PageCacheDisabled = true,
                DatabasesDisabled = true,
            };
            _webView = new WebView("about:blank", browserSettings);
            DockPanel.Children.Add(_webView);
        }

        public string Address
        {
            get { return _webView.Address; }
            set { _webView.Address = value; }
        }

        public static void Init()
        {
            var settings = new Settings();
            if(CEF.Initialize(settings))
            {
                // Plug in custom scheme handler here.
                //CEF.RegisterScheme("http", ...)
            }
        }

        private void EmbeddedOwinBrowser_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                _webView.Reload(true);
                return;
            }
            if (e.Key == Key.F12)
            {
                _webView.ShowDevTools();
            }
        }
    }
}