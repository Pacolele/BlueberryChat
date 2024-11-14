using ChatApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Model
{
    public class Message
    {
        public Message()
        {
            dateTime = DateTime.Now;
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd");
            IsReceiver = true;
        }
        public string RequestType { get; set; }
        public string MessageContent { get; set; }
        public string ServerName { get; set; }
        public string ClientName { get; set; }
        public bool IsReceiver { get; set; }

        public string SenderColor { get; set; } 
        public DateTime dateTime { get; set; }

        public string Timestamp { get; set; }
        public string FormattedTimestamp
        {
            get
            {
                TimeSpan timeSince = DateTime.Now - dateTime;

                if (timeSince.TotalMinutes < 1)
                {
                    return "Just now";
                }
                else if (timeSince.TotalHours < 1)
                {
                    int minutes = (int)timeSince.TotalMinutes;
                    return $"{minutes} minute{(minutes != 1 ? "s" : "")} ago";
                }
                else if (timeSince.TotalDays < 1)
                {
                    int hours = (int)timeSince.TotalHours;
                    return $"{hours} hour{(hours != 1 ? "s" : "")} ago";
                }
                else if (timeSince.TotalDays < 30)
                {
                    int days = (int)timeSince.TotalDays;
                    return $"{days} day{(days != 1 ? "s" : "")} ago";
                }
                else if (timeSince.TotalDays < 365)
                {
                    int months = (int)(timeSince.TotalDays / 30);
                    return $"{months} month{(months != 1 ? "s" : "")} ago";
                }
                else
                {
                    int years = (int)(timeSince.TotalDays / 365);
                    return $"{years} year{(years != 1 ? "s" : "")} ago";
                }
            }
        }
    }

   
}