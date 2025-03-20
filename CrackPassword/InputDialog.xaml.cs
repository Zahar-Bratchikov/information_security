using System.Windows;

namespace CrackPassword
{
    // Диалоговое окно для ввода значения (например, имя нового пользователя)
    public partial class InputDialog : Window
    {
        // Ответ пользователя
        public string ResponseText { get; private set; }

        public InputDialog(string prompt, string title = "Input")
        {
            InitializeComponent();
            lblPrompt.Text = prompt;
            Title = title;
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = txtResponse.Text;
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}