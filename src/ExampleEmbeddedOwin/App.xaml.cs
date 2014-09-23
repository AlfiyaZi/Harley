namespace ExampleEmbeddedOwin
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows;
    using Harley.UI;
    using Microsoft.Owin.Builder;
    using Owin;

    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var appBuilder = new AppBuilder();
            appBuilder.UseWelcomePage();
            Func<IDictionary<string, object>, Task> appFunc = appBuilder.Build();

            new MainWindow("Owin Embedded Application", appFunc).Show();
        }
    }
}