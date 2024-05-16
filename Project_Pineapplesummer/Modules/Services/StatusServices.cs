using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Project_Pineapplesummer.Modules.Services
{
    class StatusServices
    {
        internal async void SetStatus(DiscordSocketClient client)
        {
            DataTable Tbl = new SqlServices.SqlServices().FindData("SELECT * FROM Status");

            while (true)
            {
                int random = new Random().Next(0, Tbl.Rows.Count);

                ActivityType activity;

                switch(Convert.ToInt32(Tbl.Rows[random][1]))
                {
                    case 0:
                        activity = ActivityType.Playing;
                        break;
                    case 1:
                        activity = ActivityType.Streaming;
                        break;
                    case 2:
                        activity = ActivityType.Listening;
                        break;
                    case 3:
                        activity = ActivityType.Watching;
                        break;
                    default:
                        activity = ActivityType.CustomStatus;
                        break;
                }

                string streamurl = "";

                if (!Tbl.Rows[random][2].Equals(DBNull.Value))
                    streamurl = (string)Tbl.Rows[random][2];

                await client.SetGameAsync((string)Tbl.Rows[random][0], streamurl, activity);

                await Task.Delay(20000);
            }
        }
    }
}
