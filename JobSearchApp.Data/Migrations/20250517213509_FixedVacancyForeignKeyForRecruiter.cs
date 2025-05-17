using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSearchApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedVacancyForeignKeyForRecruiter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vacancies_AspNetUsers_RecruiterId",
                table: "Vacancies");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancies_RecruiterProfile_RecruiterId",
                table: "Vacancies",
                column: "RecruiterId",
                principalTable: "RecruiterProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vacancies_RecruiterProfile_RecruiterId",
                table: "Vacancies");

            migrationBuilder.AddForeignKey(
                name: "FK_Vacancies_AspNetUsers_RecruiterId",
                table: "Vacancies",
                column: "RecruiterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
