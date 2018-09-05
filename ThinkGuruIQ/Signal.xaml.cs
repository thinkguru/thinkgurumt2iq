using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using MtApi;
using MtApi.Monitors;
using System.Windows.Threading;

namespace ThinkGuruIQ
{
    /// <summary>
    /// Interaction logic for Signal.xaml
    /// </summary>
    public partial class Signal : Window
    {
        public Signal()
        {
            InitializeComponent();
            string userName = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string[] spl = userName.Split('\\');
            string dir = "C:\\Users\\" + spl[1].ToString() + "\\AppData\\Roaming\\MetaQuotes\\Terminal\\287469DEA9630EA94D0715D755974F1B\\MQL4\\Indicators";

            RunOnUiThread(() =>
            {
                if (Directory.Exists(dir))
                {
                    foreach (string name in Directory.GetFiles(dir, "*.ex4"))
                    {
                        string f_name = name.Replace(dir + "\\", "");
                        string ff = f_name.Replace(".ex4", "");
                        Indicators.Items.Add(ff);
                    }
                }
            });
        }

        private void RunOnUiThread(Action action)
        {

            Dispatcher.BeginInvoke(action);

        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            quote qt = new quote();


                qt.signal_name = signal_name.Text;
            qt.indicator = Indicators.Text;
            qt.signal_up = 0;
            qt.signal_down = 1;
            qt.trade_amount = double.Parse(trade_amnt.Text);
            qt.expiry_minue = int.Parse(expiry_time.Text);
            //qt.martingle = martingle.Text;
            qt.martingle_steps = int.Parse(Mar_steps.Text);
            qt.martingle_coef = double.Parse(mar_coef.Text);

            ThreadStart childthread = new ThreadStart(start_thread);
            Thread cht = new Thread(childthread);
            cht.Start();

            
           
            
        }

        private void start_thread() {
            start_calculation sc = new start_calculation();
            this.Dispatcher.Invoke(() =>

            {
                sc.DoWorkTimer(int.Parse(trade_time.Text), this.Title, Indicators.Text);
            });
            
        }
    }


   
}
