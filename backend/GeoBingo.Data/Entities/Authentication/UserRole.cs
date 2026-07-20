using NpgsqlTypes;

namespace GeoBingo.Data.Entities.Authentication;

public enum UserRole
{
    [PgName("ADMIN")]
    Admin,

    [PgName("USER")]
    User
}
