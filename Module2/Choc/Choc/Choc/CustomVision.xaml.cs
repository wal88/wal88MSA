using Choc.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Plugin.Geolocator;

namespace Choc
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomVision : ContentPage
    {

        private float MilkChocProb;
        private float DarkChocProb;


        public CustomVision()
        {
            InitializeComponent();
        }


        private async void loadCamera(object sender, EventArgs e)
        {
            try
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    await DisplayAlert("No Camera", ":( No camera available.", "OK");
                    return;
                }

                MediaFile file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    PhotoSize = PhotoSize.Medium,
                    Directory = "Sample",
                    Name = $"{DateTime.UtcNow}.jpg"
                });

                if (file == null)
                    return;

                image.Source = ImageSource.FromStream(() =>
                {
                    return file.GetStream();
                });


                await MakePredictionRequest(file);

                await postChocProbabilitiesAsync();

                
            } catch (Exception a)
            {
                System.Diagnostics.Debug.WriteLine(a.Message);
            }
        }


        static byte[] GetImageAsByteArray(MediaFile file)
        {
            var stream = file.GetStream();
            BinaryReader binaryReader = new BinaryReader(stream);
            return binaryReader.ReadBytes((int)stream.Length);
        }

        async Task MakePredictionRequest(MediaFile file)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Prediction-Key", "72a9452ccf2e40259859976416b671eb");

            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.0/Prediction/5a682277-65b8-41c4-9097-29ecee001805/image";

            HttpResponseMessage response;

            byte[] byteData = GetImageAsByteArray(file);

            using (var content = new ByteArrayContent(byteData))
            {

                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(url, content);


                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    JObject rss = JObject.Parse(responseString);

                    //Querying with LINQ, Get all Prediction Values
                    var Probabilities = from p in rss["Predictions"] select (string)p["Probability"];
                    var Tags = from p in rss["Predictions"] select (string)p["Tag"];

                    //Truncate values to labels in XAML
                    for(var i=0; i < Tags.Count(); i++)
                    {
                        string tagName = Tags.ElementAt(i);
                        float probability = float.Parse(Probabilities.ElementAt(i));
                        TagLabel.Text += "Probability of being " + tagName + "= " + probability + "\n";
                        if (i == 0)
                            MilkChocProb = probability;
                        else if (i == 1)
                            DarkChocProb = probability;
                    }


                    //foreach (var item in Tags)
                    //{
                    //    TagLabel.Text += item + ": \n";
                    //}

                    //EvaluationModel responseModel = JsonConvert.DeserializeObject<EvaluationModel>(responseString);
                    //double max = responseModel.Predictions.Max(m => m.Probability);
                    //TagLabel.Text = (max >= 0.5) ? "Hotdog" : "Not hotdog";

                }

                //Get rid of file once we have finished using it
                file.Dispose();
            }
            

        }

        async Task postChocProbabilitiesAsync()
        {

            //var locator = CrossGeolocator.Current;
            //locator.DesiredAccuracy = 50;

            //var position = await locator.GetPositionAsync(1000);

            List<WalTable> chocInformation = await AzureManager.AzureManagerInstance.GetChocInformation();
            int tableCount = chocInformation.Count;

            string newRowID = tableCount + 1 + "";

            WalTable model = new WalTable()
            {
                ID = newRowID,
                MilkChocProb = MilkChocProb,
                DarkChocProb = DarkChocProb
            };

            await AzureManager.AzureManagerInstance.PostChocInformation(model);
        }


    }
}