using System;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;
using WebAuth.Models;

namespace WebAuth.Services
{
    public class AuthenticationService
    {
        private readonly List<User> users;
        private readonly string PROJECT_FILE;
        private readonly string ADMIN_FILE;
        private const int ITERATION_COUNT = 10000;
        private const int HASH_SIZE = 32;

        public AuthenticationService(IHostEnvironment hostEnvironment)
        {
            users = [];

            // Use content root path for project file
            PROJECT_FILE = Path.Combine(hostEnvironment.ContentRootPath, "users.txt");

            // AppData file path
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WebAuth"
            );
            Directory.CreateDirectory(appDataPath);
            ADMIN_FILE = Path.Combine(appDataPath, "admin_users.txt");

            EnsureFilesExist();
            LoadUsers();
        }

        private void EnsureFilesExist()
        {
            try
            {
                // Create project directory if it doesn't exist
                var projectDir = Path.GetDirectoryName(PROJECT_FILE);
                if (!string.IsNullOrEmpty(projectDir))
                {
                    Directory.CreateDirectory(projectDir);
                }

                if (!File.Exists(PROJECT_FILE))
                {
                    File.WriteAllText(PROJECT_FILE, "", Encoding.UTF8);
                }
                if (!File.Exists(ADMIN_FILE))
                {
                    File.WriteAllText(ADMIN_FILE, "", Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not create project file: {ex.Message}");
            }
        }

        public void SaveUsers()
        {
            try
            {
                var lines = users.Select(u => $"{u.Username}:{u.Salt}:{u.HashedPassword}").ToList();

                // Always save to admin file
                File.WriteAllLines(ADMIN_FILE, lines, Encoding.UTF8);

                // Try to save to project file, but don't fail if it's not possible
                try
                {
                    File.WriteAllLines(PROJECT_FILE, lines, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not save to project file: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving users: {ex.Message}");
            }
        }

        public void LoadUsers()
        {
            users.Clear();
            string fileToLoad = File.Exists(PROJECT_FILE) ? PROJECT_FILE : ADMIN_FILE;

            if (!File.Exists(fileToLoad)) return;

            var lines = File.ReadAllLines(fileToLoad);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 3)
                {
                    users.Add(new User
                    {
                        Username = parts[0],
                        Salt = parts[1],
                        HashedPassword = parts[2]
                    });
                }
            }
        }

        public void SignUp(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password are required.");

            if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("Username already exists.");

            if (!IsPasswordValid(password))
                throw new ArgumentException("Password does not meet requirements.");

            byte[] saltBytes = new byte[16];
            RandomNumberGenerator.Fill(saltBytes);
            string salt = Convert.ToBase64String(saltBytes);
            string hashedPassword = HashPassword(password, salt);

            users.Add(new User
            {
                Username = username,
                Salt = salt,
                HashedPassword = hashedPassword
            });

            SaveUsers();
        }

        public bool SignIn(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var user = users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user == null) return false;

            string hashedPassword = HashPassword(password, user.Salt);
            return hashedPassword == user.HashedPassword;
        }

        private static string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using Rfc2898DeriveBytes pbkdf2 = new(
                password,
                saltBytes,
                ITERATION_COUNT,
                HashAlgorithmName.SHA256);
            {
                byte[] hashBytes = pbkdf2.GetBytes(HASH_SIZE);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private static bool IsPasswordValid(string password)
        {
            return password.Length >= 8 &&
                   password.Any(char.IsUpper) &&
                   password.Any(char.IsLower) &&
                   password.Any(char.IsDigit) &&
                   password.Any(c => !char.IsLetterOrDigit(c));
        }

        public List<string> GetAllUsernames()
        {
            return users.Select(u => u.Username).ToList();
        }

        public void DeleteUser(string username)
        {
            var user = users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (user != null)
            {
                users.Remove(user);
                SaveUsers();
            }
        }
    }
}