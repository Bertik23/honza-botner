﻿using System.Collections.Generic;

#nullable disable
namespace HonzaBotner.Services.Contract.Dto
{
    public class DiscordRoleConfig
    {
        public const string ConfigName = "DiscordRoles";
        public Dictionary<string, ulong> RoleMapping { get; set; }
        public Dictionary<string, ulong> StaffRoleMapping { get; set; }
        public Dictionary<string, ulong> StudentRoleMapping { get; set; }
    }
}
