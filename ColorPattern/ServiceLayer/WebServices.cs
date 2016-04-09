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
using System.Net;
using System.IO;
using System.Xml.Linq;
using Android.Graphics;

namespace ColorPattern.ServiceLayer
{
    public class WebServices
    {
        String OutputString = string.Empty;
        XDocument doc;
        Bitmap bitmapImage;
        String BitmapURL,ImgHexColor, imageTitle;

        public String GetImageURLFromURL(String ServiceAddress)
        {
            string ParameterString = string.Empty;
            try
            {

                #region

                try
                {
                    if (ServiceAddress != null)
                    {

                        var url = new System.Uri(ServiceAddress);
                        var request = HttpWebRequest.Create(url);
                        request.Method = "POST";
                        request.Timeout = 180000;
                        var sw = new StreamWriter(request.GetRequestStream());
                        sw.Write(url.ToString());
                        sw.Close();
                        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                        {
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                var content = reader.ReadToEnd();
                                if (content != "")
                                {
                                    XDocument doc = XDocument.Parse(content);

                                    foreach (XElement ele in doc.Root.Elements("pattern"))
                                    {
                                        BitmapURL = (string)ele.Element("imageUrl");
                                        imageTitle = (string)ele.Element("title");

                                    }
                                    if (imageTitle != "")
                                        bitmapImage = GetImageBitmapFromUrl(BitmapURL);
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    imageTitle = "ErrorMsg" + ExceptionLog(ex);
                }

                #endregion
            }
            catch (Exception e)
            {
            }
            return imageTitle;
        }

        public String GetHexcodeURLFromURL(String ServiceAddress)
        {
            string ParameterString = string.Empty;
            try
            {

                #region

                try
                {

                    if (ServiceAddress != null)
                    {

                        var url = new System.Uri(ServiceAddress);
                        var request = HttpWebRequest.Create(url);
                        request.Method = "POST";
                        request.Timeout = 180000;
                        var sw = new StreamWriter(request.GetRequestStream());
                        sw.Write(url.ToString());
                        sw.Close();
                        using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                        {
                            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                            {
                                var content = reader.ReadToEnd();
                                if (content != "")
                                {
                                    XDocument doc = XDocument.Parse(content);

                                    foreach (XElement ele in doc.Root.Elements("color"))
                                    {
                                        OutputString = (string)ele.Element("title");
                                        ImgHexColor = (string)ele.Element("hex");
                                    }

                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    OutputString = "ErrorMsg" + ExceptionLog(ex);
                }

                #endregion
            }
            catch (Exception e)
            {

            }
            return OutputString;
        }

        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            try
            {
                using (var webClient = new WebClient())
                {
                    var imageBytes = webClient.DownloadData(url);
                    if (imageBytes != null && imageBytes.Length > 0)
                    {
                        imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                        bitmapImage = imageBitmap;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return imageBitmap;
        }

        public Bitmap GetImageBitmap()
        {
           
            return bitmapImage;
        }

        public String GetImageHexColor()
        {

            return ImgHexColor;
        }

        public string ExceptionLog(Exception ex)
        {
            string ExceptionLog = string.Empty;
            try
            {
                if (ex.Message.Contains("There was no endpoint listening at") == true)
                {
                    ExceptionLog = "Unable to Connect Remote Server";
                    return ExceptionLog;
                }
                //else if (ex.Message.Contains("The remote server returned an unexpected response: (403) Forbidden") == true)
                else if (ex.Message.Contains("The remote server returned an error: (403) Forbidden") == true)
                {
                    ExceptionLog = "Forbidden Error";
                    return ExceptionLog;
                }
                else if (ex.Message.Contains("The remote server returned an error") == true)
                {
                    ExceptionLog = "Server is busy";
                    return ExceptionLog;
                }
                else if (ex.Message.Contains("CommunicationException") == true)
                {
                    ExceptionLog = "Token expired ";
                    return ExceptionLog;
                }
                else if (ex.Message.Contains("establishing a connection to SQL Server") == true)
                {
                    ExceptionLog = "Database Connection failed";
                    return ExceptionLog;
                }
                else if (ex.Message.Contains("The time allotted to this operation may have been a portion of a longer timeout") == true)
                {
                    ExceptionLog = "Timed-out expired";
                    return ExceptionLog;
                }
                else if (ex.Message.Contains("The timeout period elapsed prior to obtaining a connection from the pool") == true)
                {
                    ExceptionLog = "Database Connection failed";
                    return ExceptionLog;
                }
                else if (ex.Message.Contains("The timeout period elapsed prior to completion of the operation or the server is not responding") == true)
                {
                    ExceptionLog = "Database Connection Timed-out expired";
                    return ExceptionLog;
                }
                else if (ex.Message.Contains("Exception of type 'System.Net.WebException'") == true)
                {
                    ExceptionLog = "Please Reload the page again";
                    return ExceptionLog;
                }
                
            }
            catch (Exception exx)
            {
                ExceptionLog = "Unknown Exception";
                return ExceptionLog;
            }

            return ExceptionLog;
        }
    }
}