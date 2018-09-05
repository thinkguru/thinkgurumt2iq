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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MtApi;
using MtApi.Monitors;
using System.IO;
using System.Threading;
using System.Windows.Threading;

namespace ThinkGuruIQ
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        //private object[] arr_data = new object[50];
        private List<string[]> arr_data = new List<string[]>();
        private List<quote_inductor> list_quote = new List<quote_inductor>();
        private readonly List<Action> _groupOrderCommands = new List<Action>();
        private readonly MtApiClient _apiClient = new MtApiClient();
        private readonly TimerTradeMonitor _timerTradeMonitor;

        private volatile bool _isUiQuoteUpdateReady = true;
        private bool isConnected = false;

        public bool chat_open_flag = false;

        public MainWindow()
        {
            InitializeComponent();
            // _apiClient.QuoteUpdated += apiClient_QuoteUpdated;
            _apiClient.QuoteUpdate += _apiClient_QuoteUpdate;
            _apiClient.QuoteAdded += apiClient_QuoteAdded;
            // _apiClient.QuoteRemoved += apiClient_QuoteRemoved;
            _apiClient.ConnectionStateChanged += apiClient_ConnectionStateChanged;
            _apiClient.OnLastTimeBar += _apiClient_OnLastTimeBar;
            // _apiClient.OnChartEvent += _apiClient_OnChartEvent;
            _apiClient.OnChartEvent += apiclient_chartEvent;
            connect();
        }

        private void _apiClient_OnLastTimeBar(object sender, TimeBarArgs e)
        {
            
           // RunOnUiThread(() => Update_console(e.ToString()));
           
        }

        private void connect()
        {
            _apiClient.BeginConnect(8222);
        }

        private void RunOnUiThread(Action action)
        {

            Dispatcher.BeginInvoke(action);

        }

        private void apiclient_chartEvent(object sender, ChartEventArgs e)
        {


        }

        private void apiClient_ConnectionStateChanged(object sender, MtConnectionEventArgs e)
        {
            RunOnUiThread(() =>
            {
                connect_status.Content = e.Status.ToString();
                if(e.Status.ToString() == "Connected")
                {
                    SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                    signal_circle.Fill = brush;
                }
                else
                {
                    SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    signal_circle.Fill = brush;
                }
            });

            switch (e.Status)
            {
                case MtConnectionState.Connected:
                    RunOnUiThread(OnConnected);
                    isConnected = true;
                    break;
                case MtConnectionState.Disconnected:
                case MtConnectionState.Failed:
                    RunOnUiThread(OnDisconnected);
                    break;
            }

            

        }

        private void OnConnected()
        {
            var quotes = _apiClient.GetQuotes();

            if (quotes != null)
            {
                foreach (var quote in quotes)
                {
                    RunOnUiThread(() => AddNewQuote(quote));

                }
            }
        }
        private void OnDisconnected()
        {
            isConnected = false;
            listViewQuotes.Items.Clear();
        }

        private void apiClient_QuoteAdded(object sender, MtQuoteEventArgs e)
        {
            if (e.Quote != null)
            {
                RunOnUiThread(() => AddNewQuote(e.Quote));
            }
        }



        private void AddNewQuote(MtQuote quote)
        {
            var key = quote.ExpertHandle.ToString();
           
            listViewQuotes.Items.Add(new data() { symbol = quote.Instrument.ToString(), ask = quote.Ask.ToString(), bid = quote.Bid.ToString(), expert = key });

          

        }

        private void ChangeQuote(MtQuote quote)
        {
            _isUiQuoteUpdateReady = false;

            var key = quote.ExpertHandle.ToString();

            for (int i = 0; i < listViewQuotes.Items.Count; i++)
            {


                if (((data)listViewQuotes.Items[i]).expert == key)
                {
                    //Update_console(((data)listViewQuotes.Items[i]).symbol.ToString());
                    ((data)listViewQuotes.Items[i]).ask = quote.Ask.ToString();
                    ((data)listViewQuotes.Items[i]).bid = quote.Bid.ToString();
                    listViewQuotes.Items.Refresh();
                    
                }
            }

            _isUiQuoteUpdateReady = true;
            
        }

        private void Update_console(string content)
        {
            ListBoxItem itm = new ListBoxItem();

            itm.Content = content;

            listBoxEventLog.Items.Insert(0, itm);
        }

        private void _apiClient_QuoteUpdate(object sender, MtQuoteEventArgs e)
        {
           
            if (_isUiQuoteUpdateReady)
            {
                RunOnUiThread(() => ChangeQuote(e.Quote));
                // RunOnUiThread(() => Update_console("Changed in - "+e.Quote.Instrument+" Ask - "+ e.Quote.Ask+" Bid - "+e.Quote.Bid));
            }

            RunOnUiThread(() =>
            {
                for (int i = 0; i < arr_data.Count; i++)
                {
                    if (e.Quote.Instrument.ToString() == arr_data[i][0].ToString())
                    {
                        // Update_console(arr_data[i][0].ToString() + "Quote Updated");
                        call_icheck(arr_data[i][0].ToString(), arr_data[i][1].ToString(), DateTime.Now.ToShortTimeString());
                    }
                }
            });

            // double up = _apiClient.iCustom(e.Quote.Instrument.ToString(), 0, "Rachan MS", 1, 1);
            //double dn = _apiClient.iCustom(e.Quote.Instrument.ToString(), 0, "Rachan MS", 0, 1);

            /*  if (dn > 0 && dn != 0.0)
              {
                  RunOnUiThread(() => Update_console("Put - " + e.Quote.Instrument.ToString() + " - " + dn));
              }
              if (up > 0 && up != 0.0)
              {
                  RunOnUiThread(() => Update_console("call - " + e.Quote.Instrument.ToString() + " - " + up));
              }*/
            //RunOnUiThread(() => account.Content = up.ToString());
        }



        private void chat_btn_MouseDown(object sender, MouseButtonEventArgs e)
        {
            /* Chat_doc.Content = new chat_doc();

             Storyboard sb = this.FindResource("chat_story") as Storyboard;
             Storyboard rb = this.FindResource("chat_story_rb") as Storyboard;
             Storyboard.SetTarget(sb, Chat_doc);
             Storyboard.SetTarget(rb, Chat_doc);
             if (chat_open_flag == false)
             {

                 sb.Completed += (o, s) => sb.Stop(); chat_open_flag = true;
                 sb.Dispatcher.Invoke(new Action(() => { sb.Begin(this); }));


                 MessageBox.Show(chat_open_flag.ToString());

             }
             else
             {

                 rb.Completed += (o, s) => sb.Stop(); chat_open_flag = false;
                 rb.Dispatcher.Invoke(new Action(() => {rb.Begin(this); }));

                 MessageBox.Show("HI there");
             } */

            chat ct = new chat();
            ct.Show();
        }
       

        private void connect_status_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(isConnected == false)
            {
                connect();
            }
        }

      

        private void listViewQuotes_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //Signal sg = new Signal();
            //data dt = ((List<data>)listViewQuotes.SelectedValue)[0];
           
            

            signal_name.Text = ((data)listViewQuotes.SelectedItems[0]).symbol.ToString();
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
            //sg.Show();
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            string[] qt = new string[8] 
            {
                signal_name.Text.ToString(),Indicators.Text.ToString(),0.ToString(),1.ToString(),trade_amnt.Text,expiry_time.Text,Mar_steps.Text,mar_coef.Text

        };

         

            /*ThreadStart childthread = new ThreadStart(start_thread);
            Thread cht = new Thread(childthread);
            cht.Start();*/

            arr_data.Add(qt);

            RunOnUiThread(() =>
            {
                indicator_quote.Items.Add(new indicator() { symbol = signal_name.Text, call = "", put = "" });
            });

                    

        }

        private void call_icheck(string Signal_name, string text, string time)
        {

            double up = _apiClient.iCustom(Signal_name,0,text,0,1);
            double dn = _apiClient.iCustom(Signal_name, 0, text, 1, 1);
            //Update_console(signal_name + "--" + text);
            if (signal(up) == true)
            {
                Update_console(Signal_name + "- Call -"+up.ToString() + " - " + time);
                
                    //Thread thread = new Thread(() => start_thread());
                
            }
            else if (signal(dn) == true)
            {
                Update_console(Signal_name + " - Put -" +dn.ToString() + " - " + time);

                //Thread thread = new Thread(() => start_thread());
                /*RunOnUiThread(() =>
                {
                    for (int j = 0; j < indicator_quote.Items.Count; i++)
                    {
                        if (((indicator)indicator_quote.Items[j]).symbol.ToString() == Signal_name.ToString())
                        {
                            ((indicator)indicator_quote.Items[j]).put = dn.ToString();
                            indicator_quote.Items.Refresh();
                        }
                    }
                }
                   );*/

            }

        }

        private bool signal(double value)
        {
            if (value > 0 && value != 2147483647)
                return true;
            else
                return false;
        }

        private void start_thread()
        {
            start_calculation sc = new start_calculation();
            this.Dispatcher.Invoke(() =>

            {
                sc.DoWorkTimer(int.Parse(trade_time.Text), this.Title, Indicators.Text);
            });

        }
    }

    public class data
    {
        public string symbol { get; set; }
        public string bid { get; set; }
        public string ask { get; set; }
        public string expert { get; set; }


    }
    public class indicator
    {
        public string symbol { get; set; }
        public string call { get; set; }
        public string put { get; set; }
        


    }

    class quote
    {
        public string signal_name { get; set; }
        public string indicator { get; set; }
        public int signal_up { get; set; }
        public int signal_down { get; set; }
        public double trade_amount { get; set; }
        public int expiry_minue { get; set; }
        public int martingle { get; set; }
        public int martingle_steps { get; set; }
        public double martingle_coef { get; set; }
    }

    class quote_inductor
    {
        public string Name { get; set; }
        public string Time { get; set; }
    }
    class start_calculation
    {
        private readonly MtApiClient _apiClient = new MtApiClient();
        private string symbol;
        private string indicator;
        DispatcherTimer _timer = new DispatcherTimer();

        public void DoWorkTimer(int time, string smb, string ind)
        {
            MessageBox.Show("thread started");
            this.symbol = smb;
            this.indicator = ind;
            _timer.Interval = TimeSpan.FromMinutes(time);
            _timer.Tick += _timer_Tick;
            _timer.IsEnabled = true;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {

            MessageBox.Show("Time Ticked");
            double up = _apiClient.iCustom(this.symbol, 0, this.indicator, 1, 1);
            double dn = _apiClient.iCustom(this.symbol, 0, this.indicator, 0, 1);

            if (dn > 0 && dn != 0.0)
            {
                RunOnUiThread(() => add_signal(up, dn));
            }
            if (up > 0 && up != 0.0)
            {
                RunOnUiThread(() => add_signal(up, dn));
            }

            //RunOnUiThread(() => account.Content = up.ToString());


        }

        private void add_signal(double up, double dn)
        {
            //MainWindow mw = new MainWindow();
            // mw.indicator_quote.Items.Add(new indicator() { symbol = symbol, call = up.ToString(), put = dn.ToString() });
            MessageBox.Show(symbol + " - " + up.ToString() + " - " + dn.ToString());
        }

        private void RunOnUiThread(Action action)
        {

            MainWindow mw = new MainWindow();
            mw.Dispatcher.BeginInvoke(action);

        }


    }


}
