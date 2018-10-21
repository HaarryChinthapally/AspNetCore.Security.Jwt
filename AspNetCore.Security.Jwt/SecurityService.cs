﻿namespace AspNetCore.Security.Jwt
{
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;

    /// <summary>
    /// SecurityService class
    /// </summary>
    public class SecurityService : ISecurityService
    {
        private readonly SecuritySettings securitySettings;

        public SecurityService(SecuritySettings securitySettings)
        {
            this.securitySettings = securitySettings;
        }
        
        /// <inheritdoc>
        public string GenerateToken(string seed)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.securitySettings.Secret));

            var idType = this.securitySettings.IdType;
            Claim claim;

            switch(idType)
            {
                case IdType.Name:
                    claim = new Claim(ClaimTypes.Name, seed);
                    break;
                case IdType.Email:
                    claim = new Claim(ClaimTypes.Email, seed);
                    break;
                default:
                    claim = new Claim(ClaimTypes.Name, seed);
                    break;
            }

            var claims = new Claim[] {
                claim,
                new Claim(JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds()}"),
                new Claim(JwtRegisteredClaimNames.Nbf, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}")
            };

            var token = new JwtSecurityToken(
                issuer: this.securitySettings.Issuer,
                audience: this.securitySettings.Audience,
                claims: claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddHours(this.securitySettings.TokenExpiryInHours ?? 1),
                signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
            );

            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return jwtToken;
        }
    }
}
