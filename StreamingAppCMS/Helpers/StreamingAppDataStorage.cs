using StreamingAppCMS.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace StreamingAppCMS.Helpers
{
    public static class StreamingAppDataStorage
    {
        private static readonly string basePath = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string filePath = Path.Combine(basePath, "streamingApps.xml");

        public static void SaveApps(List<StreamingApp> apps)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<StreamingApp>));
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    serializer.Serialize(fs, apps);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving data: " + ex.Message);
            }
        }

        public static List<StreamingApp> LoadApps(string filePath)
        {
            if (!File.Exists(filePath))
                return new List<StreamingApp>();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<StreamingApp>));
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    return (List<StreamingApp>)serializer.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message);
            }
        }

        public static void AddApp(StreamingApp newApp)
        {
            try
            {
                List<StreamingApp> apps = LoadApps(filePath);
                apps.Add(newApp);
                SaveApps(apps);
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding app: " + ex.Message);
            }
        }

        

        public static void InitializeDefaultData()
        {
            List<StreamingApp> defaultApps = new List<StreamingApp>
            {
                new StreamingApp
                {
                    NumOfUsers = 1000000,
                    Name = "Twitch",
                    ImagePath = "Assets/Images/twitchLogo.png",
                    DescriptionPath = "Assets/Descriptions/twitchDescription.rtf",
                    DateAdded = DateTime.Now
                },
                new StreamingApp
                {
                    NumOfUsers = 750000,
                    Name = "YouTube Gaming",
                    ImagePath = "Assets/Images/youtubeGamingLogo.png",
                    DescriptionPath = "Assets/Descriptions/youtubeGamingDescription.rtf",
                    DateAdded = DateTime.Now
                },
                new StreamingApp
                {
                    NumOfUsers = 300000,
                    Name = "Kick",
                    ImagePath = "Assets/Images/kickLogo.png",
                    DescriptionPath = "Assets/Descriptions/kickDescription.rtf",
                    DateAdded = DateTime.Now
                },
                new StreamingApp
                {
                    NumOfUsers = 650000,
                    Name = "Nimo TV",
                    ImagePath = "Assets/Images/nimoTVLogo.png",
                    DescriptionPath = "Assets/Descriptions/nimoTVDescription.rtf",
                    DateAdded = DateTime.Now
                }
            };

            SaveApps(defaultApps);
        }

        public static void EnsureDataExists()
        {
            if (!File.Exists(filePath))
            {
                InitializeDefaultData();
            }
        }
    }
}