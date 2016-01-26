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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;
using VsTaskRunContext = Microsoft.VisualStudio.Shell.VsTaskRunContext;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell;

namespace GitSccProvider.UI
{
    /// <summary>
    /// Interaction logic for StatusBox.xaml
    /// </summary>
    public partial class StatusBox : UserControl
    {
        private Window window;
        private bool _visible = false;

        public StatusBox()
        {
            InitializeComponent();
        }

        public bool Visible
        {
            get { return _visible; }
        }

        public void Show(string title, string message, string buttonText, bool showButton = true)
        {
            window = new Window
            {
                Title = title,
                Content = this,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = System.Windows.ResizeMode.NoResize,
                Width = 350,
                Height = 200
            };
            _tbStatus.Text = message;
            _btnOk.Content = buttonText;
            ShowButton(showButton);
            _visible = true;
            window.Show();
        }

        public void Hide()
        {
            window.Hide();
            _visible = false;
        }

        public async Task SetTitle(string title)
        {
            await ThreadHelper.JoinableTaskFactory.RunAsync(
                VsTaskRunContext.UIThreadBackgroundPriority,
                async delegate
                {
                        // On caller's thread. Switch to main thread (if we're not already there).
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        // Now on UI thread via background priority.
                    window.Title = title;
                    // Resumed on UI thread, also via background priority.
                });
        }

        public async Task SetMessage(string message)
        {
            await ThreadHelper.JoinableTaskFactory.RunAsync(
               VsTaskRunContext.UIThreadBackgroundPriority,
               async delegate
               {
                    // On caller's thread. Switch to main thread (if we're not already there).
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                   // Now on UI thread via background priority.
                   _tbStatus.Text = message;
                   // Resumed on UI thread, also via background priority.
               });
           
        }

        public async Task SetButtonText(string buttonText)
        {
            await ThreadHelper.JoinableTaskFactory.RunAsync(
              VsTaskRunContext.UIThreadBackgroundPriority,
              async delegate
              {
                   // On caller's thread. Switch to main thread (if we're not already there).
                   await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                  // Now on UI thread via background priority.
                  _btnOk.Content = buttonText;
                  // Resumed on UI thread, also via background priority.
              });

        }


        public async Task ShowButton(bool showButton)
        {
            await ThreadHelper.JoinableTaskFactory.RunAsync(
             VsTaskRunContext.UIThreadBackgroundPriority,
             async delegate
             {
                  // On caller's thread. Switch to main thread (if we're not already there).
                  await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                 // Now on UI thread via background priority.
                 if (!showButton)
                 {
                     _btnOk.Visibility = Visibility.Collapsed;
                 }
                 else
                 {
                     _btnOk.Visibility = Visibility.Visible;
                 }
                 // Resumed on UI thread, also via background priority.
             });
          
        }

        private void _btnOk_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
