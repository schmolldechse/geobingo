using System;
using GeoBingo.Data.Entities.Authentication;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeoBingo.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:auth.user_roles", "ADMIN,USER");

            migrationBuilder.CreateTable(
                name: "auth_providers",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_key = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    display_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auth_providers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    handle = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    display_name = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    avatar_url = table.Column<string>(type: "text", nullable: true),
                    role = table.Column<UserRole>(type: "auth.user_roles", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_auth_identities",
                schema: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    auth_provider_id = table.Column<Guid>(type: "uuid", nullable: false),
                    external_subject = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_auth_identities", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_auth_identities_auth_providers_auth_provider_id",
                        column: x => x.auth_provider_id,
                        principalSchema: "auth",
                        principalTable: "auth_providers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_auth_identities_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "auth",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_auth_providers_provider_key",
                schema: "auth",
                table: "auth_providers",
                column: "provider_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_auth_identities_auth_provider_id",
                schema: "auth",
                table: "user_auth_identities",
                column: "auth_provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_auth_identities_user_id_auth_provider_id",
                schema: "auth",
                table: "user_auth_identities",
                columns: new[] { "user_id", "auth_provider_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                schema: "auth",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_handle",
                schema: "auth",
                table: "users",
                column: "handle",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_auth_identities",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "auth_providers",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "users",
                schema: "auth");
        }
    }
}
