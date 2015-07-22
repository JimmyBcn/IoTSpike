using IoTSpike.IoTClient;
using IoTSpike.IoTClient.Hardware;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Pi2ToEventHubs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private string connectionString = "[YOUR CONNECTION STRING HERE]";
        private string eventHubEntity = "[YOUR EVENTHUB NAME HERE]";

        private IIoTClient iotClient;

        public MainPage()
        {
            this.InitializeComponent();

            // set and open the IoT client
            if (this.iotClient == null)
            {
                // this.iotClient = new IoTClient("raspberrypi2", Guid.NewGuid().ToString(), this.connectionString, this.eventHubEntity);
                this.iotClient = new IoTClientConnectTheDots("raspberrypi2", Guid.NewGuid().ToString(), this.connectionString, this.eventHubEntity);
            }

            if (!this.iotClient.IsOpen)
                this.iotClient.Open();

            // just to start without UI :-)
            this.btnStart_Click(null, null);
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                IDictionary bag = new Dictionary<SensorType, float>();
                var rnd = new Random();

                while (true)
                {

                    var temperature = (float)GetRandomNumber(rnd, -10, 50);

                    SensorType sensorType = SensorType.Temperature;

                    if (!bag.Contains(sensorType))
                        bag.Add(sensorType, temperature);
                    else
                        bag[sensorType] = temperature;

                    if ((this.iotClient != null) && (this.iotClient.IsOpen))
                    {
                        this.iotClient.SendAsync(bag);
                    }

                    await Task.Delay(5000);
                }
            });

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this.iotClient.Close();
        }

        private double GetRandomNumber(Random random, double minimum, double maximum)
        {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }
}
