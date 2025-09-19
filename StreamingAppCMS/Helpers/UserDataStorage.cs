using StreamingAppCMS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace StreamingAppCMS.Helpers
{
    public static class UserDataStorage
    {
        private static readonly string basePath = AppDomain.CurrentDomain.BaseDirectory; // bin/Debug/
        private static readonly string filePath = Path.Combine(basePath, "users.xml"); // bin/Debug/users.xaml 

        public static void SaveUsers(List<User> users)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(fs, users);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving users: " + ex.Message);
            }
        }

        public static List<User> LoadUsers()
        {
            if (!File.Exists(filePath))
                return new List<User>();
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    return (List<User>)serializer.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading users: " + ex.Message);
            }
        }

        public static void InitializeDefaultUsers()
        {
            List<User> defaultUsers = new List<User>
            {
                new User("admin", "admin123", UserRole.Admin),
                new User("visitor", "visitor123", UserRole.Visitor)
            };
            SaveUsers(defaultUsers);
        }

        public static void EnsureDataExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                InitializeDefaultUsers();
            }
        }
    }
}
