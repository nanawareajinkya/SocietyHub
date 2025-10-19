using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SocietyHub.Application.Common.Features.Users.Interfaces;
using SocietyHub.Domain.Entities;
using System.Data;

namespace SocietyHub.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _conn;

        public UserRepository(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        // ✅ REGISTER
        public async Task<long> RegisterAsync(string username, byte[] passwordHash, byte[] passwordSalt, string? email, string? phone, string? roleCode)
        {
            using var sql = new SqlConnection(_conn);
            var p = new DynamicParameters();

            p.Add("@Proc_Option", "Register");
            p.Add("@Username", username);
            p.Add("@Email", email);
            p.Add("@Phone", phone);
            p.Add("@PasswordHash", passwordHash);
            p.Add("@PasswordSalt", passwordSalt);
            p.Add("@RoleCode", roleCode);

            p.Add("@OutStatus", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@OutMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

            var result = await sql.QueryFirstOrDefaultAsync<dynamic>(
                "[ACC].[User_Proc]", p, commandType: CommandType.StoredProcedure);

            var status = p.Get<int>("@OutStatus");
            var message = p.Get<string>("@OutMessage");
            if (status != 0)
                throw new Exception(message);

            // Read CreatedUserId from returned resultset (SP output)
            long userId = 0;
            if (result != null && result.CreatedUserId != null)
                userId = Convert.ToInt64(result.CreatedUserId);

            return userId;
        }

        // ✅ LOGIN
        public async Task<(int Status, string Message, byte[]? PasswordHash, byte[]? PasswordSalt, long? UserId)>
    LoginGetHashAsync(string username, string clientIp, string userAgent)
        {
            using var sql = new SqlConnection(_conn);
            var p = new DynamicParameters();

            // Input parameters
            p.Add("@Proc_Option", "Login");
            p.Add("@Username", username);
            p.Add("@ClientIP", clientIp);
            p.Add("@UserAgent", userAgent);

            // Required OUTPUT parameters
            p.Add("@OutStatus", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@OutMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

            // ✅ IMPORTANT: Always call with schema-qualified name
            var result = await sql.QueryFirstOrDefaultAsync<dynamic>(
                "[ACC].[User_Proc]", p, commandType: CommandType.StoredProcedure);

            // Retrieve OUTPUT values
            var status = p.Get<int>("@OutStatus");
            var message = p.Get<string>("@OutMessage");

            byte[]? hash = null;
            byte[]? salt = null;
            long? userId = null;

            if (result != null)
            {
                // Ensure keys match the columns you SELECT in your SP
                userId = result.user_id;
                hash = result.password_hash;
                salt = result.password_salt;
            }

            return (status, message, hash, salt, userId);
        }


        // ✅ GET BY ID
        public async Task<User?> GetByIdAsync(long userId)
        {
            using var sql = new SqlConnection(_conn);
            var p = new DynamicParameters();
            p.Add("@Proc_Option", "GetById");
            p.Add("@UserId", userId);

            var list = await sql.QueryAsync<User>(
                "[ACC].[User_Proc]", p, commandType: CommandType.StoredProcedure);

            return list.FirstOrDefault();
        }

        // ✅ GET BY USERNAME
        public async Task<User?> GetByUsernameAsync(string username)
        {
            using var sql = new SqlConnection(_conn);
            var p = new DynamicParameters();
            p.Add("@Proc_Option", "GetByUsername");
            p.Add("@Username", username);

            var list = await sql.QueryAsync<User>(
                "[ACC].[User_Proc]", p, commandType: CommandType.StoredProcedure);

            return list.FirstOrDefault();
        }

        // ✅ UPDATE PROFILE
        public async Task UpdateAsync(long userId, string? email, string? phone)
        {
            using var sql = new SqlConnection(_conn);
            var p = new DynamicParameters();

            p.Add("@Proc_Option", "Update");
            p.Add("@UserId", userId);
            p.Add("@Email", email);
            p.Add("@Phone", phone);

            p.Add("@OutStatus", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@OutMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

            await sql.ExecuteAsync("[ACC].[User_Proc]", p, commandType: CommandType.StoredProcedure);

            var status = p.Get<int>("@OutStatus");
            if (status != 0)
                throw new Exception(p.Get<string>("@OutMessage"));
        }

        // ✅ CHANGE PASSWORD
        public async Task ChangePasswordAsync(long userId, byte[] newHash, byte[] newSalt)
        {
            using var sql = new SqlConnection(_conn);
            var p = new DynamicParameters();

            p.Add("@Proc_Option", "ChangePassword");
            p.Add("@UserId", userId);
            p.Add("@NewPasswordHash", newHash);
            p.Add("@NewPasswordSalt", newSalt);

            p.Add("@OutStatus", dbType: DbType.Int32, direction: ParameterDirection.Output);
            p.Add("@OutMessage", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

            await sql.ExecuteAsync("[ACC].[User_Proc]", p, commandType: CommandType.StoredProcedure);

            var status = p.Get<int>("@OutStatus");
            if (status != 0)
                throw new Exception(p.Get<string>("@OutMessage"));
        }
    }
}
