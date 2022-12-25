using BeautySalon.Data;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BeautySalon.Presentation.Windows
{
    /// <summary>
    /// Interaction logic for EditClientWindow.xaml
    /// </summary>
    public partial class EditClientWindow : Window
    {
        private string _photoPath = "";
        private Client _client = null;

        private bool IsFieldValid()
        {
            if (
                String.IsNullOrEmpty(firstNameTextBox.Text)
                || String.IsNullOrEmpty(lastNameTextBox.Text)
                || String.IsNullOrEmpty(patronymicNameTextBox.Text)
            )
            {
                MessageBox.Show("Проверьте фамилию, имя или отчество");
                return false;
            }

            if (!IsEmailValid(emailTextBox.Text))
            {
                MessageBox.Show("Проверьте почту");
                return false;
            }

            if (!IsPhoneValid(phoneTextBox.Text))
            {
                MessageBox.Show("Проверьте телефон");
                return false;
            }

            if (!DateTime.TryParse(birthdayTextBox.Text, out DateTime result))
            {
                MessageBox.Show("Проверьте доту рождения");
                return false;
            }

            if (String.IsNullOrEmpty(_photoPath))
            {
                MessageBox.Show("Выберите изображение");
                return false;
            }

            return true;
        }

        private bool IsEmailValid(string email) => new Regex(@"^\w+@\w+.\w+$").IsMatch(email);

        private bool IsPhoneValid(string phoneNumber) => new Regex(@"^\d+$").IsMatch(phoneNumber);

        public EditClientWindow(Client client)
        {
            _client = client;
            InitializeComponent();
            FillInputField();

            tagListView.ItemsSource = new BeautySalonModel().Tags.ToList();
        }

        private void FillInputField()
        {
            try
            {
                idTextBox.Text = _client.ID.ToString();
                lastNameTextBox.Text = _client.LastName;
                firstNameTextBox.Text = _client.FirstName;
                patronymicNameTextBox.Text = _client.Patronymic;
                emailTextBox.Text = _client.Email;
                phoneTextBox.Text = _client.Phone;
                birthdayTextBox.Text = _client.Birthday.Value.ToString("dd.MM.yyyy");
                genderComboBox.SelectedIndex = Convert.ToInt32(_client.Gender.Code) - 1;
                clientLogoImage.Source = new BitmapImage(new Uri(_client.PhotoPath));

                _photoPath = _client.PhotoPath;
            }
            catch (Exception) { }
        }

        private void selectClientImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.Filter = "Image Files(*.jpg)|*.jpg;";

            if (fileDialog.ShowDialog() == true)
            {
                _photoPath = fileDialog.FileName;
                clientLogoImage.Source = new BitmapImage(new Uri(_photoPath));
            }
        }

        private void editClientButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFieldValid())
                return;

            try
            {
                var dbContext = new BeautySalonModel();
                var fundedClient = dbContext.Clients.FirstOrDefault(
                    client => client.ID == _client.ID
                );
                if (fundedClient == null)
                    return;

                fundedClient.FirstName = firstNameTextBox.Text;
                fundedClient.LastName = lastNameTextBox.Text;
                fundedClient.Patronymic = patronymicNameTextBox.Text;
                fundedClient.Birthday = Convert.ToDateTime(birthdayTextBox.Text);
                fundedClient.RegistrationDate = DateTime.Now;
                fundedClient.Email = emailTextBox.Text;
                fundedClient.Phone = phoneTextBox.Text;
                fundedClient.GenderCode = (genderComboBox.SelectedIndex + 1).ToString();
                fundedClient.PhotoPath = _photoPath;

                var tags = tagListView?.SelectedItems.OfType<Tag>().ToList();
                foreach (var item in tags)
                {
                    var foundedTag = dbContext.Tags.Find(item.ID);
                    if (foundedTag == null)
                        continue;

                    if (fundedClient.Tags.Any(x => item.ID == x.ID))
                        continue;

                    fundedClient.Tags.Add(foundedTag);
                }

                dbContext.SaveChanges();

                MessageBox.Show("Клиент отредактирован добавлен");
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FullNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.Any(
                symbol => Char.IsLetter(symbol) || symbol == ' ' || symbol == '-'
            );
        }

        private void phoneTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.Any(symbol => Char.IsDigit(symbol));
        }
    }
}
