using Discord.WebSocket;
using Project_Pineapplesummer.Modules.Services.Classes;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Project_Pineapplesummer.Modules.Services.SqlServices
{
    public class SqlServices
    {
        internal readonly SqlConnection sqlConnection = new SqlConnection("Server = phantom; Database=Joman;Trusted_Connection=True;");
        readonly ErrorServices es = new ErrorServices();

        /// <summary>
        /// Universal Data retrival command
        /// </summary>
        /// <returns></returns>
        internal DataTable FindData(string query, ISocketMessageChannel channel = null)
        {
            using (SqlCommand sqlCommand = new SqlCommand(cmdText: query, connection: sqlConnection))
            {
                DataTable data = new DataTable();

                try
                {
                    sqlCommand.Connection.Open();
                    using (SqlDataReader e = sqlCommand.ExecuteReader())
                    {
                        data.Load(e);
                    }
                    sqlCommand.Connection.Close(); 
                    return data;
                }
                catch (Exception ex)
                {
                    if (channel != null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFdExQ", channel, ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFdExQ", ErrorServices.severity.DB_Error);
                }
            }

            return null;
        }

        /// <summary>
        /// Should not be accept user input
        /// </summary>
        /// <returns><see cref="ulong"/> first field returned by server</returns>
        internal ulong FindData(ulong serverid, string target, string table, ISocketMessageChannel channel = null)
        {
            long sID = Convert.ToInt64(serverid);
            SqlCommand sqlCommand = new SqlCommand($"SELECT {target} FROM {table} WHERE ServerID = {sID}; ", sqlConnection);

            try
            {
                sqlCommand.Connection.Open();
                ulong u = Convert.ToUInt64(sqlCommand.ExecuteScalar());
                sqlCommand.Connection.Close();
                return u;
            }
            catch (Exception ex)
            {
                if (channel != null)
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFdVConn01", channel, ErrorServices.severity.DB_Error);
                else
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFdVConn01", ErrorServices.severity.DB_Error);
            }

            return 0;
        }

        /// <summary>
        /// Used to find DateTime of a query.
        /// </summary>
        /// <returns>the Time column</returns>
        internal DateTime FindData(string query, string target, string table, ISocketMessageChannel channel = null)
        {
            SqlCommand sqlCommand = new SqlCommand($"SELECT {target} FROM {table} WHERE Query = {query}; ", sqlConnection);

            try
            {
                sqlCommand.Connection.Open();
                DateTime u = Convert.ToDateTime(sqlCommand.ExecuteScalar());
                sqlCommand.Connection.Close();
                return u;
            }
            catch (Exception ex)
            {
                if (channel != null)
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFdVConn01", channel, ErrorServices.severity.DB_Error);
                else
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFdVConn01", ErrorServices.severity.DB_Error);
            }

            return DateTime.UnixEpoch;
        }

        internal string GetPrefix(string serverid, ISocketMessageChannel channel)
        {
            string prefix = ".";

            if(Contains("ServerSettings", serverid, "ServerId"))
            {
                using (SqlCommand sqlCommand = new SqlCommand($"SELECT Prefix FROM ServerSettings WHERE ServerId = {serverid}", sqlConnection))
                {
                    sqlConnection.Open();
                    try
                    {
                        prefix = (string)sqlCommand.ExecuteScalar();
                    }
                    catch(Exception ex)
                    {
                        if (channel != null)
                            _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFdVConn01", channel, ErrorServices.severity.DB_Error);
                        else
                            _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFdVConn01", ErrorServices.severity.DB_Error);
                    }
                    sqlConnection.Close();
                }
            }

            return prefix;
        }

        /// <summary>
        /// Finds <c>target</c> in <c>table</c> where the specifed ServerID, UserID and Time columns match the provided user informtion (No user input)
        /// </summary>
        /// <returns><see cref="int"/> first field from the target column</returns>
        internal int FindData(ulong serverId, ulong userId, DateTime time, string target, string table, ISocketMessageChannel channel = null)
        {
            SqlCommand sqlCommand = new SqlCommand($"SELECT {target} FROM {table} WHERE ServerID = @sID AND UserID = @uID AND Time = @tim; ", sqlConnection);
            sqlCommand.Parameters.AddWithValue("@sID", Convert.ToInt64(serverId));
            sqlCommand.Parameters.AddWithValue("@uID", Convert.ToInt64(userId));
            sqlCommand.Parameters.AddWithValue("@tim", time);

            try
            {
                sqlCommand.Connection.Open();
                int u = (int)sqlCommand.ExecuteScalar();
                sqlCommand.Connection.Close();
                return u;
            }
            catch (Exception ex)
            {
                if (channel != null)
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFd003", channel, ErrorServices.severity.DB_Error);
                else
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFd003", ErrorServices.severity.DB_Error);
            }

            return 0;
        }


        /// <returns>If field is not found, it will be <c>null</c></returns>
        internal BanData GetBan(int caseId, ISocketMessageChannel channel = null)
        {
            using (SqlCommand sqlCommand = new SqlCommand($"SELECT * FROM Bans WHERE Id = @ID", sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@ID", caseId);

                try
                {
                    DataTable tbl = new DataTable();
                    sqlCommand.Connection.Open();
                    using (SqlDataReader e = sqlCommand.ExecuteReader())
                    {
                        tbl.Load(e);
                    }
                    sqlCommand.Connection.Close();

                    BanData data = new BanData();

                    if (!tbl.Rows[0][0].Equals(DBNull.Value) || tbl.Rows[0][0] == null)
                        data.CaseId = Convert.ToInt32(tbl.Rows[0][0]);

                    if (!tbl.Rows[0][1].Equals(DBNull.Value) || tbl.Rows[0][1] == null)
                        data.ServerId = Convert.ToUInt64(tbl.Rows[0][1]);

                    if (!tbl.Rows[0][2].Equals(DBNull.Value) || tbl.Rows[0][2] == null)
                        data.UserId = Convert.ToUInt64(tbl.Rows[0][2]);

                    if (!tbl.Rows[0][3].Equals(DBNull.Value) || tbl.Rows[0][3] == null)
                        data.ModeratorId = Convert.ToUInt64(tbl.Rows[0][3]);

                    if (!tbl.Rows[0][4].Equals(DBNull.Value) || tbl.Rows[0][4] == null)
                        data.Reason = Convert.ToString(tbl.Rows[0][4]);

                    if (!tbl.Rows[0][5].Equals(DBNull.Value) || tbl.Rows[0][5] == null)
                        data.Time = Convert.ToDateTime(tbl.Rows[0][5]);

                    if (!tbl.Rows[0][6].Equals(DBNull.Value) || tbl.Rows[0][6] == null)
                        data.ExpirationTime = Convert.ToDateTime(tbl.Rows[0][6]);

                    if (!tbl.Rows[0][7].Equals(DBNull.Value) || tbl.Rows[0][7] == null)
                        data.Revoke.ModeratorId = Convert.ToUInt64(tbl.Rows[0][7]);

                    if (!tbl.Rows[0][8].Equals(DBNull.Value) || tbl.Rows[0][8] == null)
                        data.Revoke.Time = Convert.ToDateTime(tbl.Rows[0][8]);

                    if (!tbl.Rows[0][9].Equals(DBNull.Value) || tbl.Rows[0][9] == null)
                        data.Revoke.Reason = Convert.ToString(tbl.Rows[0][9]);

                    if (!tbl.Rows[0][10].Equals(DBNull.Value) || tbl.Rows[0][10] == null)
                        data.Revoke.IsRevoked = Convert.ToBoolean(tbl.Rows[0][10]);

                    return data;
                }
                catch (Exception ex)
                {
                    if (channel != null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFBan", channel, ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFBan", ErrorServices.severity.DB_Error);
                }
            }
            return null;
        }

        /// <summary>
        /// Used to find a caseid of unrevoked ban
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="serverid"></param>
        /// <returns>CaseId for ban</returns>
        internal int FindBan(ulong userid, ulong serverid)
        {
            int caseid = 0;

            DataTable tbl = FindData($"SELECT * FROM Bans WHERE UserId = {userid} AND ServerId = {serverid}");

            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                if (tbl.Rows[i][10].Equals(DBNull.Value) || !Convert.ToBoolean(tbl.Rows[i][10]))
                {
                    caseid = Convert.ToInt32(tbl.Rows[i][0]);
                    break;
                }
            }

            return caseid;
        }

        internal bool IsBanLogged(SocketUser arg1, SocketGuild arg2)
        {
            using (SqlCommand command = new SqlCommand($"SELECT Time FROM Bans WHERE UserId = {(long)arg1.Id}  AND ServerId = {(long)arg2.Id}", sqlConnection))
            {
                command.Connection.Open();
                DataTable tbl = new DataTable();
                using (SqlDataReader e = command.ExecuteReader())
                {
                    tbl.Load(e);
                }
                command.Connection.Close();

                for (int i = 0; i < tbl.Rows.Count; i++)
                {
                    TimeSpan span = DateTime.Now - Convert.ToDateTime(tbl.Rows[i][0]);

                    if (span < TimeSpan.FromSeconds(5))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal int GetSeverity(ulong serverid)
        {
            long sID = Convert.ToInt64(serverid);
            SqlCommand sqlCommand = new SqlCommand($"SELECT Severity FROM ServerSettings WHERE ServerID = {sID}; ", sqlConnection);

            try
            {
                sqlCommand.Connection.Open();
                int u = Convert.ToInt32(sqlCommand.ExecuteScalar());
                sqlCommand.Connection.Close();
                return u;
            }
            catch (Exception ex)
            {
                _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xFdVConn01", ErrorServices.severity.DB_Error);
            }

            return 0;
        }

        internal Task AddRevoke(ulong moderatorid, string reason, bool isRevoked, int caseid, ISocketMessageChannel channel = null)
        {
            using (SqlCommand sqlCommand = new SqlCommand("UPDATE Bans SET RevokeModID = @mID, RevokeTime = @tim, RevokeReason = @rsn, IsRevoked = @iRv WHERE ID = @id", sqlConnection))
            {
                try
                {
                    sqlCommand.Parameters.AddWithValue("@mID", Convert.ToInt64(moderatorid));
                    sqlCommand.Parameters.AddWithValue("@tim", DateTime.Now);
                    sqlCommand.Parameters.AddWithValue("@rsn", reason);
                    sqlCommand.Parameters.AddWithValue("@iRv", isRevoked);
                    sqlCommand.Parameters.AddWithValue("@id", caseid);

                    sqlCommand.Connection.Open();
                    sqlCommand.ExecuteNonQuery();
                    sqlCommand.Connection.Close();
                }
                catch (Exception ex)
                {
                    if (channel != null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xAdRvk00", channel, ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xAdRvk00", ErrorServices.severity.DB_Error);

                    return Task.FromException(ex);
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Logs message removel into the MessageLog table
        /// </summary>
        /// <returns></returns>
        internal Task LogMessage(ulong messageid, ulong serverid, ulong channelid, ulong moderatorid, ulong authorid, string content, string reason, int amount, int postition, DateTime time, ISocketMessageChannel channel)
        {
            SqlCommand sqlCommand = new SqlCommand($"INSERT INTO MessageLog VALUES (@mID, @sID, @cID, @mod, @aID, @cnt, @rsn, @amt, @pos, @tim)", sqlConnection);
            sqlCommand.Parameters.AddWithValue("@mID", (long)messageid);
            sqlCommand.Parameters.AddWithValue("@sID", (long)serverid);
            sqlCommand.Parameters.AddWithValue("@cID", (long)channelid);
            sqlCommand.Parameters.AddWithValue("@mod", (long)moderatorid);
            sqlCommand.Parameters.AddWithValue("@aID", (long)authorid);
            sqlCommand.Parameters.AddWithValue("@cnt", content);
            sqlCommand.Parameters.AddWithValue("@rsn", reason);
            sqlCommand.Parameters.AddWithValue("@amt", amount);
            sqlCommand.Parameters.AddWithValue("@pos", postition);
            sqlCommand.Parameters.AddWithValue("@tim", time);

            try
            {
                sqlCommand.Connection.Open();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Connection.Close();
            }
            catch (Exception ex)
            {
                sqlCommand.Connection.Close();
                if (channel != null)
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xInsDVconnExQ", channel, ErrorServices.severity.DB_Error);
                else
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xInsDVconnExQ", ErrorServices.severity.DB_Error);
                throw;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Insert new row into VocieConn table ]
        /// </summary>
        /// <returns></returns>
        internal Task InsertData(ulong serverid, ulong voiceid, ISocketMessageChannel channel = null)
        {
            long sID = Convert.ToInt64(serverid);
            long vID = Convert.ToInt64(voiceid);

            SqlCommand sqlCommand = new SqlCommand($"INSERT INTO VoiceConn (ServerID, VoiceID) VALUES ({sID}, {vID})", sqlConnection);

            try
            {
                sqlCommand.Connection.Open();
                sqlCommand.ExecuteNonQuery();
                sqlCommand.Connection.Close();
            }
            catch (Exception ex)
            {
                if (channel != null)
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xInsDVconnExQ", channel, ErrorServices.severity.DB_Error);
                else
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xInsDVconnExQ", ErrorServices.severity.DB_Error);
                throw;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates new row inside Time table for specified server
        /// <para> <see cref="String"/> msg is limited to 255 characters</para>
        /// </summary>
        /// <returns>1 if successful</returns>
        internal int InsertData(ulong serverid, DateTime time, string msg, string user, ISocketMessageChannel channel = null)
        {
            long sID = Convert.ToInt64(serverid);
            long cID = Convert.ToInt64(channel.Id);
            int result = 0;

            using (SqlCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = "INSERT INTO Time VALUES (@sID, @tim, @msg, @cID, @usr)";
                command.Parameters.AddWithValue("@sID", sID);
                command.Parameters.AddWithValue("@tim", time);
                command.Parameters.AddWithValue("@msg", msg);
                command.Parameters.AddWithValue("@cID", cID);
                command.Parameters.AddWithValue("@usr", user);

                command.Connection.Open();
                try
                {
                    result = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    if (channel != null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xInsDVconnExQ", channel, ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xInsDVconnExQ", ErrorServices.severity.DB_Error);
                }
                command.Connection.Close();
            }

            return result;
        }

        /// <summary>
        /// Executes a NonQuery for the <see cref="SqlCommand"/> <c>sqlCommand</c>
        /// </summary>
        /// <returns>1 if successful <c>sqlCommand</c></returns>
        internal int ExQComm(SqlCommand sqlCommand, ISocketMessageChannel channel = null)
        {
            int result = 0;
            sqlCommand.Connection = sqlConnection;
            try
            {
                sqlConnection.Open();
                result = sqlCommand.ExecuteNonQuery();
                sqlConnection.Close();
            }
            catch (Exception ex)
            {
                if (channel != null)
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xExQComm", channel, ErrorServices.severity.DB_Error);
                else
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xExQComm", ErrorServices.severity.DB_Error);
            }

            return result;
        }

        /// <summary>
        /// Inserts data into specified data table
        /// <para><c>table</c> should not accept user input!!!</para>
        /// </summary>
        /// <returns>1 indicating a success, 0 indicating failure</returns>
        internal int InsertData(string table, ulong serverid, ulong userid, string reason, ulong author, ISocketMessageChannel channel = null)
        {
            long sID = Convert.ToInt64(serverid);
            long uID = Convert.ToInt64(userid);
            long aID = Convert.ToInt64(author);
            int result = 0;

            using (SqlCommand command = new SqlCommand("", sqlConnection))
            {
                command.CommandText = $"INSERT INTO {table} VALUES (@sID, @uID, @rsn, @tim, @ath)";
                command.Parameters.AddWithValue("@sID", sID);
                command.Parameters.AddWithValue("@uID", uID);
                command.Parameters.AddWithValue("@rsn", reason);
                command.Parameters.AddWithValue("@tim", DateTime.Now);
                command.Parameters.AddWithValue("@ath", aID);

                command.Connection.Open();
                try
                {
                    result = command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (channel == null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xInsDT00", ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xInsDT00", channel, ErrorServices.severity.DB_Error);
                }
                command.Connection.Close();
            }

            return result;
        }

        /// <summary>
        /// Inserts data into Timeouts table
        /// </summary>
        /// <returns>1 indicating a success, 0 indicating failure</returns>
        internal int InsertData(ulong serverid, ulong userid, ulong moderatorid, string reason, DateTime time, ISocketMessageChannel channel = null)
        {
            long sID = Convert.ToInt64(serverid);
            long uID = Convert.ToInt64(userid);
            long aID = Convert.ToInt64(moderatorid);
            int result = 0;

            using (SqlCommand command = new SqlCommand("", sqlConnection))
            {
                command.CommandText = $"INSERT INTO Timeouts (ServerId, UserId, ModeratorId, Reason, Expiration) VALUES (@sID, @uID, @ath, @rsn, @tim)";
                command.Parameters.AddWithValue("@sID", sID);
                command.Parameters.AddWithValue("@uID", uID);
                command.Parameters.AddWithValue("@ath", aID);
                command.Parameters.AddWithValue("@rsn", reason);
                command.Parameters.AddWithValue("@tim", time);

                command.Connection.Open();
                try
                {
                    result = command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (channel == null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xInsDT00", ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xInsDT00", channel, ErrorServices.severity.DB_Error);
                }
                command.Connection.Close();
            }

            return result;
        }

        /// <summary>
        /// Inserts arg data into <c>Bans</c> table
        /// </summary>
        /// <returns>1 indicating a success, 0 indicating failure</returns>
        internal int InsertData(ulong serverid, ulong userid, ulong author, string reason, DateTime time, double offset, ISocketMessageChannel channel = null)
        {
            int result = 0;

            using (SqlCommand command = new SqlCommand("", sqlConnection))
            {
                command.CommandText = $"INSERT INTO Bans (ServerID, UserID, ModeratorID, Reason, Time, Expiration) VALUES (@sID, @uID, @ath, @rsn, @tim, @exp)";
                command.Parameters.AddWithValue("@sID", Convert.ToInt64(serverid));
                command.Parameters.AddWithValue("@uID", Convert.ToInt64(userid));
                command.Parameters.AddWithValue("@ath", Convert.ToInt64(author));
                command.Parameters.AddWithValue("@rsn", reason);
                command.Parameters.AddWithValue("@tim", time);
                command.Parameters.AddWithValue("@exp", time.AddHours(offset));

                command.Connection.Open();
                try
                {
                    result = command.ExecuteNonQuery();
                }
                catch (SqlException ex)
                {
                    if (channel == null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xInsDTBa00", ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xInsDTBa00", channel, ErrorServices.severity.DB_Error);
                }
                command.Connection.Close();
            }

            return result;
        }

        /// <summary>
        /// Updates VoiceConn table
        /// <para> <see cref="ISocketMessageChannel"/> channel should be used if possible</para>
        /// </summary>
        /// <returns></returns>
        internal Task UpdateData(ulong serverid, ulong voiceid, int IsConn, ISocketMessageChannel channel = null)
        {
            long sID = Convert.ToInt64(serverid);
            long vID = Convert.ToInt64(voiceid);

            using (SqlCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = "UPDATE VoiceConn SET VoiceID = @vID, IsConn = @iCn WHERE ServerID = @sID";
                command.Parameters.AddWithValue("@vID", vID);
                command.Parameters.AddWithValue("@iCn", IsConn);
                command.Parameters.AddWithValue("@sID", sID);

                try
                {
                    sqlConnection.Open();
                    command.ExecuteNonQuery();
                    sqlConnection.Close();
                }
                catch (Exception ex)
                {
                    if (channel != null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xUdvcExQ", channel, ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xUdvcExQ", ErrorServices.severity.DB_Error);
                    throw;
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Chackes JSON string into the table with query used, and DateTime.now
        /// <para>Table structure has to contain[JSON][QUERY][TIME]</para>
        /// </summary>
        /// <returns></returns>
        internal Task CacheJson(string table, string query, string json, ISocketMessageChannel channel)
        {
            int result = 0;
            using (SqlCommand command = sqlConnection.CreateCommand())
            {
                command.CommandText = $"INSERT INTO {table} (Json, Time, Query) VALUES (@jsn, @tim, @qry)";
                command.Parameters.AddWithValue("@jsn", json);
                command.Parameters.AddWithValue("@tim", DateTime.Now);
                command.Parameters.AddWithValue("@qry", query);

                sqlConnection.Open();
                try
                {
                    result = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    if (channel != null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xUdvcExQ", channel, ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xUdvcExQ", ErrorServices.severity.DB_Error);
                }
                sqlConnection.Close();
            }

            if (result > 0)
                return Task.CompletedTask;
            else
                throw new Exception("There were some DB errors caching JSON");
        }

        /// <summary>
        /// Checks if <c>table</c> contains the <c>target</c> with specified <c>value</c>
        /// </summary>
        /// <returns></returns>
        internal bool Contains(string table, ulong value, string target, ISocketMessageChannel channel = null)
        {
            long ID = Convert.ToInt64(value);
            SqlCommand sqlCommand = new SqlCommand($"SELECT COUNT ({target}) FROM {table} WHERE {target} = {ID}", sqlConnection);

            try
            {
                sqlCommand.Connection.Open();
                int rows = (int)sqlCommand.ExecuteScalar();
                sqlCommand.Connection.Close();

                if (rows > 0)
                    return true;
            }
            catch (Exception ex)
            {
                if (channel == null)
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xIhExQ01", ErrorServices.severity.DB_Error);
                else
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xIhExQ01", channel, ErrorServices.severity.DB_Error);
            }

            return false;
        }

        /// <summary>
        /// Checks if <c>table</c> contains the <c>target</c> with specified <c>value</c>
        /// </summary>
        /// <returns></returns>
        internal bool Contains(string table, string value, string target, ISocketMessageChannel channel = null)
        {
            SqlCommand sqlCommand = new SqlCommand($"SELECT COUNT ({target}) FROM {table} WHERE {target} = {value}", sqlConnection);

            try
            {
                sqlCommand.Connection.Open();
                int rows = (int)sqlCommand.ExecuteScalar();
                sqlCommand.Connection.Close();

                if (rows > 0)
                    return true;
            }
            catch (Exception ex)
            {
                if (channel == null)
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xIhExQ01", ErrorServices.severity.DB_Error);
                else
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xIhExQ01", channel, ErrorServices.severity.DB_Error);
            }

            return false;
        }

        /// <summary>
        /// Checks if the target exists where the target == value && target1 == value1
        /// </summary>
        /// <returns><c></returns>
        internal bool Contains(string table, ulong value, string target, ulong value1, string target1, ISocketMessageChannel channel = null)
        {
            long ID = Convert.ToInt64(value);
            long ID1 = Convert.ToInt64(value1);

            SqlCommand sqlCommand = new SqlCommand($"SELECT COUNT ({target}) FROM {table} WHERE {target} = {ID} AND {target1} = {ID1}", sqlConnection);

            try
            {
                sqlCommand.Connection.Open();
                int rows = (int)sqlCommand.ExecuteScalar();
                sqlCommand.Connection.Close();

                if (rows > 0)
                    return true;
            }
            catch (Exception ex)
            {
                if (channel != null)
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xConExQ02", channel, ErrorServices.severity.DB_Error);
                else
                    _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + sqlCommand.CommandText, "SQLs0xConExQ02", ErrorServices.severity.DB_Error);
            }

            return false;
        }

        /// <summary>
        /// Removes data from <c>table</c> in rows where <c>table == target</c>
        /// <para>Shouldn't accept user input</para>
        /// </summary>
        /// <returns> number of rows removed from <c>table</c> </returns>
        internal int RemoveData(string table, string target, string value, string aditionalCondition = null, ISocketMessageChannel channel = null)
        {
            int result = 0;

            using (SqlCommand command = new SqlCommand($"DELETE FROM {table} WHERE {target} = {value} {aditionalCondition}", sqlConnection))
            {
                command.Connection.Open();

                try
                {
                    result = command.ExecuteNonQuery();
                    command.Connection.Close();
                }
                catch (SqlException ex)
                {
                    if (channel != null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xReDtExQ", channel, ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xReDtExQ", ErrorServices.severity.DB_Error);
                }
            }

            return result;
        }

        /// <summary>
        /// Counts the number of rows inside specified table
        /// <para> Should not accept user input!!!</para>
        /// </summary>
        /// <returns><see cref="int"/> of rows affected</returns>
        internal int Count(string table, string target, ISocketMessageChannel channel = null)
        {
            using (SqlCommand command = new SqlCommand($"SELECT COUNT ({target}) FROM {table}", sqlConnection))
            {
                try
                {
                    command.Connection.Open();
                    int r = (int)command.ExecuteScalar();
                    command.Connection.Close();
                    return r;
                }
                catch (Exception ex)
                {
                    if (channel != null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xCntExQ", channel, ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xCntExQ", ErrorServices.severity.DB_Error);
                }
            }

            return 0;
        }

        /// <summary>
        /// Counts the number of <c>ServerId</c>s where the <c>where</c> = <c>condition</c> is true
        /// </summary>
        /// <returns><see cref="int"/> of above ^</returns>
        internal int Count(string table, string where, ulong condition, ISocketMessageChannel channel = null)
        {
            using (SqlCommand command = new SqlCommand($"SELECT COUNT (ServerID) FROM {table} WHERE {where} = {condition}", sqlConnection))
            {
                try
                {
                    command.Connection.Open();
                    int r = (int)command.ExecuteScalar();
                    command.Connection.Close();
                    return r;
                }
                catch (Exception ex)
                {
                    if (channel != null)
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xCntExQ", channel, ErrorServices.severity.DB_Error);
                    else
                        _ = es.SendErrorMessage(ex.Message + "\nCommandText: " + command.CommandText, "SQLs0xCntExQ", ErrorServices.severity.DB_Error);
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets a random [Text] row from <c>table</c> 
        /// <para> Table has to contain columns [Text] & [Id]</para>
        /// </summary>
        /// <returns></returns>
        internal string GetRandomString(string table)
        {
            string ad = "Ooops";
            int count = Count(table, "Id");
            Random r = new Random();

            using (SqlCommand command = new SqlCommand("", sqlConnection))
            {
                command.CommandText = $"SELECT Text FROM {table} WHERE Id = {r.Next(0, count-1)}";

                sqlConnection.Open();
                try
                {
                    ad = (string)command.ExecuteScalar();
                }
                catch(Exception ex)
                {
                    _ = es.SendErrorMessage(ex.Message, "SQLs0xGRScashisoverflow", ErrorServices.severity.DB_Error);
                }
                sqlConnection.Close();
            }

            return ad;
        }
    }
}
