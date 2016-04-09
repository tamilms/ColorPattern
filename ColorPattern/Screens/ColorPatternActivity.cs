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
using ColorPattern.ServiceLayer;
using Android.Hardware;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.Net;
using Android.Views.Animations;

namespace ColorPattern.Screens
{
    [Activity(Label = "@string/ApplicationName", MainLauncher = false, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class ColorPatternActivity : Activity, Android.Hardware.ISensorEventListener,Animation.IAnimationListener
    {
        #region Declaration
        RelativeLayout SingleViewBacgroundview;
        SensorManager sensorMgr;
        bool hasUpdated = false;
        DateTime lastUpdate;
        float last_x = 0.0f;
        float last_y = 0.0f;
        float last_z = 0.0f;

        const int ShakeDetectionTimeLapse = 250;
        const double ShakeThreshold = 500;
        Random myRandomnumberGenerator;

        private long lastTouchTime = -1;
        WebServices webServer = new WebServices();
        Animation animRotate;
        #endregion

        #region Loading Screen and Lifecycle of Activity
       
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            try
            {
                // Set our view from the "main" layout resource
                SetContentView(Resource.Layout.colorpattern_activity);

                // initialize the SensorManager for dectecting 
                sensorMgr = (SensorManager)GetSystemService(SensorService);


                //random number generator for random imageshape creation
                myRandomnumberGenerator = new Random();

                SingleViewBacgroundview = FindViewById<RelativeLayout>(Resource.Id.SingleViewBacgroundview);




                SingleViewBacgroundview.SetOnTouchListener(new OnTouchListener(this));
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            /* Resigtering Accelerometer sensor listner for detecing shaking event on application appearing into 
            visible state or foreground of the activity */
            sensorMgr.RegisterListener(this, sensorMgr.GetDefaultSensor(SensorType.Accelerometer), SensorDelay.Ui);
        }

        protected override void OnPause()
        {
            base.OnPause();
            /* Un resigtering Accelerometer sensor listner when application goes to invisible state or background of the activity */
            sensorMgr.UnregisterListener(this);
        }

        #endregion


        #region Touch and Click event

        /// <summary>
        /// On Touch Generate new View
        /// </summary>
        public class OnTouchListener : Java.Lang.Object, View.IOnTouchListener
        {
            ColorPatternActivity _context;
            int xPosition, yPosition;
            private int _xDelta;
            private int _yDelta;

            public OnTouchListener(ColorPatternActivity context)
            {
                _context = context;
            }
            public bool OnTouch(View view, MotionEvent even)
            {
                try
                {
                    switch (even.Action)
                    {
                        case MotionEventActions.Down:
                            /* geting touching X & Y position of the layout*/
                            xPosition = (int)even.GetX();
                            yPosition = (int)even.GetY();

                            // Getting Relative layout parameters for adding a view
                            RelativeLayout.LayoutParams relativeLayoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.WrapContent,
                                    RelativeLayout.LayoutParams.WrapContent);

                            //create a new imageview to added into the relative layout
                            ImageView generateView = new ImageView(_context);

                            //Generate the Random number of creating a imageview shape based on MODULO value
                            if (_context.myRandomnumberGenerator.Next() % 2 == 0) 
                            {
                                /*if generated randumber which is EVEN number then Generate the new ImageView in Square Shape */
                                relativeLayoutParams.SetMargins(xPosition, yPosition, 0, 0);
                                generateView.LayoutParameters = relativeLayoutParams;
                                generateView.LayoutParameters.Width = 100;
                                generateView.LayoutParameters.Height = 100;
                                generateView.Tag = "S"; // set tag value as 'S' for Square
                                generateView.SetImageResource(Resource.Drawable.squareimg);
                                if (_context.isNetWorkAvailable())// check if network aviable is yes then apply get image from server and apply to imageview
                                    new LoadingImgeFromServerAsync(_context, generateView, "S", "ADD", ((ViewGroup)view)).Execute();
                                else
                                {
                                    generateView.Click += _context._imageView_Click;
                                    ((ViewGroup)view).AddView(generateView);
                                    _context.ApplyHexColor(generateView, "", "S","", false);
                                    _context.ApplyAnimationToView(generateView);
                                }
                            }
                            else
                            {
                                /*if generated randumber which is ODD number then Generate the new ImageView in Circle Shape */
                                relativeLayoutParams.SetMargins(xPosition, yPosition, 0, 0);
                                generateView.LayoutParameters = relativeLayoutParams;
                                generateView.LayoutParameters.Width = 100;
                                generateView.LayoutParameters.Height = 100;
                                generateView.Tag = "C"; // set tag value as 'C' for Circle
                                generateView.SetImageResource(Resource.Drawable.circleimg);

                                // check if network aviable is yes then apply hex color pattern from server
                                if (_context.isNetWorkAvailable()) 
                                    new LoadingImgeFromServerAsync(_context, generateView, "C", "ADD", ((ViewGroup)view)).Execute();
                                else
                                {
                                    generateView.Click += _context._imageView_Click;
                                    ((ViewGroup)view).AddView(generateView);
                                    _context.ApplyHexColor(generateView, "", "C","", false);
                                    _context.ApplyAnimationToView(generateView);
                                }
                            }

                            generateView.Touch += generateView_Touch;

                            break;


                        case MotionEventActions.Move:


                            break;
                        default:
                            return false;
                    }
                }
                catch (Exception ex) { }
                return true;
            }

            /// <summary>
            /// Used for selected imageView Dragging Event
            /// </summary>
            /// <param name="sender"></param>
            /// <param name="e"></param>
            void generateView_Touch(object sender, View.TouchEventArgs e)
            {
                try
                {
                    ImageView iv = (ImageView)sender;
                    int X = (int)e.Event.RawX;
                    int Y = (int)e.Event.RawY;
                    switch (e.Event.Action)
                    {
                        case MotionEventActions.Down:
                            /* Getting the current position of the imageview on the relative layout */
                            RelativeLayout.LayoutParams lParams = (RelativeLayout.LayoutParams)(((View)sender)).LayoutParameters;
                            _xDelta = X - lParams.LeftMargin;
                            _yDelta = Y - lParams.TopMargin;
                            break;
                        case MotionEventActions.Move:
                            /* Getting the position on draggin the imageview and set into Relative layout */
                            RelativeLayout.LayoutParams layoutParams = (RelativeLayout.LayoutParams)(((View)sender)).LayoutParameters;
                            layoutParams.LeftMargin = X - _xDelta;
                            layoutParams.TopMargin = Y - _yDelta;
                            layoutParams.RightMargin = -250;
                            layoutParams.BottomMargin = -250;
                            (((View)sender)).LayoutParameters = layoutParams;
                            break;
                        case MotionEventActions.Up:
                            /* To activate click event for imageview */
                            iv.PerformClick();
                            break;


                    }
                }
                catch (Exception ex) { }
            }
        }

        /// <summary>
        /// Selected imageview Double tab for update imageView by pattern according the rules mentioned in the documents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _imageView_Click(object sender, EventArgs e)
        {
            try
            {
                ImageView selectedImageview = (ImageView)sender;
                long thisTime = Java.Lang.JavaSystem.CurrentTimeMillis();
                //imageview double tab event get by checking click speed less than 250 milli seconds
                if (thisTime - lastTouchTime < 250)
                {
                    // less than 250 milli seconds so consider as double tab
                    if (selectedImageview.Tag + "" == "S")
                    {
                        if (isNetWorkAvailable()) //check if network is avilable if yes then call webservice call for applying image pattern
                        {
                            new LoadingImgeFromServerAsync(this, selectedImageview, "S", "UPDATE", null).Execute();
                        }
                        else
                        {
                            //check if network is not avilable if yes then applying random generated color
                            ApplyHexColor(selectedImageview, "", "S","", false);
                        }
                    }
                    else
                    {

                        if (isNetWorkAvailable()) //check if network is avilable if yes then call webservice call for applying image
                        {
                            new LoadingImgeFromServerAsync(this, selectedImageview, "C", "UPDATE", null).Execute();
                        }
                        else
                        {
                            //check if network is not avilable if yes then applying random generated color
                            ApplyHexColor(selectedImageview, "", "C","", false);
                        }
                    }

                    lastTouchTime = -1;
                }
                else
                {
                    // too slow single consider as tap
                    lastTouchTime = thisTime;
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Network Availablity check
        /// <summary>
        /// Checking Network Connection is avaiable or not?
        /// </summary>
        /// <returns></returns>
        public bool isNetWorkAvailable()
        {
            try
            {
                var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
                var activeConnection = connectivityManager.ActiveNetworkInfo;
                if ((activeConnection != null) && activeConnection.IsConnected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        #endregion

        #region Random color Generation and Apply Event

        /// <summary>
        /// Applying Color for ImageView
        /// </summary>
        /// <param name="v"></param>
        /// <param name="hexColor"></param>
        /// <param name="imgType"></param>
        /// <param name="isNewtWorkAvaiable"></param>
        /// <returns></returns>
        public Drawable ApplyHexColor(View view, String hexColor, String imgType,String imgTitle,bool isNewtWorkAvaiable)
        {
            /* To activate click event for imageview */

            ImageView seletedImage = (ImageView)view;
            Drawable background = seletedImage.Drawable;

            try
            {
                /*Check if network connection is available and getting hexa color from server then apply it*/
                if (isNewtWorkAvaiable == true && hexColor != "")
                {
                    ((GradientDrawable)background).SetColor(Color.ParseColor("#" + hexColor));
                    ((GradientDrawable)background).SetStroke(1, Color.ParseColor("#" + hexColor));
                    if (imgTitle != string.Empty)
                        Title = imgTitle;
                    else
                        Title = "Random Generated color"; 
                }
                else
                {
                    if (imgType == "S")
                    {
                        
                        seletedImage.SetImageResource(Resource.Drawable.squareimg);
                        background = seletedImage.Drawable;
                        ((GradientDrawable)background).SetColor(RandomColourGenerator());
                        if (imgTitle != string.Empty)
                            Title = imgTitle;
                        else
                            Title = "Random Generated color"; // applying random color
                    }
                    else
                    {
                        ((GradientDrawable)background).SetColor(RandomColourGenerator());
                        ((GradientDrawable)background).SetStroke(1, RandomColourGenerator());
                        if (imgTitle != string.Empty)
                            Title = imgTitle;
                        else
                            Title = "Random Generated color";
                    }

                }
                // applying animation for selected imageview 
                ApplyAnimationToView(seletedImage);

            }
            catch (Exception ex)
            {

            }
            return background;
        }

        /// <summary>
        /// Generate Random color at runtime
        /// </summary>
        /// <returns></returns>
        public Color RandomColourGenerator()
        {

            try
            {
                Random rnd = new Random();
                return Color.Argb(255, rnd.Next(256), rnd.Next(256), rnd.Next(256));
            }
            catch (Exception ex)
            {

            }
            return Color.White;
        }
        #endregion

        #region Loading data from webservice
        /// <summary>
        /// Asynchronous call for retriving data from server
        /// </summary>
        public class LoadingImgeFromServerAsync : AsyncTask<Java.Lang.Void, Java.Lang.Void, Java.Lang.Void>
        {
            private ProgressDialog _progressDialog;


            Bitmap bitmapImage = null;
            private ColorPatternActivity _context;
            private ImageView _imageView;

            private ViewGroup _viewGroup;
            String _Tag, OutPutString, _IntializeOrUpdate;



            public LoadingImgeFromServerAsync(ColorPatternActivity _context, ImageView imageView, String Tag, String IntializeOrUpdate, ViewGroup viewGroup)
            {

                this._context = _context;
                this._imageView = imageView;
                this._Tag = Tag;

                this._viewGroup = viewGroup;
                this._IntializeOrUpdate = IntializeOrUpdate;
            }

            protected override void OnPreExecute()
            {
                base.OnPreExecute();

                try
                {
                    /* To show the progress bar control */
                    _progressDialog = new ProgressDialog(_context);
                    _progressDialog.SetCancelable(true);
                    _progressDialog.SetCanceledOnTouchOutside(false);
                    _progressDialog.Show();
                    /*end of showing progress bar */
                }
                catch (Exception ex)
                {

                }
            }

            protected override Java.Lang.Void RunInBackground(params Java.Lang.Void[] @params)
            {
                try
                {
                    if (_Tag == "S")
                        OutPutString = _context.webServer.GetImageURLFromURL("http://www.colourlovers.com/api/patterns/random");
                    else
                        OutPutString = _context.webServer.GetHexcodeURLFromURL("http://www.colourlovers.com/api/colors/random");
                }
                catch (Exception ex)
                {

                }
                return null;
            }

            protected override void OnPostExecute(Java.Lang.Void result)
            {
                base.OnPostExecute(result);
                try
                {
                    if (_Tag == "S")
                    {
                        if (!OutPutString.Contains("ErrorMsg"))
                        {
                           
                            _imageView.SetImageBitmap(_context.webServer.GetImageBitmap());
                            _context.Title = OutPutString;
                            _progressDialog.Dismiss();
                            _context.ApplyAnimationToView(_imageView);
                           
                        }
                        else
                        {
                            _progressDialog.Dismiss();
                            _context.ApplyHexColor(_imageView, "", "S", "", true);
                            _context.AlertMessageShowing(OutPutString);
                        }

                        if (_IntializeOrUpdate == "ADD")
                        {
                            _progressDialog.Dismiss();
                            _imageView.Click += _context._imageView_Click;
                            _viewGroup.AddView(_imageView);
                            _context.ApplyAnimationToView(_imageView);
                        }

                    }
                    else
                    {
                        if (!OutPutString.Contains("ErrorMsg"))
                        {
                            _progressDialog.Dismiss();
                            _context.ApplyHexColor(_imageView,  _context.webServer.GetImageHexColor(), "C",OutPutString, true);
                        }
                        else if (OutPutString.Contains("ErrorMsg"))
                        {
                            _progressDialog.Dismiss();
                            _context.ApplyHexColor(_imageView, "", "C","", true);
                           
                            _context.AlertMessageShowing(OutPutString);

                        }
                        if (_IntializeOrUpdate == "ADD")
                        {
                            _progressDialog.Dismiss();
                            _imageView.Click += _context._imageView_Click;
                            _viewGroup.AddView(_imageView);
                            _context.ApplyAnimationToView(_imageView);
                        }
                    }


                }
                catch (Exception ex)
                {

                }

                if(_progressDialog.IsShowing)
                {
                    _progressDialog.Dismiss();
                }
                
            }

        }

        public void AlertMessageShowing(String message)
        {
            try
            {
                message = message.Replace("ErrorMsg", "");
                Toast.MakeText(this, "Server Side Error: " + message + " . So applying Random code generation for this view.", ToastLength.Short).Show();
            }
            catch(Exception ex)
            {

            }
        }
        #endregion

        /// <summary>
        /// Remove all views by shaking a device by using Accelerometer sensor
        /// </summary>
        /// <param name="sensor"></param>
        /// <param name="accuracy"></param>
        #region Android.Hardware.ISensorEventListener implementation

        public void OnAccuracyChanged(Android.Hardware.Sensor sensor, Android.Hardware.SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(Android.Hardware.SensorEvent e)
        {
            if (e.Sensor.Type == Android.Hardware.SensorType.Accelerometer)
            {
                float x = e.Values[0];
                float y = e.Values[1];
                float z = e.Values[2];

                DateTime curTime = System.DateTime.Now;
                if (hasUpdated == false)
                {
                    hasUpdated = true;
                    lastUpdate = curTime;
                    last_x = x;
                    last_y = y;
                    last_z = z;
                }
                else
                {
                    if ((curTime - lastUpdate).TotalMilliseconds > ShakeDetectionTimeLapse)
                    {
                        float diffTime = (float)(curTime - lastUpdate).TotalMilliseconds;
                        lastUpdate = curTime;
                        float total = x + y + z - last_x - last_y - last_z;
                        float speed = Math.Abs(total) / diffTime * 10000;

                        if (speed > ShakeThreshold)
                        {
                            Toast.MakeText(this, "shake detected w/ speed: " + speed, ToastLength.Short).Show();

                            SingleViewBacgroundview.RemoveAllViews();
                        }

                        last_x = x;
                        last_y = y;
                        last_z = z;
                    }
                }
            }
        }
        #endregion


        #region Animation apply to the imageView

        public void ApplyAnimationToView(ImageView selectedImage)
        {
            try
            {
                if (selectedImage != null)
                {
                    myRandomnumberGenerator = new Random();
                    if (myRandomnumberGenerator.Next() % 2 == 0)
                    {
                        animRotate = AnimationUtils.LoadAnimation(this, Resource.Animation.splashlogo_rotate);
                    }
                    else
                    {
                        animRotate = AnimationUtils.LoadAnimation(this, Resource.Animation.slide_down);
                    }
                   
                    selectedImage.StartAnimation(animRotate);
                    animRotate.SetAnimationListener(this);
                }
            }
            catch(Exception ex)
            {

            }
        }

        public void OnAnimationEnd(Android.Views.Animations.Animation animation)
        {
            try
            {

            }
            catch(Exception ex)
            {

            }
        }

        public void OnAnimationRepeat(Android.Views.Animations.Animation animation)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }

        public void OnAnimationStart(Android.Views.Animations.Animation animation)
        {
            try
            {

            }
            catch (Exception ex)
            {

            }
        }
        #endregion
    }
}