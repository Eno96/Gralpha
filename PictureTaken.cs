using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace gralpha
{
    public class PictureTaken : Fragment
    {
        public ImageView _image;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View v = inflater.Inflate(Resource.Layout.PictureTaken, container, false);
            _image = v.FindViewById<ImageView>(Resource.Id.picture) as ImageView;
            ImageButton yes = v.FindViewById<ImageButton>(Resource.Id.yes) as ImageButton;
            ImageButton no = v.FindViewById<ImageButton>(Resource.Id.no) as ImageButton;
            yes.Click += delegate {
                ImageButton upld = this.Activity.FindViewById<ImageButton>(Resource.Id.can) as ImageButton;
                upld.PerformClick();
                Toast.MakeText(this.Activity, "Spremljeno", ToastLength.Long).Show();
            };

            no.Click += delegate
            {
                String path = this.Arguments.GetString("path");
                Java.IO.File file = new Java.IO.File(path);
                file.Delete();
                Toast.MakeText(this.Activity, "Izbrisano.", ToastLength.Long).Show();
            };

            return v;

        }
    }
}