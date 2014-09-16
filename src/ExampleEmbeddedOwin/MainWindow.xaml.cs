namespace ExampleEmbeddedOwin
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using CefSharp;
    using Harley.UI;
    using Harley.UI.Owin;
    using Microsoft.Owin.Builder;
    using Owin;

    public partial class MainWindow
    {
        public MainWindow()
        {
            Title = "Embedded OWIN Http Application";

            var appBuilder = new AppBuilder();
            appBuilder.UseWelcomePage();
            Func<IDictionary<string, object>, Task> appFunc = appBuilder.Build();

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