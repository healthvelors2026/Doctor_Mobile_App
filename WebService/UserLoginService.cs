using DoctorMobileApp.CommonClass;
using DoctorMobileApp.Models;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace DoctorMobileApp.WebService
{
    public class UserLoginService
    {
        private readonly IDbConnectionFactory _dbHelper;
        private readonly IConfiguration _configuration;
        public UserLoginService(IDbConnectionFactory db, IConfiguration configuration)
        {
            _dbHelper = db;
            _configuration = configuration;
        }
        public async Task<LoginResponse?> LoginAsync(UserLoginRequest request)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@userName", request.username),
                new SqlParameter("@password", request.password) // ideally hashed
            };
            var dataTable = await _dbHelper.ExecuteDataTableAsync(
                "Api_UserLogin",
                CommandType.StoredProcedure,
                parameters
            );
            if (dataTable == null || dataTable.Rows.Count == 0)
                return null;
            var response = await GetLoginResponse(dataTable);
            if (response == null)
                return null;
            return response;
        }
        public async Task<bool> LogoutAsync(string RefreshToken)
        {
            if (string.IsNullOrWhiteSpace(RefreshToken))
                return false;
            var parameters = new SqlParameter[]
            {
               new SqlParameter("@Token", RefreshToken),
               new SqlParameter("@Message", SqlDbType.VarChar, 100)
               {
                    Direction = ParameterDirection.Output
               }
            };
            await _dbHelper.ExecuteNonQueryAsync(
                "API_SP_Logout",
                CommandType.StoredProcedure,
                parameters
            );
            var message = parameters[1].Value?.ToString();
            return message == "Logout successful";
        }
        public async Task<LoginResponse?> GetRefreshTokenAsync(string RefreshToken)
        {
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Token", RefreshToken)
            };
            var dataTable = await _dbHelper.ExecuteDataTableAsync(
                "API_SP_RefreshToken",
                CommandType.StoredProcedure,
                parameters
            );
            if (dataTable == null || dataTable.Rows.Count == 0)
                return null;
            var response = await GetLoginResponse(dataTable);
            if (response == null)
                return null;
            return response;
        }
        public async Task<LoginResponse?> GetLoginResponse(DataTable? dataTable)
        {
            if (dataTable == null || dataTable.Rows.Count == 0)
                return null;

            var row = dataTable.Rows[0];
            var user = new UserLogin
            {
                UserId = row.ToInt("UserIdp"),
                Username = row.ToStr("userName"),
                HospitalGroupIDF = row.ToInt("HospitalGroupIDP"),
                HospitalIDF = row.ToInt("hospitalidf"),
                HospitalName = row.ToStr("HospitalName"),
                HospitalCode = row.ToStr("HospitalCode"),
                EmployeeIDF = row.ToInt("EmployeeIDF"),
                EmployeeName = row.ToStr("EmployeeName")
            };
            user.Token = GenerateToken(user);
            var refreshToken = GenerateRefreshToken();
            var paraTokenSave = new SqlParameter[]
            {
               new SqlParameter("@ReferenceIDF", user.UserId),
               new SqlParameter("@ReferenceType", 2),
               new SqlParameter("@Token", refreshToken),
               new SqlParameter("@ExpiryDate", DateTime.Now.AddDays(7))
            };
            await _dbHelper.ExecuteNonQueryAsync(
                "API_Sp_InsertLoginToken",
                CommandType.StoredProcedure,
                paraTokenSave
            );
            return new LoginResponse
            {
                AccessToken = user.Token,
                RefreshToken = refreshToken,
                ExpiresInMinutes = Convert.ToInt32(_configuration["Jwt:DurationInMinutes"]),
            };
        }
        public string GenerateToken(UserLogin obj)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, obj.Username),
                new Claim("UserIdf", obj.UserId.ToString()),
                new Claim("HospitalGroupIDF", obj.HospitalGroupIDF.ToString()),
                new Claim("HospitalIDF", obj.HospitalIDF.ToString()),
                new Claim("HospitalName", obj.HospitalName ?? ""),
                new Claim("HospitalCode", obj.HospitalCode ?? ""),
                new Claim("EmployeeIDF", obj.EmployeeIDF.ToString()),
                new Claim("EmployeeName", obj.EmployeeName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
            );
            var duration = Convert.ToDouble(_configuration["Jwt:DurationInMinutes"]);
            obj.ExpireDate = DateTime.Now.AddMinutes(duration);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                notBefore: DateTime.Now,
                expires: obj.ExpireDate,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
