﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shuttle.Access.Data;

#nullable disable

namespace Shuttle.Access.Data.Migrations
{
    [DbContext(typeof(AccessDbContext))]
    [Migration("20250329083705_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Shuttle.Access.Data.Models.Identity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("DateActivated")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("DateRegistered")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("GeneratedPassword")
                        .HasMaxLength(65)
                        .HasColumnType("nvarchar(65)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(320)
                        .HasColumnType("nvarchar(320)");

                    b.Property<string>("RegisteredBy")
                        .IsRequired()
                        .HasMaxLength(320)
                        .HasColumnType("nvarchar(320)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Name" }, "UX_Identity_Name")
                        .IsUnique();

                    b.ToTable("Identity");
                });

            modelBuilder.Entity("Shuttle.Access.Data.Models.IdentityRole", b =>
                {
                    b.Property<Guid>("IdentityId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("DateRegistered")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("IdentityId", "RoleId");

                    b.ToTable("IdentityRole");
                });

            modelBuilder.Entity("Shuttle.Access.Data.Models.Permission", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Name" }, "UX_Permission_Name")
                        .IsUnique();

                    b.ToTable("Permission");
                });

            modelBuilder.Entity("Shuttle.Access.Data.Models.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("Id");

                    b.HasIndex(new[] { "Name" }, "UX_Role_Name")
                        .IsUnique();

                    b.ToTable("Role");
                });

            modelBuilder.Entity("Shuttle.Access.Data.Models.RolePermission", b =>
                {
                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PermissionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("DateRegistered")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("RoleId", "PermissionId");

                    b.ToTable("RolePermission");
                });

            modelBuilder.Entity("Shuttle.Access.Data.Models.Session", b =>
                {
                    b.Property<Guid>("IdentityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("DateRegistered")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTimeOffset>("ExpiryDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("IdentityName")
                        .IsRequired()
                        .HasMaxLength(320)
                        .HasColumnType("nvarchar(320)");

                    b.Property<byte[]>("Token")
                        .IsRequired()
                        .HasColumnType("varbinary(900)");

                    b.HasKey("IdentityId");

                    b.HasIndex(new[] { "IdentityName" }, "UX_Session_IdentityName")
                        .IsUnique();

                    b.HasIndex(new[] { "Token" }, "UX_Session_Token")
                        .IsUnique();

                    b.ToTable("Session");
                });

            modelBuilder.Entity("Shuttle.Access.Data.Models.SessionPermission", b =>
                {
                    b.Property<Guid>("IdentityId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PermissionName")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.HasKey("IdentityId", "PermissionName");

                    b.ToTable("SessionPermission");
                });

            modelBuilder.Entity("Shuttle.Access.Data.Models.SessionTokenExchange", b =>
                {
                    b.Property<Guid>("ExchangeToken")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("ExpiryDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("SessionToken")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("ExchangeToken");

                    b.ToTable("SessionTokenExchange");
                });
#pragma warning restore 612, 618
        }
    }
}
