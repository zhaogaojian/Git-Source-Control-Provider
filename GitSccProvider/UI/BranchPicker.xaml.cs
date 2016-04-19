using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public ObservableCollection<GitBranchInfo> _branches { get; set; }
        SwitchBranchInfo _pickerResult;

        public BranchPicker(GitRepository repository)
        {
            InitializeComponent();
            this.repository = repository;
            _branches = new ObservableCollection<GitBranchInfo>();
            
            comboBranches.ItemsSource = _branches;
            //this.list = list;
        }

        internal SwitchBranchInfo Show()
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
            _branches.Clear();
            var branches = repository.GetBranchInfo(forceReload:true);
            branches = branches.OrderBy(x => x.IsRemote).ThenBy(x => x.FullName).ToList();//.Select(r => r.FullName); ;
            foreach (var gitBranchInfo in branches)
            {
                _branches.Add(gitBranchInfo);
            }
            comboBranches.ItemsSource = branches;
            comboBranches.Items.Refresh();
            comboBranches.DisplayMemberPath = "FullName";
            comboBranches.SelectedValuePath = "CanonicalName";
            comboBranches.SelectedValue = repository.CurrentBranchInfo.CanonicalName;
            _pickerResult = new SwitchBranchInfo();
            _pickerResult.Repository = repository;


            if (window.ShowDialog() == true)
            {
                return _pickerResult;
            }
            else
            {
                return new SwitchBranchInfo();
            }
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
                window.DialogResult = true;
                if (radioButton1.IsChecked == true)
                {
                    _pickerResult.BranchInfo = (GitBranchInfo) comboBranches.SelectedItem;
                    _pickerResult.Switch = true;
                    //var results = await repository.CheckoutAsync((GitBranchInfo)comboBranches.SelectedItem);
                    //if (!results.Succeeded)
                    //{
                    //    MessageBox.Show(results.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //}
                    //else
                    //{
                    //    window.DialogResult = true;
                    //}

                }
                else if (radioButton2.IsChecked == true && !string.IsNullOrWhiteSpace(txtNewBranch.Text))
                {
                    _pickerResult.CreateBranch = true;
                    _pickerResult.Switch = cbSwitch.IsChecked.HasValue ? cbSwitch.IsChecked.Value : false;
                    _pickerResult.BranchName = txtNewBranch.Text;
                    //var branchResult = repository.CreateBranch(txtNewBranch.Text);
                    //if (branchResult.Succeeded) 
                    //{
                    //    if (cbSwitch.IsChecked == true)
                    //    {
                    //        var switchResult = await repository.CheckoutAsync(branchResult.Item);
                    //        if (!switchResult.Succeeded)
                    //        {
                    //            MessageBox.Show(switchResult.ErrorMessage, "Error", MessageBoxButton.OK,
                    //                MessageBoxImage.Exclamation);
                    //        }
                    //        else
                    //        {
                    //            window.DialogResult = true;
                    //        } 
                    //    }
                    //    else
                    //    {
                    //        window.DialogResult = true;
                    //    }
                    //}
                    //else
                    //{
                    //    MessageBox.Show(branchResult.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    //}
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

        private class BranchSort : IComparer
        {
            public int Compare(object x, object y)
            {
                var first = (GitBranchInfo) x;
                var second = (GitBranchInfo)y;
                if (first.IsRemote && !second.IsRemote)
                {
                    return -1;
                }
                else if (second.IsRemote && !first.IsRemote)
                {
                    return 1;
                }
                else
                {
                    return first.Name.CompareTo(second.Name);
                }
            }
        }
    }
}
