using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Navigation;

namespace Flow.Launcher.Plugin.RDP {

    public partial class SettingsUserControl : UserControl {

        private Settings settings;
        /// <summary>
        /// usercontrol template for settings. right now there are no settings.
        /// </summary>
        /// <param name="settings"></param>
        public SettingsUserControl(Settings settings) {
            InitializeComponent();
            this.settings = settings;
        }
        /// <summary>
        /// allows the hyperlink to actually open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process process = new Process();
            process.StartInfo.FileName = e.Uri.AbsoluteUri;
            process.StartInfo.UseShellExecute = true;
            process.Start();
            e.Handled = true;
        }
    }
}