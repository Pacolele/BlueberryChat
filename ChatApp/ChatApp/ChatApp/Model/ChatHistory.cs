using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ChatApp.Model
{
    public class ChatHistory
    {
      
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Date { get; set; }

   

        public List<Message> chatHistory { get; set; }

   
        public static List<ChatHistory> LoadChatHistories(bool isServer)
        {
            string currentDirectory = Environment.CurrentDirectory;

            // Navigate up to the project root
            string projectRoot = Path.Combine(currentDirectory, "..", "..", "..");

            // Specify the folder and file names
            string folderPath = Path.Combine(projectRoot, "History");
            string filePath;

            if (isServer)
            {
                filePath = Path.Combine(folderPath, "ChatHistoryServer.txt");

            }
            else
            {
                filePath = Path.Combine(folderPath, "ChatHistoryClient.txt");
            }


            // Check if the file exists
            if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
            {
                // Read the JSON data from the file
                string json = File.ReadAllText(filePath);

                // Deserialize the JSON data back into a List<ChatHistory>
                return JsonConvert.DeserializeObject<List<ChatHistory>>(json);
            }

            // Return an empty list if the file doesn't exist
            return new List<ChatHistory>();
        }

        public void SaveChatHistory(bool isServer)
        {
            string json;

            // Check if the file already exists
            List<ChatHistory> existingChatHistories = LoadChatHistories(isServer);

            Console.WriteLine("THIS OBJECT: " + this);
            existingChatHistories.Add(this); // Add the current instance to the list
            Console.WriteLine("Saving...");

            // Serialize the list of ChatHistory instances
            json = JsonConvert.SerializeObject(existingChatHistories);

            // Get the current working directory
            string currentDirectory = Environment.CurrentDirectory;

            // Navigate up to the project root
            string projectRoot = Path.Combine(currentDirectory, "..", "..", "..");

            // Specify the folder and file names
            string folderPath = Path.Combine(projectRoot, "History");

            string filePath;

            if (isServer)
            {
                filePath = Path.Combine(folderPath, "ChatHistoryServer.txt");
            }
            else
            {
                filePath = Path.Combine(folderPath, "ChatHistoryClient.txt");
            }

            // Create the folder if it doesn't exist
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            if(Receiver != null || Sender != null)
            {
                // Write the JSON data to the file
                File.WriteAllText(filePath, json);
            }
           
        }
    }
}
