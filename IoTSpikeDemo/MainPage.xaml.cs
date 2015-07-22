// Copyright (c) Microsoft. All rights reserved.

using IoTSpike.IoTClient;
using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using System.Threading.Tasks;
using System.Collections;
using IoTSpike.IoTClient.Hardware;
using System.Collections.Generic;

namespace IoTSpike.IoTSpikeDemo
{
    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 4;
        private const int BUTTON_PIN = 23;

        private GpioPin ledPin;
        private GpioPin buttonPin;

        private GpioPinValue ledPinValue;
        private GpioPinValue buttonPinValue;

        private DispatcherTimer ledTimer;

        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        private bool ledIsActivated = false;
        private double currentRate = 500;

        private string connectionString = "Endpoint=sb://iotspikeeventhub-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=zzv+ObDHw5xGsRJ5mxZRjUGDRE3A9MhoyJgAVuwAqfo=";
        private string eventHubEntity = "ehdevices";

        private IIoTClient iotClient;

        public MainPage()
        {
            InitializeComponent();

            ledTimer = new DispatcherTimer();
            ledTimer.Interval = TimeSpan.FromMilliseconds(currentRate);
            ledTimer.Tick += RedTimer_Tick;

            LedSlider.Value = currentRate;

            InitGPIO();

            InitIoTClient();
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                ledPin = null;
                buttonPin = null;
                return;
            }

            ledPin = gpio.OpenPin(LED_PIN);
            buttonPin = gpio.OpenPin(BUTTON_PIN);

            // Initialize LED to the OFF state by first writing a HIGH value
            // We write HIGH because the LED is wired in a active LOW configuration
            ledPinValue = GpioPinValue.High;
            ledPin.Write(ledPinValue);
            ledPin.SetDriveMode(GpioPinDriveMode.Output);

            // Check if input pull-up resistors are supported
            if (buttonPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
            {
                buttonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            }
            else
            {
                buttonPin.SetDriveMode(GpioPinDriveMode.Input);
            }

            // Set a debounce timeout to filter out switch bounce noise from a button press
            buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            buttonPin.ValueChanged += buttonPin_ValueChanged;
        }

        private void InitIoTClient()
        {
            // set and open the IoT client
            if (this.iotClient == null)
            {
                // this.iotClient = new IoTClient("IoTSpikeRaspberry", Guid.NewGuid().ToString(), this.connectionString, this.eventHubEntity);
                this.iotClient = new IoTClientConnectTheDots("IoTSpikeRaspberry", Guid.NewGuid().ToString(), this.connectionString, this.eventHubEntity);
            }

            if (!this.iotClient.IsOpen)
                this.iotClient.Open();
        }

        private void RedTimer_Tick(object sender, object e)
        {
            if (ledPinValue == GpioPinValue.High)
            {
                if (ledIsActivated)
                {
                    ledPinValue = GpioPinValue.Low;
                    ledPin.Write(ledPinValue);
                    Led.Fill = redBrush;
                    SendData();
                }
            }
            else
            {
                if (ledIsActivated)
                {
                    ledPinValue = GpioPinValue.High;
                    ledPin.Write(ledPinValue);
                    Led.Fill = grayBrush;
                }
            }
        }

        private void SendData()
        {
            Task.Run(() =>
            {
                IDictionary bag = new Dictionary<SensorType, float>();

                var rate = (float)currentRate;

                SensorType sensorType = SensorType.Temperature;

                if (!bag.Contains(sensorType))
                    bag.Add(sensorType, rate);
                else
                    bag[sensorType] = rate;

                if ((this.iotClient != null) && (this.iotClient.IsOpen))
                {
                    this.iotClient.SendAsync(bag);
                }
            });

        }

        private void LedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ledPin != null)
            {
                ledTimer.Stop();
                ledTimer = new DispatcherTimer();
                ledTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
                ledTimer.Tick += RedTimer_Tick;
                ledTimer.Start();
                currentRate = e.NewValue;
            }
        }

        private void buttonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {
            // toggle the state of the LED every time the button is pressed
            if (e.Edge == GpioPinEdge.FallingEdge)
            {
                if (ledIsActivated)
                {
                    ledPinValue = GpioPinValue.High;
                    ledPin.Write(ledPinValue);
                    ledIsActivated = false;
                }
                else
                {
                    ledIsActivated = true;
                }
            }
        }
    }
}
