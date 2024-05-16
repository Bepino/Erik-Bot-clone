using Discord.Commands;
using Project_Pineapplesummer.Modules.Services;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Project_Pineapplesummer.Modules.Command_s
{
    [RequireOwner]
    public class Service : ModuleBase<SocketCommandContext>
    {
        SqlConnection sqlConnection = new SqlConnection("Server = phantom; Database=Joman;Trusted_Connection=True;");

        [Command("nQuery")]
        [Alias("nqy")]
        public async Task NonQueryDb([Remainder] string query)
        {
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                sqlConnection.Open();
                try
                {
                    await Context.Channel.SendMessageAsync($"{command.ExecuteNonQuery()} row(s) affected");
                }
                catch(Exception ex)
                {
                    ErrorServices es = new ErrorServices();
                    await es.SendErrorMessage(ex.Message, "NQUERY", Context.Channel, ErrorServices.severity.DB_Error);
                }
                sqlConnection.Close();
            }
        }

        [Command("sQuery")]
        [Alias("Sqy")]
        public async Task ScalarQueryDb([Remainder] string query)
        {
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                sqlConnection.Open();
                try
                {
                    await Context.Channel.SendMessageAsync($"DB returned : {command.ExecuteScalar()}");
                }
                catch (Exception ex)
                {
                    ErrorServices es = new ErrorServices();
                    await es.SendErrorMessage(ex.Message, "SCALAR", Context.Channel, ErrorServices.severity.DB_Error);
                }
                sqlConnection.Close();
            }
        }

        [Command("Query")]
        [Alias("Qy")]
        public async Task QueryDb([Remainder] string query)
        {
            DataTable data = new DataTable();
            string response = "DB returned : \n";

            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                sqlConnection.Open();
                try
                {
                    using (SqlDataReader e = command.ExecuteReader())
                    {
                        data.Load(e);
                    }
                }
                catch (Exception ex)
                {
                    ErrorServices es = new ErrorServices();
                    await es.SendErrorMessage(ex.Message, "QUERY", Context.Channel, ErrorServices.severity.DB_Error);
                }
                sqlConnection.Close();
            }

            for (int i = 0; i < data.Rows.Count; i++)
            {
                response += $"[{i}]";
                for(int j = 0; j < data.Columns.Count; j++)
                {
                    response += $" {data.Rows[i][j]} /";
                }
                response += "\n";
            }

            await Context.Channel.SendMessageAsync(response);
        }
    }
}
