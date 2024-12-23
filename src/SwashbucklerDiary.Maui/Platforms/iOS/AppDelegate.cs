﻿using Foundation;
using SwashbucklerDiary.Maui.Essentials;
using UIKit;

namespace SwashbucklerDiary.Maui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
        {
            if (LaunchActivation.Activated is null)
            {
                LaunchActivation.OnLaunched(url);
            }
            else
            {
                LaunchActivation.OnActivated(url);
            }

            return true;
            //return base.OpenUrl(application, url, options);
        }
    }
}