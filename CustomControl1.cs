using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FTDI_SPI_project
{
    public class IndicatorBar : Control
    {
        private List<Rectangle> _signals = new();

        static IndicatorBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(IndicatorBar),
                new FrameworkPropertyMetadata(typeof(IndicatorBar)));
        }

        public static readonly DependencyProperty IndicatorLabelProperty = DependencyProperty.Register(
        nameof(IndicatorLabel),
        typeof(string),
        typeof(IndicatorBar),
        new PropertyMetadata("Bar Label"));

        public string IndicatorLabel
        {
            get => (string)GetValue(IndicatorLabelProperty);
            set => SetValue(IndicatorLabelProperty, value);
        }

        public static readonly DependencyProperty IndicatorValueProperty = DependencyProperty.Register(
        nameof(IndicatorValue),
        typeof(double),
        typeof(IndicatorBar),
        new PropertyMetadata(0.0, Update_Indicator_Value));

        public double IndicatorValue
        {
            get => (double)GetValue(IndicatorValueProperty);
            set => SetValue(IndicatorValueProperty, value);
        }

        private static void Update_Indicator_Value(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            var control = (IndicatorBar)d;

            double newValue = (double)e.NewValue;

            control.UpdateRectColor();

            Debug.WriteLine("Indicator value was changed...");

        }

        private void UpdateRectColor()
        {
            if ((IndicatorValue >= 0.5) || (IndicatorValue <= -0.5))
            {
                _signals[5].Fill = (Brush)new BrushConverter().ConvertFrom("#FF00FF00") ;
            }
            else if ((IndicatorValue >= 0.2) || (IndicatorValue <= -0.2))
            {
                _signals[5].Fill = Brushes.Blue;
            }
            else
            {
                _signals[5].Fill = Brushes.Transparent;
            }

            for (int i = 0; i <= 4; i++)
            {
                if ((i + 1.0) < IndicatorValue)
                {
                    if (i == 4)
                    {
                        if (7.0 < IndicatorValue)
                        {
                            _signals[0].Fill = (Brush)new BrushConverter().ConvertFrom("#FFFF0000");
                        }
                        else
                        {
                            _signals[4 - i].Fill = (Brush)new BrushConverter().ConvertFrom("#FF00FF00");
                        }
                    }
                    else
                    {
                        _signals[4 - i].Fill = (Brush)new BrushConverter().ConvertFrom("#FF00FF00");
                    }
                }
                else
                {
                    _signals[4-i].Fill = Brushes.Transparent;
                }
            }

            for (int i = 6; i <= 10; i++)
            {
                if (((-i + 6) - 1.0) > IndicatorValue)
                {
                    if (i == 10)
                    {
                        if (-7.0 > IndicatorValue)
                        {
                            _signals[i].Fill = (Brush)new BrushConverter().ConvertFrom("#FFFF0000");
                        }
                        else
                        {
                            _signals[i].Fill = (Brush)new BrushConverter().ConvertFrom("#FF00FF00");
                        }
                    }
                    else
                    {
                        _signals[i].Fill = (Brush)new BrushConverter().ConvertFrom("#FF00FF00");
                    }
                }
                else
                {
                    _signals[i].Fill = Brushes.Transparent;
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _signals.Clear();

            for (int i = 0; i <= 10; i++)
            {
                var Indicator_Element = GetTemplateChild($"Signal_{i}") as Rectangle;
                if (Indicator_Element != null)
                {
                    _signals.Add(Indicator_Element);
                }
            }
            UpdateRectColor();
        }
    }

   public class Angulargauge : Control
   {
       static Angulargauge()
       {
           DefaultStyleKeyProperty.OverrideMetadata(
               typeof(Angulargauge),
               new FrameworkPropertyMetadata(typeof(Angulargauge)));
       }

       public static readonly DependencyProperty AngularValueProperty =
           DependencyProperty.Register(
               nameof(AngularValue),
               typeof(double),
               typeof(Angulargauge),
               new PropertyMetadata(0.0));

       public double AngularValue
       {
           get => (double)GetValue(AngularValueProperty);
           set => SetValue(AngularValueProperty, value);
       }

       public static readonly DependencyProperty AngleLabelProperty = DependencyProperty.Register(
           nameof(AngleLabel),
           typeof(string),
           typeof(Angulargauge),
           new PropertyMetadata("Bar"));

       public string AngleLabel
       {
           get => (string)GetValue(AngleLabelProperty);
           set => SetValue(AngleLabelProperty, value);
       }
    }
}