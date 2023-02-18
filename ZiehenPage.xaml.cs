using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Zeitmessung.Models;

namespace Zeitmessung
{
    /// <summary>
    /// Interaktionslogik für ZiehenPage.xaml
    /// </summary>
    public partial class ZiehenPage : Page
    {

        public bool isEditMode { get; set; }

        public bool isFresh { get; set; } = true;
        public bool isFinished { get; set; }

        public static ZiehenPage instance;
        List<Position> positions= new List<Position>();
        public ZiehenPage()
        {
            instance = this;
            InitializeComponent();

            positions.Add(new Position(0, "STM"));
            positions.Add(new Position(1, "STF"));
            positions.Add(new Position(2, "WTM"));
            positions.Add(new Position(3, "WTF"));
            positions.Add(new Position(4, "ATM"));
            positions.Add(new Position(5, "ATF"));
            positions.Add(new Position(6, "Maschinist"));
            positions.Add(new Position(7, "Melder"));
            positions.Add(new Position(8, "GK"));
        }

       

        private void btSTM_Click(object sender, RoutedEventArgs e)
        {
            SetOpacity("STM", imgSTM);
        }

        private void btSTF_Click(object sender, RoutedEventArgs e)
        {
            SetOpacity("STF", imgSTF);
        }

        private void btWTM_Click(object sender, RoutedEventArgs e)
        {
            SetOpacity("WTM", imgWTM);
        }

        private void btWTF_Click(object sender, RoutedEventArgs e)
        {
            SetOpacity("WTF", imgWTF);
        }

        private void btATM_Click(object sender, RoutedEventArgs e)
        {
            SetOpacity("ATM", imgATM);
        }

        private void btATF_Click(object sender, RoutedEventArgs e)
        {
            SetOpacity("ATF", imgATF);
        }

        private void btMelder_Click(object sender, RoutedEventArgs e)
        {
            SetOpacity("Melder", imgMelder);
        }

        private void btMaschinist_Click(object sender, RoutedEventArgs e)
        {
            SetOpacity("Maschinist", imgMaschinist);
        }

        private void btGK_Click(object sender, RoutedEventArgs e)
        {
            SetOpacity("GK",imgGK);
        }

       
        private void SetOpacity(string positionName,Image image)
        {
            positions.Where(s => s.Name == positionName).First().isEnabled = !positions.Where(s => s.Name == positionName).First().isEnabled;
            var isEnabled = positions.Where(s => s.Name == positionName).First().isEnabled;
            if (isEnabled) { image.Opacity = 1; }
            else { image.Opacity = 0.3; }
        }


