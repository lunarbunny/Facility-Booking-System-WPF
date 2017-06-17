using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace AssignmentS2P2
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private Authentication auth;
        internal static MainWindow session;
        internal static LoginWindow login;
        private BookingSystemDBEntities context;

        public LoginWindow()
        {
            InitializeComponent();
            textBoxUsernameField.Focus();
        }

        private void buttonLogin_Click(object sender, RoutedEventArgs e) // Login button
        {
            using (context = new BookingSystemDBEntities()) // Check if database exist
            {
                if (!context.Database.Exists())
                {
                    MessageBoxResult r = MessageBox.Show("Database \"BookingSystemDB\" doesn't exist.\r\nCreate database now?", "Database", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (r.Equals(MessageBoxResult.Yes))
                        buttonCreateAndPopulateDB_Click(this, new RoutedEventArgs());
                    return;
                }
            }

            auth = new Authentication(textBoxUsernameField.Text, textBoxPasswordField.Password);
            bool[] loginParameters = auth.Login();
            if (loginParameters[0]) // Logged in
            {
                MessageBox.Show(String.Format("Welcome!{0}You have logged in as {1}!", Environment.NewLine, textBoxUsernameField.Text),
                    "Authentication", MessageBoxButton.OK, MessageBoxImage.Information);
                session = new MainWindow(this.textBoxUsernameField.Text, loginParameters[1]);
                session.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("The username or password you entered is incorrect.\r\nPlease try again.", "Authentication", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void buttonRegister_Click(object sender, RoutedEventArgs e) // Register button
        {
            using (context = new BookingSystemDBEntities()) // Check if database exist
            {
                if (!context.Database.Exists())
                {
                    MessageBoxResult r = MessageBox.Show("Database \"BookingSystemDB\" doesn't exist.\r\nCreate database now?", "Database", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (r.Equals(MessageBoxResult.Yes))
                        buttonCreateAndPopulateDB_Click(this, new RoutedEventArgs());
                    return;
                }
            }

            if ((String.IsNullOrEmpty(textBoxUsernameField.Text)) || (String.IsNullOrEmpty(textBoxPasswordField.Password)))
            {
                MessageBox.Show("You cannot register using empty username or passwords!", "Registration", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            auth = new Authentication(textBoxUsernameField.Text, textBoxPasswordField.Password);
            if (auth.Register() == true)
            {
                MessageBox.Show(String.Format("You have successfully registered as {0}!{1}You can now login with your new account.",
                    textBoxUsernameField.Text, Environment.NewLine), "Registration", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                MessageBox.Show(String.Format("The username \"{0}\" is already taken!\r\n Please use another username.", textBoxUsernameField.Text), "Registration", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void textBoxPasswordField_KeyDown(object sender, KeyEventArgs e) // Enter key in password textbox = click on login button
        {
            if (e.Key == Key.Enter)
                buttonLogin_Click(this, new RoutedEventArgs());
        }

        private void Window_Closed(object sender, EventArgs e) // Window Closed
        {
            Application.Current.Shutdown();
        }

        // ==================== Advanced Controls Handler Methods ====================
        private void buttonAutoLoginAdmin_Click(object sender, RoutedEventArgs e) // Auto Login as admin
        {
            this.textBoxUsernameField.Text = "admin";
            this.textBoxPasswordField.Password = "admin";
            buttonLogin_Click(this, new RoutedEventArgs());
        }

        private void buttonAutoLoginUser_Click(object sender, RoutedEventArgs e) // Auto Login as user
        {
            this.textBoxUsernameField.Text = "user";
            this.textBoxPasswordField.Password = "user";
            buttonLogin_Click(this, new RoutedEventArgs());
        }

        private void buttonCreateAndPopulateDB_Click(object sender, RoutedEventArgs e) // Create and populate Database for booking system
        {
            MessageBoxResult r = MessageBox.Show("This will CREATE and POPULATE a database called \"BookingSystemDB\" in SQL Server Management Studio." + Environment.NewLine + Environment.NewLine + "Continue?", "SQL Database", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (r.Equals(MessageBoxResult.OK))
            {
                using (context = new BookingSystemDBEntities())
                {
                    if (context.Database.Exists())
                    {
                        MessageBox.Show("Database \"BookingSystemDB\" already exist.", "SQL Database", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    string sqlConnectionString = @"Server=.\SQLEXPRESS;database=master;Integrated security=True";
                    string createAndPopulateScript = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SQLScripts", "GenerateDBScript.sql"));
                    using (SqlConnection conn = new SqlConnection(sqlConnectionString))
                    {
                        Server server = new Server(new ServerConnection(conn));
                        server.ConnectionContext.ExecuteNonQuery(createAndPopulateScript);
                    }

                    MessageBox.Show("Database \"BookingSystemDB\" has been created and populated.\r\nThe default connection string for Entity Framework points to (local)\\SQLEXPRESS with database name as mentioned above and should not require any config unless database name is different.", "SQL Database", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void buttonOpenSQLFile_Click(object sender, RoutedEventArgs e) // Open raw sql file for create and populate Database
        {
            MessageBoxResult r = MessageBox.Show("Open raw sql query file for create and populate database?", "SQL Database", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (r.Equals(MessageBoxResult.OK))
                System.Diagnostics.Process.Start(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SQLScripts", "GenerateDBScript.sql"));
        }
    }
}