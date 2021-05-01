using System;
using System.Collections.Generic;
using System.Text;

namespace LeaveMeAlone
{
    public enum EErrorCode
    {
        OK = 0,
        GTA5_PATH_INVALID = 1,
        GUARDIAN_PATH_INVALID = 2,
        REQUIRES_ADMIN_RIGHTS = 3,
        FAILED_TO_CHANGE_WINDOWS_FIREWALL = 4,
        FAILED_TO_LOOKUP_REGISTRY_KEY = 5
    }
}
