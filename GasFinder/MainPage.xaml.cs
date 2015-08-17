using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Documents;
using Windows.UI;
using Windows.Devices.Geolocation;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.Services.Maps;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace GasFinder
{

    public sealed partial class MainPage : Page
    {
        private List<GasStation> stations = new List<GasStation>();
        private Geopoint myLocation;
        private Geopoint nearestStation;
        private SearchCriteria searchCriteria;
        private SpeechRecognizer recognizer;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            myLocation = GetDefaultPosition();
            MyMap.Center = myLocation;
            MyMap.ZoomLevel = 16;

            this.SetUserIcon(myLocation);

            this.SetStations();

            MyMap.Routes.Clear();

            base.OnNavigatedTo(e);

            // Get search criteria from parameter passed in frame.Navigate call
            searchCriteria = e.Parameter as SearchCriteria;
            // Check for null!
            if (searchCriteria != null)
            {
                nearestStation = GetNearestStation(searchCriteria);
                MyMap.Center = nearestStation;
            }

            //Get recognition result from paramter passed in frame.Navigate call
            SpeechRecognitionResult vcResult = e.Parameter as SpeechRecognitionResult;
            // Check for null
            if (vcResult != null)
            {
                string commandMode = vcResult.SemanticInterpretation.Properties["commandMode"][0];
                if (commandMode == "voice") //Did the use speak or type the command?
                {
                    HandleVoiceCommand(vcResult);
                }
                else if (commandMode == "text")
                {
                    HandleTextCommand(vcResult);
                }
            }
        }
        
        /// <summary>
        /// Sets an icon where the user is
        /// </summary>
        /// <param name="location">The location of the user</param>

        private void SetUserIcon(Geopoint location)
        {
            MapIcon mapIcon = new MapIcon();
            mapIcon.Location = location;
            mapIcon.NormalizedAnchorPoint = new Point(0.5, 1.0);
            mapIcon.Title = "You are here";
            MyMap.MapElements.Add(mapIcon);
        }

        private Geopoint GetNearestStation(SearchCriteria searchCriteria)
        {
            //Compute distance and return nearest gas station
            //Center map to the nearest gas station
            return new Geopoint(new BasicGeoposition { Latitude = stations.ElementAt(0).Latitude, Longitude = stations.ElementAt<GasStation>(0).Longitude });
        }

        /// <summary>
        /// Sets the center of the map to a default position.
        /// </summary>
        private Geopoint GetDefaultPosition()
        {
            return new Geopoint(new BasicGeoposition { Latitude = 48.290499, Longitude = 11.581094 });

        }

        /// <summary>
        /// Calculates the current position of the user and sets the center of the map there.
        /// </summary>
        private async void SetCurrentPosition()
        {
            Geolocator geolocator = new Geolocator();
            Geoposition geoposition = null;
            try
            {
                geoposition = await geolocator.GetGeopositionAsync();
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Exception: " + exception.Message);
            }
            myLocation = geoposition.Coordinate.Point;
            MyMap.Center = myLocation;
            MyMap.ZoomLevel = 16;

        }

        private void SetStations()
        {

            // I just hardcoded some stations here for example purposes
            Fuel randomDieselFuel = new Fuel("Diesel", 2);
            Fuel randomGasFuel = new Fuel("Benzin", 2.3);
            Fuel[] randomFuel = { randomDieselFuel, randomGasFuel };
            GasStation randomStation = new GasStation("RandomStation", 48.282320, 11.576871, randomFuel);
            GasStation bestStation = new GasStation("BestStation", 47.682609, -122.130754, randomFuel);

            stations.Add(randomStation);
            stations.Add(bestStation);

            MapIcon mapIcon01 = new MapIcon();
            this.SetMapIcon(mapIcon01, randomStation.Latitude, randomStation.Longitude);
            MapIcon mapIcon02 = new MapIcon();
            this.SetMapIcon(mapIcon02, bestStation.Latitude, bestStation.Longitude);
        }

        private void SetMapIcon(MapIcon mapIcon, double latitude, double longitude)
        {
            mapIcon.Image = RandomAccessStreamReference.CreateFromUri(
              new Uri("ms-appx:///Assets/gaspump_mapicon_small.png"));
            mapIcon.NormalizedAnchorPoint = new Point(0.25, 0.9);
            mapIcon.Location = new Geopoint(new BasicGeoposition { Latitude = latitude, Longitude = longitude });
            mapIcon.Title = "Gas Station";
            MyMap.MapElements.Add(mapIcon);
        }


        private void HandleTextCommand(SpeechRecognitionResult vcResult)
        {
            this.HandleCommand(vcResult);
        }

        private async void HandleVoiceCommand(SpeechRecognitionResult vcResult)
        {
            this.HandleCommand(vcResult);
            try
            {
                //Text to speech
                await SpeakText("Die nächste Tankstelle befindet sich 1 km entfernt. Sollte ich die beste Route berechnen?");
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Exception: " + exception.Message);
            }
        }


        private async void GetRouteAndDirections(Geopoint startPoint, Geopoint endPoint)
        {
            // Get the route between the points.
            MapRouteFinderResult routeResult =
                await MapRouteFinder.GetDrivingRouteAsync(
                startPoint,
                endPoint,
                MapRouteOptimization.Time,
                MapRouteRestrictions.None);

            if (routeResult.Status == MapRouteFinderStatus.Success)
            {
                // Use the route to initialize a MapRouteView.
                MapRouteView viewOfRoute = new MapRouteView(routeResult.Route);
                viewOfRoute.RouteColor = Colors.Yellow;
                viewOfRoute.OutlineColor = Colors.Black;

                // Add the new MapRouteView to the Routes collection
                // of the MapControl.
                MyMap.Routes.Add(viewOfRoute);

                // Fit the MapControl to the route.
                await MyMap.TrySetViewBoundsAsync(
                    routeResult.Route.BoundingBox,
                    null,
                    MapAnimationKind.None);
            }
        }

        private void MyMap_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            foreach (var mapObject in sender.FindMapElementsAtOffset(args.Position))
            {
                MapIcon touchedIcon = (MapIcon)mapObject;
                if(touchedIcon != null)
                {
                    this.GetRouteAndDirections(myLocation, nearestStation);
                }
            }
        }


        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Type navType = typeof(SearchPage);
            if (this.Frame != null)
            {
                this.Frame.Navigate(navType);
            }
        }

        private void HandleCommand(SpeechRecognitionResult vcResult)
        {
            string voiceCommandName = vcResult.RulePath.First(); //What command launched the app?

            switch (voiceCommandName) //React accordingly to the voice command
            {
                case "NextStation": //User requested the closest station
                    if (vcResult.Text.Contains("billigem") || vcResult.Text.Contains("günstige"))
                    {
                        searchCriteria = new SearchCriteria(Criteria.Price, FuelType.Super);
                    }
                    else
                    {
                        searchCriteria = new SearchCriteria(Criteria.Distance, FuelType.Super);
                    }

                    nearestStation = this.GetNearestStation(searchCriteria);
                    //Center map to nearest gas station
                    MyMap.Center = nearestStation;
                    break;

                case "anotherCommand": // User requested an unknown command
                    Debug.WriteLine("Unkown command");
                    break;
            }
        }

        //The object for controlling the speech synthesis object (voice)
        private async Task SpeakText(string textToSpeak)
        {
            using (var speech = new SpeechSynthesizer())
            {

                //Retrieve the first German female voice
                speech.Voice = SpeechSynthesizer.AllVoices.First(i => (i.Gender == VoiceGender.Female && i.Description.Contains("Germany")));
                //Generate audio streams from plain text
                SpeechSynthesisStream ttsStream = await speech.SynthesizeTextToStreamAsync(textToSpeak);
                mediaPlayer.SetSource(ttsStream, ttsStream.ContentType);
                mediaPlayer.Play();
                mediaPlayer.CurrentStateChanged += OnStateChanged;
            }
        }


        private async Task Listen()
        {
            try
            {
                //Perform speech recognition
                SpeechRecognitionResult speechRecognitionResult = await RecognizeSpeech();
                //Check the confidence level of the specch recognition attempt
                if (speechRecognitionResult.Confidence == SpeechRecognitionConfidence.Rejected)
                {
                    await SpeakText("Ich habe leider nicht verstanden. Kannst Du bitte wiederholen?");
                }

                else
                {
                    if (speechRecognitionResult.Text == "Ja" || speechRecognitionResult.Text == "Jawohl" ||
                        speechRecognitionResult.Text == "Klar" || speechRecognitionResult.Text == "Freilich" ||
                        speechRecognitionResult.Text == "Natürlich")
                    {
                        this.GetRouteAndDirections(myLocation, nearestStation);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception: " + e);
            }
        }



        private async void OnStateChanged(object sender, RoutedEventArgs e)
        {

            // When media player finished streaming start listening to the user
            if ((mediaPlayer.CurrentState == MediaElementState.Closed) ||
            (mediaPlayer.CurrentState == MediaElementState.Stopped) ||
            (mediaPlayer.CurrentState == MediaElementState.Paused))
            {
                mediaPlayer.CurrentStateChanged -= OnStateChanged;
                await Listen();
            }
        }


        private async Task<SpeechRecognitionResult> RecognizeSpeech()
        {
            try
            {
                if (recognizer == null)
                {
                    recognizer = new SpeechRecognizer();
                    string[] possibleAnswers = { "Ja", "Jawohl", "Klar", "Natürlich", "Nein", "Ne" };
                    var listConstraint = new SpeechRecognitionListConstraint(possibleAnswers, "Answer");
                    recognizer.UIOptions.ExampleText = @"Bsp. 'ja', 'nein'";
                    recognizer.Constraints.Add(listConstraint);

                    await recognizer.CompileConstraintsAsync();
                }

                SpeechRecognitionResult result = await recognizer.RecognizeWithUIAsync();
                return result;
            }
            catch (Exception exception)
            {
                const uint HResultPrivacyStatementDeclined = 0x80045509;
                if ((uint)exception.HResult == HResultPrivacyStatementDeclined)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog("You must accept the speech privacy policy");
                    messageDialog.ShowAsync().GetResults();
                }
                else
                {
                    Debug.WriteLine("Error: " + exception.Message);
                }
            }
            return null;
        }


        private void mediaPlayer_Unloaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("MediaPlayer unloaded");
            mediaPlayer.Stop();
            mediaPlayer.Source = null;
        }

    }
}
