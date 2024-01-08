
using System.Windows;
using System.Management;
using System.Net.NetworkInformation;
using System.Linq;
using System.Windows.Controls;
using System.Globalization;
using System.Text.RegularExpressions;

namespace IPChanger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Button_Click_1(null, null);
            //ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter");

            //foreach (ManagementObject adapter in searcher.Get())
            //{
            //    cb.Items.Add(new Helper() { adapter = adapter });

            //}


        }
        ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
       

        public void setIP(ManagementObject objMO, string ip_address, string subnet_mask)
        {
            
            ManagementBaseObject newIP = objMO.GetMethodParameters("EnableStatic");

            newIP["IPAddress"] = new string[] { ip_address };
            newIP["SubnetMask"] = new string[] { subnet_mask };

            objMO.InvokeMethod("EnableStatic", newIP, null);
            
        }

        public void setGateway(ManagementObject objMO, string gateway)
        {
            ManagementBaseObject newGateway = objMO.GetMethodParameters("SetGateways");

            newGateway["DefaultIPGateway"] = new string[] { gateway };
            newGateway["GatewayCostMetric"] = new int[] { 1 };
            objMO.InvokeMethod("SetGateways", newGateway, null);
        }

        public void setDNS(ManagementObject objMO, string DNS1, string DNS2)
        {
            ManagementBaseObject newDNS =    objMO.GetMethodParameters("SetDNSServerSearchOrder");
            newDNS["DNSServerSearchOrder"] = new string[] { DNS1,DNS2};
            objMO.InvokeMethod("SetDNSServerSearchOrder", newDNS, null);
        }

        public void setWINS(ManagementObject objMO, string priWINS, string secWINS)
        {

            ManagementBaseObject wins =  objMO.GetMethodParameters("SetWINSServer");
            wins.SetPropertyValue("WINSPrimaryServer", priWINS);
            wins.SetPropertyValue("WINSSecondaryServer", secWINS);

            objMO.InvokeMethod("SetWINSServer", wins, null);
          
        }

        public void setDHCP(ManagementObject objMO) 
        {
            var ndns = objMO.GetMethodParameters("SetDNSServerSearchOrder");
            ndns["DNSServerSearchOrder"] = null;
            var enableDhcp = objMO.InvokeMethod("EnableDHCP", null, null);
            var setDns = objMO.InvokeMethod("SetDNSServerSearchOrder", ndns, null);
        }
        //Get adapters
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            cb_adapters.Items.Clear();
            ManagementClass objMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection objMOC = objMC.GetInstances();
          

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (ManagementObject adapter in objMOC)
                {
                    if (adapter["Description"].ToString() == nic.Description)
                        cb_adapters.Items.Add(new Helper() { adapter = adapter });

                }
                var value = nic.Name;
            }
        }
        //set ip
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int selectedindex = cb_adapters.SelectedIndex;
            if (dhcp.IsChecked == true)
            {
                setDHCP(((Helper)cb_adapters.SelectedItem).adapter);
            }
            else 
            {
                setIP(((Helper)cb_adapters.SelectedItem).adapter, lip.Text, lmask.Text);
                setGateway(((Helper)cb_adapters.SelectedItem).adapter, lgateway.Text);
                setDNS(((Helper)cb_adapters.SelectedItem).adapter, ldns1.Text, ldns2.Text);
            }
          
            Button_Click_1(null, null);
            cb_adapters.SelectedIndex = selectedindex;
            //setWINS(((Helper)cb.SelectedItem).adapter, dns2.Text, dns2.Text);
        }

        private void cb_adapters_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cb_adapters.Items.Count > 0) 
            {
                sip.Text = ((Helper)cb_adapters.SelectedItem).adapter.Properties["IPAddress"].Value != null ? ((string[])((Helper)cb_adapters.SelectedItem).adapter.Properties["IPAddress"].Value)[0].ToString() : "- - - -";
                smask.Text = ((Helper)cb_adapters.SelectedItem).adapter.Properties["IPSubnet"].Value != null ? ((string[])((Helper)cb_adapters.SelectedItem).adapter.Properties["IPSubnet"].Value)[0].ToString() : "- - - -";
                sgateway.Text = ((Helper)cb_adapters.SelectedItem).adapter.Properties["DefaultIPGateway"].Value != null ? ((string[])((Helper)cb_adapters.SelectedItem).adapter.Properties["DefaultIPGateway"].Value)[0].ToString() : "- - - -";
                string[] dns = ((Helper)cb_adapters.SelectedItem).adapter.Properties["DNSServerSearchOrder"].Value != null ? ((string[])((Helper)cb_adapters.SelectedItem).adapter.Properties["DNSServerSearchOrder"].Value) : new string[] { "- - - -", "- - - -" };
                if (dns.Length == 2)
                {
                    sdns1.Text = dns[0];
                    sdns2.Text = dns[1];
                }
                else
                {
                    sdns1.Text = dns[0];
                    sdns2.Text = "- - - -";
                }
            }
        
           
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            lip.IsEnabled = false;
            lmask.IsEnabled = false;
            lgateway.IsEnabled = false;
            ldns1.IsEnabled = false;
            ldns2.IsEnabled = false;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            lip.IsEnabled = true;
            lmask.IsEnabled = true;
            lgateway.IsEnabled = true;
            ldns1.IsEnabled = true;
            ldns2.IsEnabled = true;
        }

        private void lip_TextChanged(object sender, TextChangedEventArgs e)
        {
            Regex ip = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            MatchCollection result = ip.Matches(lip.Text);
        }
    }

    public class Helper 
    {
        public Helper() { }
        public ManagementObject adapter { set; get; }
        public override string ToString()
        {
            
            return adapter.Properties["Description"].Value.ToString();
        }

    }

  
}
