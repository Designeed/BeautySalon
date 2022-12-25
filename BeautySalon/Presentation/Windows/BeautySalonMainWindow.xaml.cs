using BeautySalon.Data;
using BeautySalon.Presentation.Windows;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
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

namespace BeautySalon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _currentPage = 1;
        private string _searchParameter = "";
        private int _sortParameter = 0;
        private int _filterParameter = 0;
        private ButtonState _state = ButtonState.Show;
        private int _itemPerPage = new BeautySalonModel().Clients.Count();
        private List<Client> _currentBirthdayClientList = new BeautySalonModel().Clients
            .Where(
                client =>
                    EntityFunctions.TruncateTime(client.Birthday).Value.Month
                    == DateTime.UtcNow.Month
            )
            .ToList();
        private List<Client> _filteredList = new BeautySalonModel().Clients.ToList();

        public List<Client> ClientList
        {
            get => new BeautySalonModel().Clients.ToList();
        }
        public int TotalPages
        {
            get => (int)Math.Ceiling((double)_filteredList.Count / _itemPerPage);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (value >= 1 && value <= TotalPages)
                {
                    _currentPage = value;
                    DisplayItemPerPage();
                }
            }
        }

        enum ButtonState
        {
            Show,
            Hide
        }

        public MainWindow()
        {
            InitializeComponent();
            SetSearchParameters();
            UpdateTotalNoteCount();
        }

        private void ResetPage()
        {
            _currentPage = 1;
        }

        private void UpdateNoteCount()
        {
            noteCountTextBlock.Text = clientListView.Items.Count.ToString();
        }

        private void UpdateTotalNoteCount()
        {
            totalNoteCountTextBlock.Text = new BeautySalonModel().Clients.Count().ToString();
        }

        private void DisplayItemPerPage()
        {
            pageNumberTextBlock.Text = CurrentPage.ToString();
            clientListView.ItemsSource = _filteredList
                .Skip((CurrentPage - 1) * _itemPerPage)
                .Take(_itemPerPage)
                .ToList();

            UpdateNoteCount();
        }

        private void SetSearchParameters()
        {
            var searchParameters = ClientList
                .Where(
                    client =>
                        $"{client.FirstName} {client.LastName} {client.Patronymic} {client.Email} {client.Phone}".Contains(
                            _searchParameter
                        )
                )
                .Where(
                    client =>
                        client.GenderCode == _filterParameter.ToString() || _filterParameter == 0
                );

            if (_sortParameter == 0)
                searchParameters = searchParameters.OrderBy(client => client.LastName);

            if (_sortParameter == 1)
                searchParameters = searchParameters.OrderByDescending(client => client.LastVisit);

            if (_sortParameter == 2)
                searchParameters = searchParameters.OrderByDescending(client => client.CountVisit);

            _filteredList = searchParameters.ToList();

            ResetPage();
            DisplayItemPerPage();
        }

        private void ComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (itemPerPageComboBox.SelectedIndex == 0)
                pageSelectorStackPanel.Visibility = Visibility.Collapsed;
            else
                pageSelectorStackPanel.Visibility = Visibility.Visible;

            switch (itemPerPageComboBox.SelectedIndex)
            {
                case 0:
                    _itemPerPage = ClientList.Count();
                    break;
                case 1:
                    _itemPerPage = 10;
                    break;
                case 2:
                    _itemPerPage = 50;
                    break;
                case 3:
                    _itemPerPage = 200;
                    break;
            }

            ResetPage();
            DisplayItemPerPage();
        }

        private void ChangeButtonState()
        {
            switch (_state)
            {
                case ButtonState.Show:
                    _state = ButtonState.Hide;
                    currentMounthBirthdayButton.Content = "Скрыть ДР за текущий месяц";
                    ShowBirthdayonCurrentMounth();
                    break;
                case ButtonState.Hide:
                    _state = ButtonState.Show;
                    currentMounthBirthdayButton.Content = "Показать ДР на текуший месяц";
                    HideBirthdayonCurrentMounth();
                    break;
            }
        }

        private void ShowBirthdayonCurrentMounth()
        {
            clientListView.ItemsSource = _currentBirthdayClientList;
        }

        private void HideBirthdayonCurrentMounth()
        {
            clientListView.ItemsSource = _filteredList;
        }

        private void SearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            _searchParameter = searchTextBox.Text;
            SetSearchParameters();
        }

        private void SortComboBoxDropDownClosed(object sender, EventArgs e)
        {
            _sortParameter = sortComboBox.SelectedIndex;
            SetSearchParameters();
        }

        private void FilterComboBoxDropDownClosed(object sender, EventArgs e)
        {
            _filterParameter = filterComboBox.SelectedIndex;
            SetSearchParameters();
        }

        private void PreviousButtonPageClick(object sender, RoutedEventArgs e)
        {
            CurrentPage -= 1;
        }

        private void NextButtonPageClick(object sender, RoutedEventArgs e)
        {
            CurrentPage += 1;
        }

        private void addClientButton_Click(object sender, RoutedEventArgs e)
        {
            AddClientWindow addclientWindow = new AddClientWindow();
            addclientWindow.ShowDialog();
            if ((bool)addclientWindow.DialogResult)
            {
                SetSearchParameters();
                UpdateTotalNoteCount();
            }
        }

        private void editClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (clientListView.SelectedItem == null)
                return;

            EditClientWindow editClientWindow = new EditClientWindow(
                new BeautySalonModel().Clients.Find(((Client)clientListView.SelectedItem).ID)
            );

            editClientWindow.ShowDialog();
            if ((bool)editClientWindow.DialogResult)
            {
                SetSearchParameters();
                UpdateTotalNoteCount();
            }
        }

        private void deleteClientButton_Click(object sender, RoutedEventArgs e)
        {
            var dbContext = new BeautySalonModel();

            if (clientListView.SelectedItem == null)
                return;

            var selectedClient = dbContext.Clients.FirstOrDefault(
                client => ((Client)clientListView.SelectedItem).ID == client.ID
            );

            if (selectedClient == null)
                return;

            if (selectedClient.CountVisit != 0)
            {
                MessageBox.Show("Удаление данного клиента не возможно");
                return;
            }

            dbContext.Clients.Remove(selectedClient);
            dbContext.SaveChanges();

            SetSearchParameters();
            UpdateTotalNoteCount();
        }

        private void currentMounthBirthdayButton_Click(object sender, RoutedEventArgs e)
        {
            ChangeButtonState();
        }

        private void clientListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (clientListView.SelectedItem == null)
                return;

            new ClientVisitInfoWindow(clientListView.SelectedItem as Client).ShowDialog();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            itemPerPageComboBox.SelectedIndex = 0;
        }
    }
}
