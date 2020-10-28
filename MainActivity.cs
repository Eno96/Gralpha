using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Graphics;
using Android.Content;
using Android.Provider;
using System;
using Android.Content.PM;
using System.Collections.Generic;
using Android.Views;
using Java.IO;
using System.IO;
using Android.Gms.Common.Apis;
using Android.Locations;
using Android.Gms.Location;
using Android.Gms.Common;
using Android.Gms.Maps.Model;
using Java.Util;
using Java.Lang;

namespace gralpha
{
    public static class BitmapHelpers
    {
        public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
        {
            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

            // Next we calculate the ratio that we need to resize the image by
            // to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            if (inSampleSize == 0) inSampleSize = 1;
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            Bitmap nova = null;
            if (resizedBitmap != null) nova = Bitmap.CreateScaledBitmap(resizedBitmap, outWidth / inSampleSize, outHeight / inSampleSize, false);
            FileStream fOut;
            try
            {
                fOut = new FileStream(fileName, FileMode.Open);
                nova.Compress(Bitmap.CompressFormat.Jpeg, 30, fOut);
                fOut.Flush();
                fOut.Close();
            }
            catch (System.Exception e) {
                System.Console.WriteLine(e.StackTrace);
            }

            return resizedBitmap;
        }
    }

    public static class App
    {
        public static Java.IO.File _file;
        public static Java.IO.File _dir;
        public static Bitmap bitmap;
    }

    [Activity(Label = "gralpha", MainLauncher = true)]
    public class MainActivity : Activity, IOnMapReadyCallback, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
        private Location _currentLocation;
        private MapFragment _mapFragment;
        private GoogleMap _map;
        private PictureTaken pictureTaken;
        private Pictures _pictures;

        GoogleApiClient googleApiClient;
        public void OnMapReady(GoogleMap googleMap)
        {
            _map = googleMap;
            _map.MapType = GoogleMap.MapTypeSatellite;
            if (_currentLocation != null)
            {
                _map.AddMarker(new MarkerOptions().SetPosition(new LatLng(_currentLocation.Latitude, _currentLocation.Longitude)).SetTitle("Vasa trenutna pozicija"));
                _map.MoveCamera(CameraUpdateFactory.NewLatLng(new LatLng(_currentLocation.Latitude, _currentLocation.Longitude)));
                _map.AnimateCamera(CameraUpdateFactory.ZoomTo(16));
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            googleApiClient = new GoogleApiClient.Builder(this)
                .AddApi(LocationServices.API)
                .AddConnectionCallbacks(this)
                .AddOnConnectionFailedListener(this)
                .Build();
            googleApiClient.Connect();

            if (IsThereAnAppToTakePictures())
            {
                CreateDirectoryForPictures();
            }

            FragmentManager.BeginTransaction().Add(Resource.Id.gmap, new WelcomePage()).Commit();
            ImageButton map = FindViewById<ImageButton>(Resource.Id.map);
            map.Click += delegate
            {

                if (_mapFragment == null)
                {
                    InitMap();
                }
                else
                {
                    FragmentManager.BeginTransaction().Replace(Resource.Id.gmap, _mapFragment, "map").Commit();
                    _mapFragment.GetMapAsync(this);
                }

            };

            ImageButton camera = FindViewById<ImageButton>(Resource.Id.camera);

            camera.Click += delegate
            {
                pictureTaken = new PictureTaken();

                Bundle mybundle2 = new Bundle();
                CreateDirectoryForPictures();
                if (App._dir.ListFiles().Length == 0)
                {
                    mybundle2.PutString("path", "null");
                }
                else
                {
                    Java.IO.File newfile = App._dir.ListFiles()[App._dir.ListFiles().Length - 1];
                    mybundle2.PutString("path", newfile.Path.ToString());
                }

                pictureTaken.Arguments = mybundle2;

                FragmentManager.BeginTransaction().Replace(Resource.Id.gmap, pictureTaken, "slika").Commit();
                Intent intent = new Intent(MediaStore.ActionImageCapture);
                App._file = new Java.IO.File(App._dir, System.String.Format("report_{0}.jpg", Guid.NewGuid()));
                intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App._file));
                StartActivityForResult(intent, 0);

            };

            ImageButton can = FindViewById<ImageButton>(Resource.Id.can);

            can.Click += delegate
            {
                _pictures = new Pictures();
                Bundle mybundle = new Bundle();
                CreateDirectoryForPictures();
                if (App._dir.ListFiles().Length == 0)
                {
                    mybundle.PutString("path", "null");
                }
                else
                {
                    Java.IO.File newfile = App._dir.ListFiles()[App._dir.ListFiles().Length - 1];
                    mybundle.PutString("path", newfile.Path.ToString());
                }

                mybundle.PutString("lon", _currentLocation.Longitude.ToString());
                mybundle.PutString("lat", _currentLocation.Latitude.ToString());


                _pictures.Arguments = mybundle;
                FragmentManager.BeginTransaction().Replace(Resource.Id.gmap, _pictures, "pictures").Commit();
            };

            ImageButton help = FindViewById<ImageButton>(Resource.Id.help) as ImageButton;
            help.Click += delegate
            {
                HelpPage hp = new HelpPage();
                FragmentManager.BeginTransaction().Replace(Resource.Id.gmap, hp, "help").Commit();
            };

            ImageButton welcome = FindViewById<ImageButton>(Resource.Id.first) as ImageButton;
            welcome.Click += delegate
            {
                WelcomePage wp = new WelcomePage();
                FragmentManager.BeginTransaction().Replace(Resource.Id.gmap, wp, "help").Commit();
            };
        }

        private void InitMap()
        {
            _mapFragment = FragmentManager.FindFragmentByTag("gmap") as MapFragment;
            if (_mapFragment == null)
            {
                GoogleMapOptions mapOptions = new GoogleMapOptions()
                    .InvokeMapType(GoogleMap.MapTypeSatellite)
                    .InvokeZoomControlsEnabled(false)
                    .InvokeCompassEnabled(true);

                _mapFragment = MapFragment.NewInstance(mapOptions);
                FragmentManager.BeginTransaction().Replace(Resource.Id.gmap, _mapFragment, "map").Commit();
            }
            _mapFragment.GetMapAsync(this);
        }

        private void CreateDirectoryForPictures()
        {
            App._dir = new Java.IO.File(
                Android.OS.Environment.GetExternalStoragePublicDirectory(
                    Android.OS.Environment.DirectoryPictures), "gralpha");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }

        private bool IsThereAnAppToTakePictures()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            // Make it available in the gallery

            Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
            Android.Net.Uri contentUri = Android.Net.Uri.FromFile(App._file);
            mediaScanIntent.SetData(contentUri);
            SendBroadcast(mediaScanIntent);

            // Display in ImageView. We will resize the bitmap to fit the display.
            // Loading the full sized image will consume to much memory
            // and cause the application to crash.

            int height = Resources.DisplayMetrics.HeightPixels;
            int width = 2048;
            App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
            if (App.bitmap != null)
            {
                pictureTaken._image.SetImageBitmap(App.bitmap);
                App.bitmap = null;
            }

            // Dispose of the Java side bitmap.
            GC.Collect();
        }

        public void OnConnected(Bundle connectionHint)
        {
            _currentLocation = LocationServices.FusedLocationApi.GetLastLocation(googleApiClient);
            if (_map != null)
            {
                OnMapReady(_map);
            }
        }

        public void OnConnectionSuspended(int cause)
        {
            throw new NotImplementedException();
        }

        public void OnConnectionFailed(ConnectionResult result)
        {
            throw new NotImplementedException();
        }
    }
}

