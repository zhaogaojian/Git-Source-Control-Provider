using System;
using System.Collections.Generic;
using System.Linq;
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
using GitScc.DataServices;

namespace GitScc.UI
{
    /// <summary>
    /// Interaction logic for BranchPicker.xaml
    /// </summary>
    public partial class BranchPicker : UserControl
    {
        private Window window;
        private GitRepository repository;
        //private IList<DataServices.Ref> list;

        public string BranchName { get; set; }
        public bool CreateNew { get; set; }

        protected List<GitBranchInfo> _branches; 

        public BranchPicker(GitRepository repository)
        {
            InitializeComponent();
            this.repository = repository;
            _branches = new List<GitBranchInfo>();
            //this.list = list;
        }

        internal bool? Show()
        {
            window = new Window
            {
                Title = "Switch (checkout) branch",
                Content = this,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = System.Windows.ResizeMode.NoResize,
                Width = 350,
                Height = 200
            };

            var branches = repository.GetBranchInfo().OrderBy(x => !x.IsRemote).ThenBy(x => x.Name);//.Select(r => r.FullName); ;
            comboBranches.ItemsSource = branches;
            comboBranches.DisplayMemberPath = "FullName";
            comboBranches.SelectedValuePath = "CanonicalName";
            comboBranches.SelectedValue = repository.CurrentBranchInfo.CanonicalName;
            return window.ShowDialog(); 
        }

        private void comboBranches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            radioButton1.IsChecked = true;
            btnOK.IsEnabled = true;
        }

        private void txtNewBranch_GotFocus(object sender, RoutedEventArgs e)
        {
            radioButton2.IsChecked = true;
            btnOK.IsEnabled = txtNewBranch.Text.Length > 0;
        }

        private void txtNewBranch_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnOK.IsEnabled = txtNewBranch.Text.Length > 0;
        }
        
        private async void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (radioButton1.IsChecked == true)
                {
                   var results = await repository.CheckoutAsync((GitBranchInfo) comboBranches.SelectedItem);
                    if (!results.Succeeded)
                    {
                        MessageBox.Show(results.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                    else
                    {
                        window.DialogResult = true;
                    }
                    
                }
                else if (radioButton2.IsChecked == true && !string.IsNullOrWhiteSpace(txtNewBranch.Text))
                {
                    var branchResult = repository.CreateBranch(txtNewBranch.Text);
                    if (branchResult.Succeeded) 
                    {
                        if (cbSwitch.IsChecked == true)
                        {
                            var switchResult = await repository.CheckoutAsync(branchResult.Item);
                            if (!switchResult.Succeeded)
                            {
                                MessageBox.Show(switchResult.ErrorMessage, "Error", MessageBoxButton.OK,
                                    MessageBoxImage.Exclamation);
                            }
                            else
                            {
                                window.DialogResult = true;
                            } 
                        }
                        else
                        {
                            window.DialogResult = true;
                        }
                    }
                    else
                    {
                        MessageBox.Show(branchResult.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
