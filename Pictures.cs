using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace gralpha
{
    public class Pictures : Fragment
    {
        public ImageView imageView;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View v = inflater.Inflate(Resource.Layout.Pictures, container, false);

            imageView = v.FindViewById<ImageView>(Resource.Id.picture1) as ImageView;
            String path = this.Arguments.GetString("path");
            if (path != "null") imageView.SetImageBitmap(path.LoadAndResizeBitmap(2048, Resources.DisplayMetrics.HeightPixels));

            String lon = this.Arguments.GetString("lon");
            String lat = this.Arguments.GetString("lat");

            ImageButton upload = v.FindViewById<ImageButton>(Resource.Id.upload) as ImageButton;
            upload.Click += delegate
            {
                System.Net.WebClient Client = new System.Net.WebClient();
                Client.Headers.Add("Content-Type", "binary/octet-stream");
                String name = "";
                for (int i = path.Length - 1; i >= 0; i--)
                {
                    if (path[i] == '/') break;
                    name += path[i];
                }
                String nameF = "";
                for (int i = 0; i < name.Length; i++)
                {
                    nameF += name[name.Length - i - 1];
                }

                NameValueCollection values = new NameValueCollection();
                values.Add("zahtjev", nameF);
                values.Add("lang", lon);
                values.Add("lat", lat);
                values.Add("grad", "Bihac");

                byte[] result = Client.UploadFile("http://192.168.3.36/cleanworld/pictureupload.php", "POST", path);
                Client.Headers.Set("Content-Type", "application/x-www-form-urlencoded");
                byte[] another = Client.UploadValues("http://192.168.3.36/cleanworld/locationupload.php", "POST", values);
                string Result_msg = Encoding.UTF8.GetString(result, 0, result.Length);
                Toast.MakeText(this.Activity, Result_msg, ToastLength.Long).Show();
                string Result_msg2 = Encoding.UTF8.GetString(another, 0, another.Length);
                Toast.MakeText(this.Activity, Result_msg2, ToastLength.Long).Show();
            };

            ImageButton no = v.FindViewById<ImageButton>(Resource.Id.delete) as ImageButton;

            no.Click += delegate
            {
                String pathimg = this.Arguments.GetString("path");
                Java.IO.File file = new Java.IO.File(path);
                file.Delete();
                Toast.MakeText(this.Activity, "Izbrisano.", ToastLength.Long).Show();
                this.Activity.FindViewById<ImageButton>(Resource.Id.can).PerformClick();
            };
            return v;
        }


    }
}