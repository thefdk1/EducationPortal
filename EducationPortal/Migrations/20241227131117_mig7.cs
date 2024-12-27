using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EducationPortal.Migrations
{
    /// <inheritdoc />
    public partial class mig7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut rolleri güncelle
            migrationBuilder.Sql(@"
                UPDATE AspNetRoles 
                SET Name = CASE 
                    WHEN Name = 'Teacher' THEN 'Öğretmen'
                    WHEN Name = 'Student' THEN 'Öğrenci'
                    ELSE Name 
                END,
                NormalizedName = CASE 
                    WHEN NormalizedName = 'TEACHER' THEN 'ÖĞRETMEN'
                    WHEN NormalizedName = 'STUDENT' THEN 'ÖĞRENCİ'
                    ELSE NormalizedName 
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rolleri eski haline döndür
            migrationBuilder.Sql(@"
                UPDATE AspNetRoles 
                SET Name = CASE 
                    WHEN Name = 'Öğretmen' THEN 'Teacher'
                    WHEN Name = 'Öğrenci' THEN 'Student'
                    ELSE Name 
                END,
                NormalizedName = CASE 
                    WHEN NormalizedName = 'ÖĞRETMEN' THEN 'TEACHER'
                    WHEN NormalizedName = 'ÖĞRENCİ' THEN 'STUDENT'
                    ELSE NormalizedName 
                END
            ");
        }
    }
}
