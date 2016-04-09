using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Views.Animations;
using Java.Lang;
using Java.IO;
namespace ColorPattern.Screens
{
    [Activity(Label = "@string/ApplicationName", Theme = "@android:style/Theme.Black.NoTitleBar", MainLauncher = true, NoHistory = true)]
    public class SplashScreen : Activity, Android.Views.Animations.Animation.IAnimationListener
    {
        int SPLASH_TIME_OUT = 3000;
        ImageView logoRotate;
        TextView splashText;
        Animation animRotate;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            try
            {
                SetContentView(Resource.Layout.splashscreen_activity);
                logoRotate = FindViewById<ImageView>(Resource.Id.splash_logo);
                splashText = FindViewById<TextView>(Resource.Id.splashText);
                animRotate=AnimationUtils.LoadAnimation(this, Resource.Animation.splashlogo_rotate);
               
                splashText.StartAnimation(AnimationUtils.LoadAnimation(this, Resource.Animation.blink));
                logoRotate.StartAnimation(animRotate);
                animRotate.SetAnimationListener(this);

                //new Handler().PostDelayed(() =>
                //{

                //    StartActivity(typeof(MainActivity));
                //    Finish();

                //}, SPLASH_TIME_OUT);
            }
            catch (Java.Lang.Exception ex)
            {

            }
        }

        public void OnAnimationEnd(Animation animation)
        {
            StartActivity(typeof(ColorPatternActivity));
            Finish();
            
        }

        public void OnAnimationRepeat(Animation animation)
        {
          
        }

        public void OnAnimationStart(Animation animation)
        {
           
        }
    }
}