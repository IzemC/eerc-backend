using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace ENOC.Domain.Extensions
{
    public static class AccountManagementExtensions
    {
        public static string GetProperty(this Principal principal, string property)
        {
            DirectoryEntry directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
            if (directoryEntry.Properties.Contains(property))
                return directoryEntry.Properties[property].Value.ToString();
            else
                return string.Empty;
        }

        public static string GetJob(this Principal principal)
        {
            return principal.GetProperty("title");
        }

        public static string GetDepartment(this Principal principal)
        {
            return principal.GetProperty("department");
        }
    }
}
