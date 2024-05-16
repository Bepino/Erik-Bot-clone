using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Project_Pineapplesummer.Modules.Services
{
    public class BankingServices
    {
        SqlServices.SqlServices sqlServices = new SqlServices.SqlServices();

        public async Task CreateAccount(ulong userid)
        {
            if (AccountExitsts(userid))
            {
                await new ErrorServices().SendErrorMessage("User already has a valid account", "BS - CA0x1 (1023)", ErrorServices.severity.Error);
                return;
            }

            using(SqlCommand sqlCommand = new SqlCommand("", sqlServices.sqlConnection))
            {
                sqlCommand.CommandText = "INSERT INTO Bank (AccountId) VALUES (@uId)";
                sqlCommand.Parameters.AddWithValue("@uId", Convert.ToInt64(userid));

                sqlCommand.Connection.Open();
                try
                { sqlCommand.ExecuteNonQuery(); }
                catch (Exception ex)
                {
                    await new ErrorServices().SendErrorMessage(ex.Message, "BS - CA01 (1021)", ErrorServices.severity.Error);
                }
                sqlCommand.Connection.Close();
            }

            await AddToAccount(100, userid);
        }

        public async Task AddToAccount(int amount, ulong userid)
        {
            if (amount < 0 || amount > int.MaxValue)
            {
                await new ErrorServices().SendErrorMessage("Invalid amount", "BS - ATA0x1 (1003)", ErrorServices.severity.Error);
                return;
            }

            if (!AccountExitsts(userid))
            { 
                await new ErrorServices().SendErrorMessage("User doesn't have a valid account", "BS - ATA0x2 (1004)", ErrorServices.severity.Error);
                return;
            }

            using(SqlCommand sqlCommand = new SqlCommand("", sqlServices.sqlConnection))
            { 
                int balance = 0;
                sqlCommand.CommandText = "SELECT Balance FROM Bank WHERE AccountId = @uId";
                sqlCommand.Parameters.AddWithValue("@uId", Convert.ToInt64(userid));

                sqlCommand.Connection.Open();
                try
                { 
                    var temp = sqlCommand.ExecuteScalar();

                    if (temp.Equals(DBNull.Value))
                        balance = 0;
                    else
                        balance = Convert.ToInt32(temp);
                }
                catch (Exception ex)
                {
                    await new ErrorServices().SendErrorMessage(ex.Message, "BS - ATA01 (1001)", ErrorServices.severity.Error);
                }

                sqlCommand.CommandText = "UPDATE Bank SET Balance = @bal WHERE AccountId = @uId";
                sqlCommand.Parameters.AddWithValue("@bal", balance + amount);


                try
                { sqlCommand.ExecuteNonQuery(); }
                catch(Exception ex)
                {
                    await new ErrorServices().SendErrorMessage(ex.Message, "BS - ATA02 (1002)", ErrorServices.severity.Error);
                }
                sqlCommand.Connection.Close();
            }

        }

        public async Task RemoveFromAccount(int amount, ulong userid)
        {
            if (amount < 0 || amount > int.MaxValue)
            {
                await new ErrorServices().SendErrorMessage("Invalid amount", "BS - RFA0x1 (1013)", ErrorServices.severity.Error);
                return;
            }

            if (!AccountExitsts(userid))
            {
                await new ErrorServices().SendErrorMessage("User doesn't have a valid account", "BS - RFA0x2 (1014)", ErrorServices.severity.Error);
                return;
            }

            using (SqlCommand sqlCommand = new SqlCommand("", sqlServices.sqlConnection))
            {
                int balance = 0;
                sqlCommand.CommandText = "SELECT Balance FROM Bank WHERE AccountId = @uId";
                sqlCommand.Parameters.AddWithValue("@uId", Convert.ToInt64(userid));

                sqlCommand.Connection.Open();
                try
                {
                    var temp = sqlCommand.ExecuteScalar();

                    if (temp.Equals(DBNull.Value))
                        balance = 0;
                    else
                        balance = Convert.ToInt32(temp);
                }
                catch (Exception ex)
                {
                    await new ErrorServices().SendErrorMessage(ex.Message, "BS - ATA01 (1001)", ErrorServices.severity.Error);
                }

                sqlCommand.CommandText = "UPDATE Bank SET Balance = @bal WHERE AccountId = @uId";
                sqlCommand.Parameters.AddWithValue("@bal", balance - amount);


                try
                { sqlCommand.ExecuteNonQuery(); }
                catch (Exception ex)
                {
                    await new ErrorServices().SendErrorMessage(ex.Message, "BS - ATA02 (1002)", ErrorServices.severity.Error);
                }
                sqlCommand.Connection.Close();
            }

        }

        private bool AccountExitsts(ulong userid)
        {
            if (sqlServices.Contains("Bank", userid, "AccountId"))
                return true;
            else return false;
        }
    }
}
