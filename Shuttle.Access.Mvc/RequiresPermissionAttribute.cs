﻿using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shuttle.Access.DataAccess;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Access.Mvc
{
    public class RequiresPermissionAttribute : TypeFilterAttribute
    {
        public RequiresPermissionAttribute(string permission) : base(typeof(RequiresPermission))
        {
            Arguments = new object[] {permission};
        }

        private class RequiresPermission : IAuthorizationFilter
        {
            private readonly IAccessConfiguration _configuration;
            private readonly IDatabaseContextFactory _databaseContextFactory;
            private readonly string _permission;
            private readonly ISessionQuery _sessionQuery;

            public RequiresPermission(IAccessConfiguration configuration,
                IDatabaseContextFactory databaseContextFactory, ISessionQuery sessionQuery,
                string permission)
            {
                Guard.AgainstNull(configuration, nameof(configuration));
                Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
                Guard.AgainstNull(sessionQuery, nameof(sessionQuery));
                Guard.AgainstNullOrEmptyString(permission, "permission");

                _configuration = configuration;
                _databaseContextFactory = databaseContextFactory;
                _sessionQuery = sessionQuery;
                _permission = permission;
            }

            public void OnAuthorization(AuthorizationFilterContext context)
            {
                var sessionTokenValue = context.HttpContext.GetAccessSessionToken();

                if (string.IsNullOrEmpty(sessionTokenValue))
                {
                    SetUnauthorized(context);
                    return;
                }

                if (!Guid.TryParse(sessionTokenValue, out var sessionToken))
                {
                    SetUnauthorized(context);
                    return;
                }

                using (_databaseContextFactory.Create(_configuration.ProviderName, _configuration.ConnectionString))
                {
                    if (!(_sessionQuery.Contains(sessionToken, _permission) || _sessionQuery.Contains(sessionToken, "*")))
                    {
                        SetUnauthorized(context);
                    }
                }
            }

            private static void SetUnauthorized(AuthorizationFilterContext context)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}