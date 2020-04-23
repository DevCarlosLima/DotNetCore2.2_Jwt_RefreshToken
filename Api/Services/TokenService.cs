using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Api.Models;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Api.Services {
    public class TokenService {
        private readonly IDistributedCache _dc;
        private readonly SigningConfigurations _sc;
        private readonly TokenConfigurations _tc;

        public TokenService (IDistributedCache dc, SigningConfigurations sc, TokenConfigurations tc) {
            _dc = dc;
            _sc = sc;
            _tc = tc;
        }

        public object New (string userId) {
            var claims = new List<Claim> {
                new Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid ().ToString ("N")),
                new Claim (JwtRegisteredClaimNames.UniqueName, userId)
            };

            var creationDate = DateTime.Now;
            var expirationDate = creationDate + TimeSpan.FromMinutes (_tc.Minutes);
            var finalExpiration = TimeSpan.FromHours (_tc.FinalExpiration);

            var securityToken = new JwtSecurityToken (
                _tc.Issuer,
                _tc.Audience,
                claims,
                creationDate,
                expirationDate,
                _sc.SigningCredentials
            );

            var token = new JwtSecurityTokenHandler ().WriteToken (securityToken);

            var result = new {
                userId = userId,
                created = creationDate.ToString ("yyyy-MM-dd HH:mm:ss"),
                expiration = expirationDate.ToString ("yyyy-MM-dd HH:mm:ss"),
                accessToken = token,
                refreshToken = NewRefreshToken ()
            };

            var refreshTokenData = new RefreshTokenData ();
            refreshTokenData.RefreshToken = result.refreshToken;
            refreshTokenData.UserID = userId;

            var cacheOptions = new DistributedCacheEntryOptions ();
            cacheOptions.SetAbsoluteExpiration (finalExpiration);
            _dc.SetString (result.refreshToken,
                JsonConvert.SerializeObject (refreshTokenData),
                cacheOptions
            );

            return result;
        }
        public object Refresh (RefreshCredentials model) {
            RefreshTokenData _rtd = null;

            var storedToken = _dc.GetString (model.RefreshToken);

            if (!string.IsNullOrEmpty (storedToken) && !string.IsNullOrWhiteSpace (storedToken))
                _rtd = JsonConvert.DeserializeObject<RefreshTokenData> (storedToken);

            var isValid = false;
            if (_rtd != null)
                isValid = (model.UserId == _rtd.UserID && model.RefreshToken == _rtd.RefreshToken);

            if (isValid) {
                _dc.Remove (model.RefreshToken);
                return New (model.UserId);
            }

            return null;
        }
        private string NewRefreshToken () {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create ()) {
                rng.GetBytes (randomNumber);
                return Convert.ToBase64String (randomNumber);
            }
        }
    }
}