        private void btEdit_Click(object sender, RoutedEventArgs e)
        {
            var bgreen = new BrushConverter().ConvertFromString("#C9EBC2");
            var green = bgreen as SolidColorBrush;
            lbZiehen.Visibility = Visibility.Hidden;

            isEditMode = !isEditMode;

            if(isEditMode)
            { 
                borderBack.BorderThickness = new Thickness(8);
                borderBack.BorderBrush = green;
                btSTM.IsEnabled = true;
                btSTF.IsEnabled = true;
                btWTM.IsEnabled = true;
                btWTF.IsEnabled = true;
                btATF.IsEnabled = true;
                btATM.IsEnabled = true;
                btMaschinist.IsEnabled=true;
                btMelder.IsEnabled=true;
                btGK.IsEnabled=true;

                btSTM.Visibility=Visibility.Visible;
                btSTF.Visibility = Visibility.Visible;
                btWTM.Visibility = Visibility.Visible;
                btWTF.Visibility = Visibility.Visible;
                btATM.Visibility = Visibility.Visible;
                btATF.Visibility = Visibility.Visible;
                btMaschinist.Visibility = Visibility.Visible;
                btMelder.Visibility = Visibility.Visible;
                btGK.Visibility = Visibility.Visible;


            }
            else 
            {
                borderBack.BorderThickness = new Thickness(0);
                btSTM.IsEnabled = false;
                btSTF.IsEnabled = false;
                btWTM.IsEnabled = false;
                btWTF.IsEnabled = false;
                btATF.IsEnabled = false;
                btATM.IsEnabled = false;
                btMaschinist.IsEnabled = false;
                btMelder.IsEnabled = false;
                btGK.IsEnabled = false;

                btSTM.Visibility = positions.Where(s => s.Name == "STM").First().GetVisibility();
                btSTF.Visibility = positions.Where(s => s.Name == "STF").First().GetVisibility();

                btWTM.Visibility = positions.Where(s => s.Name == "WTM").First().GetVisibility();
                btWTF.Visibility = positions.Where(s => s.Name == "WTF").First().GetVisibility();

                btATM.Visibility = positions.Where(s => s.Name == "ATM").First().GetVisibility();
                btATF.Visibility = positions.Where(s => s.Name == "ATF").First().GetVisibility();

                btMaschinist.Visibility = positions.Where(s => s.Name == "Maschinist").First().GetVisibility();
                btMelder.Visibility = positions.Where(s => s.Name == "Melder").First().GetVisibility();

                btGK.Visibility = positions.Where(s => s.Name == "GK").First().GetVisibility();

                MainWindow.instance.btZiehen.Focus();
                isFresh = true;
                isFinished = false;
            }
            

        }


       
        public void GetPosition()
        {

            if (isEditMode)
            {
                return;
            }
                var bred = new BrushConverter().ConvertFromString("#0A0A0A");
            var red = bred as SolidColorBrush;

            if (isFresh)
            {
                btSTM.Visibility = Visibility.Hidden;
                btSTF.Visibility = Visibility.Hidden;
                btWTM.Visibility = Visibility.Hidden;
                btWTF.Visibility = Visibility.Hidden;
                btATM.Visibility = Visibility.Hidden;
                btATF.Visibility = Visibility.Hidden;
                btMaschinist.Visibility = Visibility.Hidden;
                btMelder.Visibility = Visibility.Hidden;
                btGK.Visibility = Visibility.Hidden;
                isFresh = false;
                lbZiehen.Visibility = Visibility.Visible;
                borderBack.BorderThickness = new Thickness(0);
                positions.ForEach(s => s.isDrawn = false);
                return;
            }
            else
            {
                var enabledPositions = positions.Where(s=>s.isEnabled == true).ToList();
                var notDrawnPositions = enabledPositions.Where(s => s.isDrawn == false).ToList();
                
                if (notDrawnPositions.Count == 1)
                {
                    borderBack.BorderThickness = new Thickness(6);
                    borderBack.BorderBrush = red;
                    MainWindow.instance.btStopuhr.Focus();
                    isFresh = true;
                }
                lbZiehen.Visibility = Visibility.Hidden;

                Random rnd = new Random();
                var r = rnd.Next(notDrawnPositions.Count);
                var next = notDrawnPositions[r];
                
                
                
                next.isDrawn = true;
                switch (next.Name)
                {
                    case "STM":
                        btSTM.Visibility = Visibility.Visible;
                        break;
                    case "STF":
                        btSTF.Visibility = Visibility.Visible;
                        break;
                    case "WTM":
                        btWTM.Visibility = Visibility.Visible;
                        break;
                    case "WTF":
                        btWTF.Visibility = Visibility.Visible;
                        break;
                    case "ATM":
                        btATM.Visibility = Visibility.Visible;
                        break;
                    case "ATF":
                        btATF.Visibility = Visibility.Visible;
                        break;
                    case "Maschinist":
                        btMaschinist.Visibility = Visibility.Visible;
                        break;
                    case "Melder":
                        btMelder.Visibility = Visibility.Visible;
                        break;
                    case "GK":
                        btGK.Visibility = Visibility.Visible;
                        break;
                }
               

                
            }
            





        }
    }
}
