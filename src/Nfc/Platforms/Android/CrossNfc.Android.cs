﻿using System;
using Android.App;
using Android.Content;
using Android.Nfc;

namespace Plugin.Nfc
{
    public partial class CrossNfc
    {
        private static Func<Activity> _activityResolver;

        public static Activity CurrentActivity => GetCurrentActivity();
        public static void SetCurrentActivityResolver(Func<Activity> activityResolver)
        {
            _activityResolver = activityResolver;
        }

        public static void OnNewIntent(Intent intent, string[] actionsToHandle = null)
        {
            (Current as NfcImplementation).CheckForNfcMessage(intent, actionsToHandle);
        }

        private static Activity GetCurrentActivity()
        {
            if (_activityResolver == null)
                throw new InvalidOperationException("Resolver for the current activity is not set. Call CrossNfc.SetCurrentActivityResolver somewhere in your startup code.");

            return _activityResolver();
        }

        
    }
}