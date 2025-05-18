using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobSearchApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangedMethodForIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vacancies_Embedding",
                table: "Vacancies");

            migrationBuilder.DropIndex(
                name: "IX_CandidateProfile_Embedding",
                table: "CandidateProfile");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_Embedding",
                table: "Vacancies",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" })
                .Annotation("Npgsql:StorageParameter:ef_construction", 80)
                .Annotation("Npgsql:StorageParameter:m", 16);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateProfile_Embedding",
                table: "CandidateProfile",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "hnsw")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" })
                .Annotation("Npgsql:StorageParameter:ef_construction", 80)
                .Annotation("Npgsql:StorageParameter:m", 16);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Vacancies_Embedding",
                table: "Vacancies");

            migrationBuilder.DropIndex(
                name: "IX_CandidateProfile_Embedding",
                table: "CandidateProfile");

            migrationBuilder.CreateIndex(
                name: "IX_Vacancies_Embedding",
                table: "Vacancies",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "ivfflat")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" })
                .Annotation("Npgsql:StorageParameter:lists", 100);

            migrationBuilder.CreateIndex(
                name: "IX_CandidateProfile_Embedding",
                table: "CandidateProfile",
                column: "Embedding")
                .Annotation("Npgsql:IndexMethod", "ivfflat")
                .Annotation("Npgsql:IndexOperators", new[] { "vector_cosine_ops" })
                .Annotation("Npgsql:StorageParameter:lists", 100);
        }
    }
}
