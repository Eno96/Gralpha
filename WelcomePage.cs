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
    public class WelcomePage : Fragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            View v = inflater.Inflate(Resource.Layout.WelcomePage, container, false);
            Button btn = v.FindViewById<Button>(Resource.Id.idiNa) as Button;
            btn.Click += delegate
            {
                this.Activity.FindViewById<ImageButton>(Resource.Id.camera).PerformClick();
            };
            return v;

        }
    }
}