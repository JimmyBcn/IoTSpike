// Copyright (c) Microsoft. All rights reserved.

using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Blinky
{
    public sealed partial class MainPage : Page
    {
        private const int RED_LED_PIN = 5;
        private const int YELLOW_LED_PIN = 6;

        private GpioPin redPin;
        private GpioPin yellowPin;

        private GpioPinValue redPinValue;
        private GpioPinValue yellowPinValue;

        private DispatcherTimer redTimer;
        private DispatcherTimer yellowTimer;

        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush yellowBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);

        public MainPage()
        {
            InitializeComponent();

            redTimer = new DispatcherTimer();
            redTimer.Interval = TimeSpan.FromMilliseconds(500);
            redTimer.Tick += RedTimer_Tick; 

            yellowTimer = new DispatcherTimer();
            yellowTimer.Interval = TimeSpan.FromMilliseconds(500);
            yellowTimer.Tick += YellowTimer_Tick;

            RedSlider.Value = 500;
            YellowSlider.Value = 500;

            InitGPIO();
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                redPin = null;
                yellowPin = null;
                return;
            }

            redPin = gpio.OpenPin(RED_LED_PIN);
            redPinValue = GpioPinValue.High;
            redPin.Write(redPinValue);
            redPin.SetDriveMode(GpioPinDriveMode.Output);

            yellowPin = gpio.OpenPin(YELLOW_LED_PIN);
            yellowPinValue = GpioPinValue.High;
            yellowPin.Write(yellowPinValue);
            yellowPin.SetDriveMode(GpioPinDriveMode.Output);
        }

        private void RedTimer_Tick(object sender, object e)
        {
            if (redPinValue == GpioPinValue.High)
            {
                redPinValue = GpioPinValue.Low;
                redPin.Write(redPinValue);
                RedLed.Fill = redBrush;
            }
            else
            {
                redPinValue = GpioPinValue.High;
                redPin.Write(redPinValue);
                RedLed.Fill = grayBrush;
            }
        }

        private void YellowTimer_Tick(object sender, object e)
        {
            if (yellowPinValue == GpioPinValue.High)
            {
                yellowPinValue = GpioPinValue.Low;
                yellowPin.Write(yellowPinValue);
                YellowRed.Fill = yellowBrush;
            }
            else
            {
                yellowPinValue = GpioPinValue.High;
                yellowPin.Write(yellowPinValue);
                YellowRed.Fill = grayBrush;
            }
        }

        private void RedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (redPin != null)
            {
                redTimer.Stop();
                redTimer = new DispatcherTimer();
                redTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
                redTimer.Tick += RedTimer_Tick;
                redTimer.Start();
            }
        }

        private void YellowSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (yellowPin != null)
            {
                yellowTimer.Stop();
                yellowTimer = new DispatcherTimer();
                yellowTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
                yellowTimer.Tick += YellowTimer_Tick;
                yellowTimer.Start();
            }
        }
    }
}
