namespace Harley.UI
{
    using System.Collections.Generic;
    using System.Windows.Input;
    using CefSharp;
    using CefSharp.Wpf;

    public partial class EmbeddedBrowser
    {
        private readonly ChromiumWebBrowser _browser;

        public EmbeddedBrowser()
        {
            InitializeComponent();

            _browser = new ChromiumWebBrowser();
            DockPanel.Children.Add(_browser);
        }

        public string Address
        {
            get { return _browser.Address; }
            set { _browser.Address = value; }
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
                _browser.Reload();
            }
        }
    }
}