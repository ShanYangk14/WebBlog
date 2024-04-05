using Microsoft.EntityFrameworkCore.Migrations;

namespace WebBlog.Migrations
{
    public partial class UpdateDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "BlogPosts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId2",
                table: "BlogPosts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_UserId1",
                table: "BlogPosts",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogPosts_AspNetUsers_UserId1",
                table: "BlogPosts",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogPosts_AspNetUsers_UserId1",
                table: "BlogPosts");

            migrationBuilder.DropIndex(
                name: "IX_BlogPosts_UserId1",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "BlogPosts");

            migrationBuilder.DropColumn(
                name: "UserId2",
                table: "BlogPosts");
        }
    }
}
