using System;

namespace StreamingAppCMS.Models
{
    [Serializable]
    public class StreamingApp
    {
        public int NumOfUsers { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public string DescriptionPath { get; set; }
        public DateTime DateAdded { get; set; }
        public StreamingApp() { }

        public StreamingApp(int numOfUsers, string name, string imagePath, string descriptionPath, DateTime dateAdded)
        {
            NumOfUsers = numOfUsers;
            Name = name;
            ImagePath = imagePath;
            DescriptionPath = descriptionPath;
            DateAdded = dateAdded;
        }
    }
}
