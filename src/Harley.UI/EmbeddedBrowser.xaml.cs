namespace Harley.UI
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using CefSharp;
    using CefSharp.Wpf;

    public partial class EmbeddedBrowser
    {
        private readonly WebView _webView;

        public EmbeddedBrowser()
        {
            InitializeComponent();

            _webView = new WebView();
            DockPanel.Children.Add(_webView);
        }

        public string Address
        {
            get { return _webView.Address; }
            set { _webView.Address = value; }
        }

        public static void Init(IEnumerable<CefCustomScheme> customSchemes = null)
        {
            var settings = new CefSettings
            {
                BrowserSubprocessPath = "CefSharp.BrowserSubprocess.exe",
            };
            if(customSchemes != null)
            {
                foreach(var scheme in customSchemes)
                {
                    settings.RegisterScheme(scheme);
                }
            }
            Cef.Initialize(settings);
        }

        private void EmbeddedOwinBrowser_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                _webView.Reload();
                return;
            }
            if (e.Key == Key.F12)
            {
                _webView.ShowDevTools();
            }
        }
    }
}